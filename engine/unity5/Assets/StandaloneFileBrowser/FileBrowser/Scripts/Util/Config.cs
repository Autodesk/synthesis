namespace Crosstales.FB.Util
{
    /// <summary>Configuration for the asset.</summary>
    public static class Config
    {

        #region Changable variables

        /// <summary>Enable or disable debug logging for the asset.</summary>
        public static bool DEBUG = Constants.DEFAULT_DEBUG;

        /// <summary>Is the configuration loaded?</summary>
        public static bool isLoaded = false;

        #endregion


        #region Public static methods

        /// <summary>Resets all changable variables to their default value.</summary>
        public static void Reset()
        {
            DEBUG = Constants.DEFAULT_DEBUG;
        }

        /// <summary>Loads the all changable variables.</summary>
        public static void Load()
        {
            if (Common.Util.CTPlayerPrefs.HasKey(Constants.KEY_DEBUG))
            {
                DEBUG = Common.Util.CTPlayerPrefs.GetBool(Constants.KEY_DEBUG);
            }

            isLoaded = true;
        }

        /// <summary>Saves the all changable variables.</summary>
        public static void Save()
        {
            Common.Util.CTPlayerPrefs.SetBool(Constants.KEY_DEBUG, DEBUG);

            Common.Util.CTPlayerPrefs.Save();
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)