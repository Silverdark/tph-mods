using Harmony12;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TH20;
using UnityEngine;
using UnityModManagerNet;

namespace EpidemicHelper
{
#if DEBUG

    [EnableReloading]
#endif
    internal static class Program
    {
        #region Fields

        #region Infection

        private static float _infectionHighlightTimer = 0f;
        private static bool _infectionHighlightEnabled = false;
        private static Vector2 _infectedScrollPosition;

        #endregion

        #region Aliens

        private static float _alienHighlightTimer = 0f;
        private static bool _alienHighlightEnabled = false;
        private static Vector2 _alienScrollPosition;

        #endregion

        #endregion

        #region Properties

        public static bool Enabled { get; private set; } = true;
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        #endregion

        #region Methods

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).PatchAll(Assembly.GetExecutingAssembly());

            Logger = modEntry.Logger;

            modEntry.OnUpdate = OnUpdate;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;

#if DEBUG
            modEntry.OnUnload += OnUnload;
#endif
            return true;
        }

        #endregion

        #region Event handler

        private static void OnUpdate(UnityModManager.ModEntry arg1, float arg2)
        {
            if (_infectionHighlightEnabled)
            {
                if (_infectionHighlightTimer <= 0)
                {
                    _infectionHighlightTimer = 2.5f;
                    HighlightInfectedCharacters();
                }
                else
                {
                    _infectionHighlightTimer -= Time.deltaTime;
                }
            }

            if (_alienHighlightEnabled)
            {
                if (_alienHighlightTimer <= 0)
                {
                    _alienHighlightTimer = 2.5f;
                    HighlightAliens();
                }
                else
                {
                    _alienHighlightTimer -= Time.deltaTime;
                }
            }
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!Enabled)
                return;

            {
                var infectedEnabled = IsInfectionInProgress(out _);
                var infectedEnabledStr = infectedEnabled ? "Enabled" : "Disabled";
                GUILayout.Label($"Infection ({infectedEnabledStr})", UnityModManager.UI.h2);

                if (infectedEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Log infected names ", GUILayout.ExpandWidth(false));
                    if (GUILayout.Button("Log", GUILayout.Width(150f)))
                        LogInfectedCharacters();
                    GUILayout.EndHorizontal();

                    _infectionHighlightEnabled = GUILayout.Toggle(_infectionHighlightEnabled, "Highlight infected characters");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Vaccinate everyone ", GUILayout.ExpandWidth(false));
                    if (GUILayout.Button("Vaccinate", GUILayout.Width(150f)))
                        VaccinateInfectedCharacters();
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Vaccinate single character", UnityModManager.UI.h2);
                    _infectedScrollPosition = GUILayout.BeginScrollView(_infectedScrollPosition, GUILayout.Height(150f));
                    foreach (var character in GetInfectedCharacters())
                        if (GUILayout.Button(character.CharacterName.GetCharacterName(), GUILayout.Width(300f)))
                            VaccinateInfectedCharacter(character);
                    GUILayout.EndScrollView();
                }
            }

            {
                var aliensEnabled = IsAlienEnabled();
                var aliensEnabledStr = aliensEnabled ? "Enabled" : "Disabled";
                GUILayout.Label($"Aliens ({aliensEnabledStr})", UnityModManager.UI.h2);

                if (aliensEnabled)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Log current aliens ", GUILayout.ExpandWidth(false));
                    if (GUILayout.Button("Log", GUILayout.Width(150f)))
                        LogAlienPatients();
                    GUILayout.EndHorizontal();

                    _alienHighlightEnabled = GUILayout.Toggle(_alienHighlightEnabled, "Highlight undiscovered aliens");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Expose all undiscovered aliens ", GUILayout.ExpandWidth(false));
                    if (GUILayout.Button("Expose", GUILayout.Width(150f)))
                        ExposeAliens();
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Expose undiscovered alien", UnityModManager.UI.h2);
                    _alienScrollPosition = GUILayout.BeginScrollView(_alienScrollPosition, GUILayout.Height(150f));
                    foreach (var alien in GetUndiscoveredAliens())
                        if (GUILayout.Button(alien.CharacterName.GetCharacterName(), GUILayout.Width(300f)))
                            ExposeAlien(alien);
                    GUILayout.EndScrollView();
                }
            }
        }

#if DEBUG

        private static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.Create(modEntry.Info.Id).UnpatchAll();
            Enabled = false;
            return true;
        }

