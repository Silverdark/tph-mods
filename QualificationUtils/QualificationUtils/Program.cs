﻿using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using QualificationUtils.Patches;
using TH20;
using UnityEngine;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace QualificationUtils
{
    internal static class Program
    {
        #region Properties

        public static bool Enabled { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        #endregion

        #region Fields

        private static Vector2 _scrollPosition;

        #endregion

        #region Methods

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            return true;
        }

        #endregion

        #region Event handler

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                var staff = InspectorMenu_Inspect_Patch.SelectedStaff;

                if (staff == null)
                {
                    GUILayout.Label("No staff selected");
                    return;
                }

                GUILayout.Label($"Selected: {staff.NameWithTitle}");

                PrintSetRank(staff);
                PrintRemoveQualifications(staff);
                PrintAddQualifications(staff);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        #endregion

        #region GUI Helper

        private static void PrintSetRank(Staff staff)
        {
            GUILayout.Label($"Set rank (Current: {staff.Rank + 1})", UnityModManager.UI.h2);

            GUILayout.BeginHorizontal();

            var labelStyle = new GUIStyle(GUI.skin.GetStyle("Label")) { alignment = TextAnchor.MiddleCenter };

            for (var i = 1; i <= 5; i++)
            {
                if (i >= staff.Qualifications.Count && staff.Rank + 1 != i)
                {
                    if (GUILayout.Button("" + i, GUILayout.Width(50f)))
                    {
                        staff.SetRank(i - 1);
                        staff.SetSalary(staff.GetDesiredSalary(), false);
                    }
                }
                else
                    GUILayout.Label("" + i, labelStyle, GUILayout.Width(50f));
            }

            GUILayout.EndHorizontal();
        }

        private static void PrintRemoveQualifications(Staff staff)
        {
            GUILayout.Label("Remove qualification", UnityModManager.UI.h2);

            if (staff.Qualifications.Count <= 0)
            {
                GUILayout.Label("No qualification found.");
                return;
            }

            GUILayout.BeginHorizontal();

            var qualifications = staff.Qualifications.ToList();
            var allRequiredQualifications = qualifications
                .Select(slot => slot.Definition.RequiredQualifications)
                .SelectMany(instances => instances)
                .Select(instance => (QualificationDefinition) instance.GetInstance)
                .ToArray();

            foreach (var qualification in qualifications)
            {
                string qualificationName = qualification.Definition.NameLocalised.Translation;

                if (allRequiredQualifications.Any(definition => definition == qualification.Definition))
                    GUILayout.Label(qualificationName, GUILayout.ExpandWidth(false));
                else
                {
                    if (GUILayout.Button(qualificationName, GUILayout.ExpandWidth(false)))
                    {
                        staff.Qualifications.Remove(qualification);
                        staff.ModifiersComponent?.RemoveModifiers(qualification.Definition.Modifiers);
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void PrintAddQualifications(Staff staff)
        {
            GUILayout.Label("Add qualification", UnityModManager.UI.h2);

            if (staff.NumFreeQualificationSlots == 0)
            {
                GUILayout.Label("No qualification slots available.");
                return;
            }

            var level = GetCurrentLoadedLevel();
            var allAvailableQualificationsDefinitions = level.JobApplicantManager.Qualifications.List.Keys;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150f));

            foreach (var definition in allAvailableQualificationsDefinitions)
            {
                if (!definition.ValidFor(staff))
                    continue;

                if (GUILayout.Button(definition.NameLocalised.Translation, GUILayout.Width(150f)))
                {
                    staff.Qualifications.Add(new QualificationSlot(definition, true));
                    staff.ModifiersComponent?.AddModifiers(definition.Modifiers);
                }
            }

            GUILayout.EndScrollView();
        }

        #endregion

        #region Helper methods

        private static Level GetCurrentLoadedLevel()
        {
            var mainScript = Object.FindObjectOfType<MainScript>();
            if (!mainScript)
                return default;

            var app = Traverse.Create(mainScript).Field("_app").GetValue<App>();
            if (app == null || app.Level == null)
                return default;

            return app.Level;
        }

        #endregion
    }
}