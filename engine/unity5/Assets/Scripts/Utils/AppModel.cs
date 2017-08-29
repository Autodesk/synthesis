using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// This class is used to store global variables, which carry over from scene to scene.
    /// </summary>
    public static class AppModel
    {
        private static bool errorDefined;

        /// <summary>
        /// The error message to display when returning to the main menu.
        /// </summary>
        public static string ErrorMessage { get; private set; }

        /// <summary>
        /// Determines if this is the first time loading the main menu scene.
        /// </summary>
        public static bool InitialLaunch { get; set; }
        
        /// <summary>
        /// Initializes the static AppModel instance.
        /// </summary>
        static AppModel()
        {
            InitialLaunch = true;
        }

        /// <summary>
        /// Clears the error message is one is set.
        /// </summary>
        public static void ClearError()
        {
            errorDefined = false;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Returns to the menu and displays the given error message.
        /// </summary>
        /// <param name="message"></param>
        public static void ErrorToMenu(string message)
        {
            if (!errorDefined)
            {
                errorDefined = true;
                ErrorMessage = message;
                SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
