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

        GameObject gamepieceDropdownButton;
        GameObject gamepieceDropdownArrow;
        GameObject gamepieceDropdownLabel;
        GameObject gamepieceDropdownExtension;
        List<GameObject> gamepieceDropdownElements;
        GameObject gamepieceDropdownPrefab;
        Transform dropdownLocation;
        bool dropdown = false;
        bool buffer = false;

        GameObject trajectoryPanel;

        int gamepieceIndex; //IMPORTANT - index of current gamepiece in FieldDataHandler.gamepieces[]

        MainState mainState;
        DriverPracticeRobot dpmRobot;

        public override void Start()
        {
            mainState = StateMachine.SceneGlobal.FindState<MainState>();

            canvas = GameObject.Find("Canvas");
            dpmToolbar = Auxiliary.FindObject(canvas, "DPMToolbar");

            gamepieceDropdownButton = Auxiliary.FindObject(dpmToolbar, "GamepieceDropdownButton");
            gamepieceDropdownArrow = Auxiliary.FindObject(gamepieceDropdownButton, "Arrow");
            gamepieceDropdownLabel = Auxiliary.FindObject(gamepieceDropdownButton, "GamepieceName");
            gamepieceDropdownExtension = Auxiliary.FindObject(gamepieceDropdownButton, "Scroll View");
            gamepieceDropdownExtension.SetActive(false);
            gamepieceDropdownPrefab = Resources.Load("Prefabs/GamepieceDropdownElement") as GameObject;
            dropdownLocation = Auxiliary.FindObject(gamepieceDropdownButton, "DropdownLocation").transform;

            trajectoryPanel = Auxiliary.FindObject(canvas, "TrajectoryPanel");

            gamepieceIndex = FieldDataHandler.gamepieceIndex;

            InitGamepieceDropdown();
        }
        public override void Update()
        {
            if (dpmRobot == null) dpmRobot = mainState.ActiveRobot.GetDriverPractice();
            if (mainState.ActiveRobot.GetDriverPractice() != dpmRobot) dpmRobot = mainState.ActiveRobot.GetDriverPractice(); //updates active robot to current active robot
            if (dropdown && buffer) //buffer on gamepiece tab prefabs
                if (Input.GetMouseButtonUp(0))
                {
                    dropdown = false;
                    buffer = false;
                    HideGamepieceDropdown();
                }
            if (!buffer && dropdown && Input.GetMouseButtonDown(0))
                buffer = true;
        }
        /// <summary>
        /// Initializes the gamepiece label
        /// </summary>
        private void InitGamepieceDropdown()
        {
            SetGamepieceDropdownName();
            if (FieldDataHandler.gamepieces.Count() <= 1)
            {
                gamepieceDropdownArrow.SetActive(false);
                gamepieceDropdownButton.GetComponent<Image>().enabled = false;
            }
            else gamepieceDropdownArrow.SetActive(true);
        }
        /// <summary>
        /// Sets the gamepiece button label to the name of the current gamepiece
        /// </summary>
        private void SetGamepieceDropdownName()
        {
            gamepieceDropdownLabel.GetComponent<Text>().text = FieldDataHandler.gamepieces.Count() > 0 ? FieldDataHandler.gamepieces[gamepieceIndex].name : "No Gamepieces";
        }
        /// <summary>
        /// Creates fake dropdown with button prefabs
        /// </summary>
        public void OnGamepieceDropdownButtonClicked()
        {
            HideGamepieceDropdown();
            if (FieldDataHandler.gamepieces.Count > 1)
            {
                dropdown = true;
                for (int i = 0; i < FieldDataHandler.gamepieces.Count; i++)
                {
                    int id = i;

                    if (id != gamepieceIndex)
                    {
                        //create dropdown buttons
                        GameObject gamepieceDropdownElement = GameObject.Instantiate(gamepieceDropdownPrefab);
                        gamepieceDropdownElement.name = "Gamepiece " + id.ToString() + ": " + FieldDataHandler.gamepieces[id].name;
                        gamepieceDropdownElement.transform.parent = dropdownLocation;
                        Auxiliary.FindObject(gamepieceDropdownElement, "Name").GetComponent<Text>().text = FieldDataHandler.gamepieces[id].name;

                        //change current gamepiece
                        Button change = Auxiliary.FindObject(gamepieceDropdownElement, "Change").GetComponent<Button>();
                        change.onClick.AddListener(delegate { gamepieceIndex = id; SetGamepieceDropdownName(); HideGamepieceDropdown(); dropdown = false; buffer = false; });

                        gamepieceDropdownElements.Add(gamepieceDropdownElement);
                    }
                }
                //show panel
                gamepieceDropdownExtension.SetActive(true);
            }

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Changed,
                "Dropdown - Gamepiece",
                AnalyticsLedger.getMilliseconds().ToString());
        }
        /// <summary>
        /// Destroys current dropdown and hides it
        /// </summary>
        private void HideGamepieceDropdown()
        {
            if (gamepieceDropdownElements == null)
                gamepieceDropdownElements = new List<GameObject>(); //avoid null reference 
            //destroy current dropdown buttons
            while (gamepieceDropdownElements.Count > 0)
            {
                GameObject.Destroy(gamepieceDropdownElements[0]);
                gamepieceDropdownElements.RemoveAt(0);
            }
            //hide panels
            gamepieceDropdownExtension.SetActive(false);
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