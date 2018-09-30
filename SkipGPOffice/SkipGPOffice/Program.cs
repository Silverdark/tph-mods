using System.Reflection;
using Harmony12;
using UnityEngine;
using UnityModManagerNet;

namespace SkipGPOffice
{
    internal static class Program
    {
        #region Properties

        public static bool Enabled { get; private set; }
        public static Settings Settings { get; private set; }
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        #endregion

        #region Methods

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            Settings.DiagnosisCertaintyLevel = Mathf.Clamp(Settings.DiagnosisCertaintyLevel, 0, 100);

            Logger = modEntry.Logger;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
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
            GUILayout.BeginHorizontal();
            GUILayout.Label("Skip GPOffice by diagnosis certainty level (0-100) ", GUILayout.ExpandWidth(false));
            string text = Settings.DiagnosisCertaintyLevel.ToString();
            string inputText = GUILayout.TextField(text, 3, GUILayout.Width(50f));

            if (inputText != text && int.TryParse(inputText, out int result))
                Settings.DiagnosisCertaintyLevel = Mathf.Clamp(result, 0, 100);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Enable logging ", GUILayout.ExpandWidth(false));
            Settings.EnableLogging = (GUILayout.Toggle((Settings.EnableLogging ? 1 : 0) != 0, "", GUILayout.ExpandWidth(false)) ? 1 : 0) != 0;
            GUILayout.EndHorizontal();
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }

        #endregion
    }
}