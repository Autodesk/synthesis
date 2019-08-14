using Synthesis.FSM;
using Synthesis.GUI.Scrollables;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Synthesis.GUI;
using Synthesis.FEA;
using Button = UnityEngine.UI.Button;

namespace Synthesis.States
{
    public class LoadReplay : MonoBehaviour
    {
        GameObject mainCam;
        GameObject canvas;
        GameObject loadingPanel;

        GameObject loadReplayPanel;

        Button deleteButton;
        Button cancelButton;
        Button launchButton;

        MainState mainState;

        /// <summary>
        /// Initializes references to requried <see cref="GameObject"/>s.
        /// </summary>
        public void Start()
        {
            mainCam = GameObject.Find("Main Camera");
            canvas = GameObject.Find("Canvas");
            loadingPanel = Auxiliary.FindObject(mainCam, "LoadingPanel");

            loadReplayPanel = Auxiliary.FindObject(canvas, "LoadReplayPanel");

            GameObject replayList = GameObject.Find("SimLoadReplayList");
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + "Replays";

            deleteButton = Auxiliary.FindObject(canvas, "DeleteButton").GetComponent<Button>();
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(DeleteReplay);

            //cancelButton = Auxiliary.FindObject(canvas, "CancelButton").GetComponent<Button>();
            //cancelButton.onClick.RemoveAllListeners();
            //cancelButton.onClick.AddListener(OnCancelButtonClicked);

            //launchButton = GameObject.Find("LaunchButton").GetComponent<Button>();
            //launchButton.onClick.RemoveAllListeners();
            //launchButton.onClick.AddListener(OnLaunchButtonClicked);

            loadReplayPanel.SetActive(true);
            DynamicCamera.ControlEnabled = false;
        }

        public void OnEnable()
        {
            this.Start();
        }

        /// <summary>
        /// Renders the UserMessageManager.
        /// </summary>
        public void OnGUI()
        {
            UserMessageManager.Render();
        }

        /// <summary>
        /// Disables the canvas and re-enables camera movement.
        /// </summary>
        public void End()
        {
            //canvas.SetActive(false);
            DynamicCamera.ControlEnabled = true;
        }

        /// <summary>
        /// Deletes the selected replay when the delete button is pressed.
        /// </summary>
        public void DeleteReplay()
        {
            GameObject replayList = GameObject.Find("SimLoadReplayList");
            string entry = replayList.GetComponent<LoadReplayScrollable>().selectedEntry;

            if (entry != null && entry.EndsWith(".replay"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Replays\" + entry);
                replayList.SetActive(false);
                replayList.SetActive(true);
            }
        }

        /// <summary>
        /// Pops the current <see cref="State"/> when the back button is pressed.
        /// </summary>
        //public void OnCancelButtonClicked()
        //{
        //    StateMachine.PopState();
        //}

        /// <summary>
        /// Launches the selected replay when the launch replay button is pressed.
        /// </summary>
        public void LaunchReplay()
        {
            GameObject replayList = GameObject.Find("SimLoadReplayList");
            string entry = replayList.GetComponent<LoadReplayScrollable>().selectedEntry;
            //loadingPanel.SetActive(true);

            if (entry != null)
            {
                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                    AnalyticsLedger.TimingVarible.Viewing,
                    AnalyticsLedger.TimingLabel.ReplayMode);

                loadingPanel.SetActive(true);
                PlayerPrefs.SetString("simSelectedReplay", entry);
                PlayerPrefs.Save();
                SceneManager.LoadScene("Scene");
            }

            replayList.SetActive(false);
        }
    }
}
