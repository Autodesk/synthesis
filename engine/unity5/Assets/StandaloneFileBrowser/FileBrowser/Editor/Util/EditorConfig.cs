using UnityEngine;
using UnityEditor;

namespace Crosstales.FB.EditorUtil
{
    /// <summary>Editor configuration for the asset.</summary>
    [InitializeOnLoad]
    public static class EditorConfig
    {

        #region Variables

        /// <summary>Enable or disable update-checks for the asset.</summary>
        public static bool UPDATE_CHECK = EditorConstants.DEFAULT_UPDATE_CHECK;

        /// <summary>Open the UAS-site when an update is found.</summary>
        public static bool UPDATE_OPEN_UAS = EditorConstants.DEFAULT_UPDATE_OPEN_UAS;

        /// <summary>Enable or disable reminder-checks for the asset.</summary>
        public static bool REMINDER_CHECK = EditorConstants.DEFAULT_REMINDER_CHECK;

        /// <summary>Enable or disable CT reminder-checks for the asset.</summary>
        public static bool CT_REMINDER_CHECK = EditorConstants.DEFAULT_CT_REMINDER_CHECK;
        
        /// <summary>Enable or disable anonymous telemetry data.</summary>
        public static bool TELEMETRY = EditorConstants.DEFAULT_TELEMETRY;

        /// <summary>Is the configuration loaded?</summary>
        public static bool isLoaded = false;

        #endregion


        #region Constructor

        static EditorConfig()
        {
            if (!isLoaded)
            {
                Load();
            }
        }

        #endregion


        #region Public static methods

        /// <summary>Resets all changable variables to their default value.</summary>
        public static void Reset()
        {
            UPDATE_CHECK = EditorConstants.DEFAULT_UPDATE_CHECK;
            UPDATE_OPEN_UAS = EditorConstants.DEFAULT_UPDATE_OPEN_UAS;
            REMINDER_CHECK = EditorConstants.DEFAULT_REMINDER_CHECK;
            CT_REMINDER_CHECK = EditorConstants.DEFAULT_CT_REMINDER_CHECK;
            TELEMETRY = EditorConstants.DEFAULT_TELEMETRY;
        }

        /// <summary>Loads the all changable variables.</summary>
        public static void Load()
        {
            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_UPDATE_CHECK))
            {
                UPDATE_CHECK = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_UPDATE_CHECK);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_UPDATE_OPEN_UAS))
            {
                UPDATE_OPEN_UAS = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_UPDATE_OPEN_UAS);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_REMINDER_CHECK))
            {
                REMINDER_CHECK = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_REMINDER_CHECK);
            }

            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_CT_REMINDER_CHECK))
            {
                CT_REMINDER_CHECK = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_CT_REMINDER_CHECK);
            }
            
            if (Common.Util.CTPlayerPrefs.HasKey(EditorConstants.KEY_TELEMETRY))
            {
                TELEMETRY = Common.Util.CTPlayerPrefs.GetBool(EditorConstants.KEY_TELEMETRY);
            }

            isLoaded = true;
        }

        /// <summary>Saves the all changable variables.</summary>
        public static void Save()
        {
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_UPDATE_CHECK, UPDATE_CHECK);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_UPDATE_OPEN_UAS, UPDATE_OPEN_UAS);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_REMINDER_CHECK, REMINDER_CHECK);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_CT_REMINDER_CHECK, CT_REMINDER_CHECK);
            Common.Util.CTPlayerPrefs.SetBool(EditorConstants.KEY_TELEMETRY, TELEMETRY);

            Common.Util.CTPlayerPrefs.Save();
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)