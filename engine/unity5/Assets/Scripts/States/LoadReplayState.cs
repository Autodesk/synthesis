using Synthesis.FSM;
using Synthesis.GUI.Scrollables;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.States
{
    public class LoadReplayState : State
    {
        private GameObject splashScreen;

        /// <summary>
        /// Initializes references to requried <see cref="GameObject"/>s.
        /// </summary>
        public override void Start()
        {
            splashScreen = Auxiliary.FindGameObject("LoadSplash");
        }

        /// <summary>
        /// Pops the current <see cref="State"/> when the back button is pressed.
        /// </summary>
        public void OnBackButtonClicked()
        {
            StateMachine.PopState();
        }

        /// <summary>
        /// Deletes the selected replay when the delete button is pressed.
        /// </summary>
        public void OnDeleteButtonClicked()
        {
            GameObject replayList = GameObject.Find("SimLoadReplayList");
            string entry = replayList.GetComponent<ScrollableList>().selectedEntry;

            if (entry != null)
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Replays\" + entry + ".replay");
                replayList.SetActive(false);
                replayList.SetActive(true);
            }
        }

        /// <summary>
        /// Launches the selected replay when the launch replay button is pressed.
        /// </summary>
        public void OnLaunchButtonClicked()
        {
            GameObject replayList = GameObject.Find("SimLoadReplayList");
            string entry = replayList.GetComponent<ScrollableList>().selectedEntry;

            if (entry != null)
            {
                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                    AnalyticsLedger.TimingVarible.Viewing,
                    AnalyticsLedger.TimingLabel.ReplayMode);

                splashScreen.SetActive(true);
                PlayerPrefs.SetString("simSelectedReplay", entry);
                PlayerPrefs.Save();
                SceneManager.LoadScene("Scene");
            }

            replayList.SetActive(false);
        }
    }
}
