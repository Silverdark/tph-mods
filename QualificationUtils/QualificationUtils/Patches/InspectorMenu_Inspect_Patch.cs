using System;
using Harmony12;
using TH20;

namespace QualificationUtils.Patches
{
    [HarmonyPatch(typeof(InspectorMenu), "Inspect", typeof(Character))]
    public class InspectorMenu_Inspect_Patch
    {
        #region Properties

        public static Staff SelectedStaff { get; private set; }

        #endregion

        #region Methods

        private static void Postfix(Character character)
        {
            if (!Program.Enabled)
                return;

            if (character == null || !(character is Staff staff))
                return;

            SelectedStaff = staff;
        }

        #endregion
    }
}