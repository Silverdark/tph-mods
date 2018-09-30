using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using TH20;
using UnityEngine;
using UnityModManagerNet;

namespace EpidemicHelper
{
    internal static class Program
    {
        #region Properties

        public static bool Enabled { get; private set; } = true;
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

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
            GUILayout.Label("Actions", UnityModManager.UI.h2);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Log current infected characters ", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Log", GUILayout.Width(150f)))
                LogInfectedCharacters();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Highlight current infected characters ", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Highlight", GUILayout.Width(150f)))
                HighlightInfectedCharacters();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Vaccinate all infected characters ", GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Vaccinate all", GUILayout.Width(150f)))
                VaccinateInfectedCharacters();
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Actions

        private static void LogInfectedCharacters()
        {
            Logger.Log("Infected players:");

            foreach (var infectedCharacter in GetInfectedCharacters())
                Logger.Log(infectedCharacter.CharacterName.Translate());
        }

        private static void HighlightInfectedCharacters()
        {
            foreach (var infectedCharacter in GetInfectedCharacters())
            {
                var component = infectedCharacter.GameObject.GetComponent<AnimationParticleEventListener>();
                if (component != null)
                    component.SpawnFX("InjectC");
            }
        }

        private static void VaccinateInfectedCharacters()
        {
            var level = GetCurrentLoadedLevel();

            var challengesOfType = level.ChallengeManager.GetActiveChallengesOfType<ChallengeEpidemic>();
            if (challengesOfType.Count != 1)
                return;

            var epidemicChallenge = challengesOfType[0];

            if (epidemicChallenge.ChallengeStatus != Challenge.ChallengeState.InProgress)
                return;

            foreach (var infectedCharacter in GetInfectedCharacters())
            {
                if (!epidemicChallenge.VaccinesAvailable())
                    break;

                epidemicChallenge.VaccinateCharacter(infectedCharacter);
            }
        }

        #endregion

        #region Helper methods

        private static IEnumerable<Character> GetInfectedCharacters()
        {
            return GetCurrentLoadedLevel().CharacterManager.AllCharacters.Where(IsInfected);
        }

        private static bool IsInfected(Character character)
        {
            if (character.ModifiersComponent == null)
                return false;

            return character.ModifiersComponent.Modifiers.Any(m => m.GetType() == typeof(CharacterModifierInfected));
        }

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