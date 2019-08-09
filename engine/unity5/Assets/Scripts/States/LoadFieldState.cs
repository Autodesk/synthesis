using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.GUI.Scrollables;
using Synthesis.MixAndMatch;
using Synthesis.Utils;
using System;
using System.IO;
using UnityEngine;

namespace Synthesis.States
{
    public class LoadFieldState : State
    {
        private readonly State nextState;

        private string fieldDirectory;
        private GameObject mixAndMatchModeScript;
        private GameObject splashScreen;
        private SelectScrollable fieldList;

        /// <summary>
        /// Initializes a new <see cref="LoadFieldState"/> instance.
        /// </summary>
        /// <param name="nextState"></param>
        public LoadFieldState(State nextState = null)
        {
            this.nextState = nextState;
        }

        /// <summary>
        /// Initializes required <see cref="GameObject"/> references.
        /// </summary>
        public override void Start()
        {
            fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields"));
            mixAndMatchModeScript = Auxiliary.FindGameObject("MixAndMatchModeScript");
            splashScreen = Auxiliary.FindGameObject("LoadSplash");
            fieldList = GameObject.Find("SimLoadFieldList").GetComponent<SelectScrollable>();

            fieldList.ThumbTexture = Resources.Load("Images/New Textures/Synthesis_an_Autodesk_Technology_2019_lockup_OL_stacked_no_year") as Texture2D;
            fieldList.ListTextColor = Color.black;

        }

        /// <summary>
        /// Updates the field list when this state is activated.
        /// </summary>
        public override void Resume()
        {
            fieldList.Refresh(PlayerPrefs.GetString("FieldDirectory"));
        }

        /// <summary>
        /// Pops this state when the back button is pressed.
        /// </summary>
        public void OnBackButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.TimingCatagory.MixMatch,
                AnalyticsLedger.EventAction.BackedOut,
                "notCompletely",
                AnalyticsLedger.getMilliseconds().ToString());

            StateMachine.PopState();
        }

        /// <summary>
        /// When the select field button is pressed, the selected field is saved and
        /// the current state is popped.
        /// </summary>
        public void OnSelectFieldButtonClicked()
        {
            GameObject fieldList = GameObject.Find("SimLoadFieldList");
            string entry = (fieldList.GetComponent<SelectScrollable>().selectedEntry);
            UserMessageManager.Dispatch(fieldDirectory, 20);
            if (entry != null)
            {
                string simSelectedFieldName = fieldList.GetComponent<SelectScrollable>().selectedEntry;
                string simSelectedField = fieldDirectory + Path.DirectorySeparatorChar + simSelectedFieldName + Path.DirectorySeparatorChar;

                if (StateMachine.FindState<MixAndMatchState>() != null) //Starts the MixAndMatch scene
                {
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MixMatch,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MixAndMatchMenu);

                    AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MixAndMatchSimulator,
                        AnalyticsLedger.EventAction.StartSim,
                        "",
                        AnalyticsLedger.getMilliseconds().ToString());

                    PlayerPrefs.SetString("simSelectedField", simSelectedField);
                    PlayerPrefs.SetString("simSelectedFieldName", simSelectedFieldName);
                    fieldList.SetActive(false);
                    splashScreen?.SetActive(true);
                    mixAndMatchModeScript.GetComponent<MixAndMatchMode>().StartMaMSim();
                }
                else
                {
                    PlayerPrefs.SetString("simSelectedField", simSelectedField);
                    PlayerPrefs.SetString("simSelectedFieldName", simSelectedFieldName);

                    if (nextState == null)
                        StateMachine.PopState();
                    else
                        StateMachine.PushState(nextState);
                }
            }
            else
            {
                UserMessageManager.Dispatch("No Field Selected!", 2);
            }
        }

        /// <summary>
        /// Launches the browser and opens the field tutorials webpage when the field exporter
        /// tutorial button is pressed.
        /// </summary>
        public void OnFieldExportButtonClicked()
        {
            Application.OpenURL("http://synthesis.autodesk.com/tutorials.html");
        }

        /// <summary>
        /// Pushes a new <see cref="BrowseFieldState"/> when the change field directory
        /// button is pressed.
        /// </summary>
        public void OnChangeFieldButtonClicked()
        {
            StateMachine.PushState(new BrowseFieldState());
        }
    }
}
