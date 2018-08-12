namespace Crosstales.FB.Util
{
    /// <summary>Collected constants of very general utility for the asset.</summary>
    public abstract class Constants : Common.Util.BaseConstants
    {

        #region Constant variables

        /// <summary>Is PRO-version?</summary>
        public static readonly bool isPro = false;

        /// <summary>Name of the asset.</summary>
        public const string ASSET_NAME = "File Browser";
        //public const string ASSET_NAME = "File Browser PRO";

        /// <summary>Version of the asset.</summary>
        public const string ASSET_VERSION = "1.2.2";

        /// <summary>Build number of the asset.</summary>
        public const int ASSET_BUILD = 20180607;

        /// <summary>Create date of the asset (YYYY, MM, DD).</summary>
        public static readonly System.DateTime ASSET_CREATED = new System.DateTime(2017, 8, 1);

        /// <summary>Change date of the asset (YYYY, MM, DD).</summary>
        public static readonly System.DateTime ASSET_CHANGED = new System.DateTime(2018, 6, 7);

        /// <summary>URL of the PRO asset in UAS.</summary>
        public const string ASSET_PRO_URL = "https://www.assetstore.unity3d.com/#!/content/98713?aid=1011lNGT&pubref=" + ASSET_NAME;

        /// <summary>URL for update-checks of the asset</summary>
        public const string ASSET_UPDATE_CHECK_URL = "https://www.crosstales.com/media/assets/fb_versions.txt";

        /// <summary>Contact to the owner of the asset.</summary>
        public const string ASSET_CONTACT = "fb@crosstales.com";

        // Keys for the configuration of the asset
        public const string KEY_PREFIX = "FILEBROWSER_CFG_";
        public const string KEY_DEBUG = KEY_PREFIX + "DEBUG";

        #endregion

    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)