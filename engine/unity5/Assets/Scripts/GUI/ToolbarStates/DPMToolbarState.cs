using BulletSharp;
using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// The Driver Practice toolbar. Controls buttons and states on the driver practice toolbar.
    /// </summary>
    public class DPMToolbarState : State
    {
        GameObject canvas;
        GameObject dpmToolbar;
        GameObject tabs;

        Dropdown gamepieceDropdown;
        GameObject gamepieceDropdownArrow;
        Text gamepieceDropdownLabel;

        GameObject trajectoryPanel;

        int gamepieceIndex; //IMPORTANT - index of current gamepiece in FieldDataHandler.gamepieces[]

        MainState mainState;
        DriverPracticeRobot dpmRobot;

        public override void Start()
        {
            mainState = StateMachine.SceneGlobal.FindState<MainState>();

            canvas = GameObject.Find("Canvas");
            dpmToolbar = Auxiliary.FindObject(canvas, "DPMToolbar");
            tabs = Auxiliary.FindObject(canvas, "Tabs");

            gamepieceDropdown = Auxiliary.FindObject(dpmToolbar, "GamepieceDropdown").GetComponent<Dropdown>();
            gamepieceDropdownArrow = Auxiliary.FindObject(gamepieceDropdown.gameObject, "Arrow");
            gamepieceDropdown.onValueChanged.AddListener(OnGamepieceDropdownValueChanged);
            gamepieceDropdownLabel = Auxiliary.FindObject(gamepieceDropdown.gameObject, "Label").GetComponent<Text>();
            for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
            {
                gamepieceDropdown.options.Add(new Dropdown.OptionData(FieldDataHandler.gamepieces[i].name));
            }
            if (FieldDataHandler.gamepieces.Count == 0)
            {
                gamepieceDropdown.options.Clear();
                gamepieceDropdown.options.Add(new Dropdown.OptionData("No Gamepieces"));
                gamepieceDropdown.value = 0;
                gamepieceDropdown.RefreshShownValue();
                gamepieceDropdown.interactable = false;
                gamepieceDropdownArrow.SetActive(false);
            }
            else
            {
                gamepieceDropdown.RefreshShownValue();
                gamepieceDropdown.value = 0;
                gamepieceDropdown.interactable = true;
                gamepieceDropdownArrow.SetActive(true);
            }

            trajectoryPanel = Auxiliary.FindObject(canvas, "TrajectoryPanel");

            gamepieceIndex = FieldDataHandler.gamepieceIndex;
        }

        public override void Update()
        {
            if (dpmRobot == null) dpmRobot = mainState.ActiveRobot.GetDriverPractice();
            if (mainState.ActiveRobot.GetDriverPractice() != dpmRobot) dpmRobot = mainState.ActiveRobot.GetDriverPractice(); //updates active robot to current active robot
        }

        public void OnGamepieceDropdownValueChanged(int value)
        {
            gamepieceIndex = value;
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Changed,
                "Dropdown - Gamepiece",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        /// <summary>
        /// Change to intake state
        /// </summary>
        public void OnDefineIntakeButtonClicked()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, true, dpmRobot), true);

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Clicked,
                "Define Intake",
            AnalyticsLedger.getMilliseconds().ToString());
        }
        /// <summary>
        /// Change to release state
        /// </summary>
        public void OnDefineReleaseButtonClicked()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, false, dpmRobot), true);

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Clicked,
                "Define Release",
            AnalyticsLedger.getMilliseconds().ToString());
        }
        /// <summary>
        /// Change to gamepiece spawnpoint state
        /// </summary>
        public void OnSetSpawnpointButtonClicked()
        {
            StateMachine.SceneGlobal.PushState(new GamepieceSpawnState(gamepieceIndex), true);

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Clicked,
                "Set Gamepiece Spawnpoint",
                AnalyticsLedger.getMilliseconds().ToString());
        }
        /// <summary>
        /// Spawn gamepiece clone
        /// </summary>
        public void OnSpawnButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Clicked,
                "Spawn Gamepiece",
                AnalyticsLedger.getMilliseconds().ToString());

            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject gamepieceClone = GameObject.Instantiate(Auxiliary.FindGameObject(g.name), g.spawnpoint, UnityEngine.Quaternion.identity); //clone gamepiece - exact clone will keep joints
            gamepieceClone.SetActive(true); //show in case all gamepieces are hidden
            if (gamepieceClone.GetComponent<BFixedConstraintEx>() != null) GameObject.Destroy(gamepieceClone.GetComponent<BFixedConstraintEx>()); //remove joints from clone
            gamepieceClone.name = g.name + "(Clone)"; //add clone tag to allow clear later
            gamepieceClone.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.None;
            gamepieceClone.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
        }
        /// <summary>
        /// Clear gamepiece clones
        /// </summary>
        public void OnClearButtonClicked()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
            dpmRobot.DestroyAllHeld(true, g.name); //destroy clones held by robot
            //Destory all clones
            foreach (GameObject o in gameObjects.Where(o => o.name.Equals(g.name + "(Clone)")))
                GameObject.Destroy(o);

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Clicked,
                "Clear Gamepiece",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        public override void ToggleHidden()
        {
            dpmToolbar.SetActive(!dpmToolbar.activeSelf);
        }

        public override void End()
        {
            FieldDataHandler.gamepieceIndex = gamepieceIndex;
        }
    }
}