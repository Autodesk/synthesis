using Crosstales.FB;
using Crosstales.FB.Demo;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.GUI.Scrollables;
using Synthesis.MixAndMatch;
using Synthesis.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

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
            fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\synthesis\\Fields"));
            mixAndMatchModeScript = Auxiliary.FindGameObject("MixAndMatchModeScript");
            splashScreen = Auxiliary.FindGameObject("LoadSplash");
            fieldList = GameObject.Find("SimLoadFieldList").GetComponent<SelectScrollable>();
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
        public void OnBackButtonPressed()
        {
            StateMachine.PopState();
        }

        /// <summary>
        /// When the select field button is pressed, the selected field is saved and
        /// the current state is popped.
        /// </summary>
        public void OnSelectFieldButtonPressed()
        {
            GameObject fieldList = GameObject.Find("SimLoadFieldList");
            string entry = (fieldList.GetComponent<SelectScrollable>().selectedEntry);
            if (entry != null)
            {
                string simSelectedFieldName = fieldList.GetComponent<SelectScrollable>().selectedEntry;
                string simSelectedField = fieldDirectory + "\\" + simSelectedFieldName + "\\";

                if (StateMachine.FindState<MixAndMatchState>() != null) //Starts the MixAndMatch scene
                {
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
        public void OnFieldExportButtonPressed()
        {
            Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-field.html");
        }

        /// <summary>
        /// Pushes a new <see cref="BrowseFieldState"/> when the change field directory
        /// button is pressed.
        /// </summary>
        public void OnChangeFieldButtonPressed()
        {
            StateMachine.PushState(new BrowseFieldState());
        }
    }
}
