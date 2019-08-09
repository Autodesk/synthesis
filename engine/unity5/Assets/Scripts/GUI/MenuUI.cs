using UnityEngine;
using Synthesis.FSM;
using Synthesis.States;

namespace Synthesis.GUI
{
    /// <summary>
    /// MenuUI serves as an interface between the Unity button UI and the various functions within the settings menu.
    /// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
    /// </summary>
    public class MenuUI : LinkedMonoBehaviour<MainState>
    {
        GameObject canvas;

        // Robot controls
        // Global Controls
        // Settings
        // View Replays
        // Help

        private void OnGUI()
        {
            UserMessageManager.Render();
        }

        /// <summary>
        /// Finds all the necessary UI elements that need to be updated/modified
        /// </summary>
        private void FindElements()
        {
            canvas = GameObject.Find("Canvas");
        }

        #region robot controls



        #endregion
        #region global controls



        #endregion
        #region settings



        #endregion
        #region view replays



        #endregion
        #region help



        #endregion

        /// <summary>
        /// Call this function whenever the user enters a new state
        /// </summary>
        public void EndOtherProcesses()
        {

        }
        
    }
}