#endif

        #endregion

        #region Actions

        #region Infection

        private static void LogInfectedCharacters()
        {
            Logger.Log("Infected players:");

            foreach (var infectedCharacter in GetInfectedCharacters())
                Logger.Log($" - {infectedCharacter.CharacterName.GetCharacterName()}");
        }

        private static void HighlightInfectedCharacters()
        {
            GetInfectedCharacters().ForEach(HighlightCharacter);
        }

        private static void VaccinateInfectedCharacters()
        {
            foreach (var infectedCharacter in GetInfectedCharacters())
                VaccinateInfectedCharacter(infectedCharacter);
        }

        private static void VaccinateInfectedCharacter(Character character)
        {
            if (!IsInfectionInProgress(out var epidemicChallenge))
                return;

            if (!epidemicChallenge.VaccinesAvailable())
                return;

            Logger.Log($"Vaccinate {character.CharacterName.GetCharacterName()}");
            epidemicChallenge.VaccinateCharacter(character);
        }

        #endregion

        #region Aliens

        private static void LogAlienPatients()
        {
            var aliensManager = GetAliensManager();
            if (aliensManager == null)
                return;

            Logger.Log($"Alien patients (Discovered {aliensManager.NumAliensDiscovered} of {aliensManager.NumAliens})");
            foreach (var alienPatient in aliensManager.Aliens)
            {
                var discovered = alienPatient.GetComponent<AlienComponent>().Discovered;
                var discoveredStr = discovered ? "Discovered" : "Not discovered";

                Logger.Log($" - {alienPatient.CharacterName.GetCharacterName()} - {discoveredStr}");
            }
        }

        private static void HighlightAliens()
        {
            GetUndiscoveredAliens().ForEach(HighlightCharacter);
        }

        private static void ExposeAliens()
        {
            foreach (var alien in GetUndiscoveredAliens())
            {
                Logger.Log($"Expose alien {alien.CharacterName.GetCharacterName()}");
                ExposeAlien(alien);
            }
        }

        private static void ExposeAlien(Entity alien)
        {
            var alienComponent = alien.GetComponent<AlienComponent>();
            if (alienComponent == null || alienComponent.Discovered)
                return;

            alienComponent.SetDiscoveredPending();
        }

        #endregion

        #endregion

        #region Helper methods

        private static Level GetCurrentLoadedLevel()
        {
            var mainScript = Object.FindObjectOfType<MainScript>();
            if (!mainScript)
                return default;

            var app = Traverse.Create(mainScript).Field("_app").GetValue<App>();
            return app?.Level;
        }

        #region Infection

        private static void HighlightCharacter(Character character)
        {
            var component = character.GameObject.GetComponent<AnimationParticleEventListener>();
            if (component != null)
            {
                component.SpawnFX("InjectC");
            }
        }

        private static List<Character> GetInfectedCharacters()
        {
            return GetCurrentLoadedLevel().CharacterManager.AllCharacters.Where(IsInfected).ToList();
        }

        private static bool IsInfectionInProgress(out ChallengeEpidemic epidemicChallenge)
        {
            epidemicChallenge = null;
            var level = GetCurrentLoadedLevel();
            if (level == null)
                return false;

            var challengesOfType = level.ChallengeManager.GetActiveChallengesOfType<ChallengeEpidemic>();
            if (challengesOfType.Count != 1)
                return false;

            epidemicChallenge = challengesOfType[0];

            return epidemicChallenge.ChallengeStatus == Challenge.ChallengeState.InProgress;
        }

        private static bool IsInfected(Character character)
        {
            if (character.ModifiersComponent == null)
                return false;

            return character.ModifiersComponent.Modifiers.Any(m => m.GetType() == typeof(CharacterModifierInfected));
        }

        #endregion

        #region Aliens

        private static AliensManager GetAliensManager()
        {
            return GetCurrentLoadedLevel()?.CharacterManager?.GetAliensManager();
        }

        private static bool IsAlienEnabled()
        {
            return GetAliensManager() != null;
        }

        private static List<Patient> GetAliens()
        {
            return GetAliensManager()?.Aliens ?? new List<Patient>();
        }

        private static List<Patient> GetUndiscoveredAliens()
        {
            return GetAliens().Where(IsAlienDiscovered).ToList();
        }

        private static bool IsAlienDiscovered(Entity alien)
        {
            var alienComponent = alien.GetComponent<AlienComponent>();
            return alienComponent != null && alienComponent.Discovered;
        }

        #endregion

        #endregion
    }
}