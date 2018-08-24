﻿using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.GUI.Scrollables;
using System;
using UnityEngine;

namespace Synthesis.States
{
    public class LoadRobotState : State
    {
        private readonly State nextState;

        private string robotDirectory;
        private SelectScrollable robotList;

        /// <summary>
        /// Initializes a new <see cref="LoadRobotState"/> instance.
        /// </summary>
        /// <param name="nextState"></param>
        public LoadRobotState(State nextState = null)
        {
            this.nextState = nextState;
        }

        /// <summary>
        /// Initializes the <see cref="LoadRobotState"/>.
        /// </summary>
        public override void Start()
        {
            robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
            robotList = GameObject.Find("SimLoadRobotList").GetComponent<SelectScrollable>();

            robotList.ThumbTexture = Resources.Load("Images/New Textures/Synthesis_an_Autodesk_Technology_2019_lockup_OL_stacked_no_year") as Texture2D;
            robotList.ListTextColor = Color.black;
        }

        /// <summary>
        /// Updates the robot list when this state is activated.
        /// </summary>
        public override void Resume()
        {
            robotList.Refresh(PlayerPrefs.GetString("RobotDirectory"));
        }

        /// <summary>
        /// Pops the current state when the back button is pressed.
        /// </summary>
        public void OnBackButtonClicked()
        {
            StateMachine.PopState();
        }

        /// <summary>
        /// Saves the current selected robot and pops the current <see cref="State"/> when
        /// the select robot button is pressed.
        /// </summary>
        public void OnSelectRobotButtonClicked()
        {
            GameObject robotList = GameObject.Find("SimLoadRobotList");
            string entry = (robotList.GetComponent<SelectScrollable>().selectedEntry);
            if (entry != null)
            {
                string simSelectedRobotName = robotList.GetComponent<SelectScrollable>().selectedEntry;

                PlayerPrefs.SetString("simSelectedRobot", robotDirectory + "\\" + simSelectedRobotName + "\\");
                PlayerPrefs.SetString("simSelectedRobotName", simSelectedRobotName);

                if (nextState == null)
                    StateMachine.PopState();
                else
                    StateMachine.PushState(nextState);
            }
            else
            {
                UserMessageManager.Dispatch("No Robot Selected!", 2);
            }
        }

        /// <summary>
        /// Launches the browser and opens the robot export tutorials webpage.
        /// </summary>
        public void OnRobotExportButtonClicked()
        {
            Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
        }

        /// <summary>
        /// Pushes a new <see cref="BrowseRobotState"/> when the change robot directory
        /// button is pressed.
        /// </summary>
        public void OnChangeRobotButtonClicked()
        {
            StateMachine.PushState(new BrowseRobotState());
        }
    }
}
