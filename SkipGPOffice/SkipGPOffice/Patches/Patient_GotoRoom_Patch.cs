using System;
using Harmony12;
using TH20;

namespace SkipGPOffice.Patches
{
    [HarmonyPatch(typeof(Patient), "GotoRoom")]
    public class Patient_GotoRoom_Patch
    {
        /// <summary>
        /// Send the patients directly to the treatment room, if the diagnosis certainty level is higher than the
        /// given limit.
        /// </summary>
        /// <param name="__instance">The instance.</param>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        private static bool Prefix(Patient __instance, ReasonUseRoom reason)
        {
            if (!Program.Enabled)
                return true;

            try
            {
                if (reason != ReasonUseRoom.Diagnosis || __instance.DiagnosisCertainty < Program.Settings.DiagnosisCertaintyLevel)
                    return true;

                var researchManager = __instance.Level.ResearchManager;
                var treatmentRoom = __instance.Illness.GetTreatmentRoom(__instance, researchManager);

                if (Program.Settings.EnableLogging)
                    Program.Logger.Log($"{DateTime.Now} Patient: {__instance.CharacterName.GetCharacterName()} with {__instance.DiagnosisCertainty}% diagnosis certainty! Redirect to {treatmentRoom.LocalisedName.Translation}!");

                __instance.SendToTreatmentRoom(treatmentRoom, true);
                return false;
            }
            catch (Exception ex)
            {
                Program.Logger.Error(ex.ToString());
            }

            return true;
        }
    }
}