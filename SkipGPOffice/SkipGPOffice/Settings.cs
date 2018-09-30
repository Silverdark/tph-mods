using UnityModManagerNet;

namespace SkipGPOffice
{
    public class Settings : UnityModManager.ModSettings
    {
        #region Properties

        public int DiagnosisCertaintyLevel = 90;
        public bool EnableLogging = false;

        #endregion

        #region Methods

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        #endregion
    }
}