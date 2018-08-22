using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.DriverPractice;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
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

        //help menu stuffs
        GameObject helpMenu;
        GameObject overlay;
        GameObject tabs;
        Text helpBodyText;

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

            tabs = Auxiliary.FindObject(canvas, "Tabs");
            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            trajectoryPanel = Auxiliary.FindObject(canvas, "TrajectoryPanel");

            gamepieceIndex = 0;

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);

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
            if (!buffer && dropdown)
                if (Input.GetMouseButtonDown(0))
                {
                    buffer = true;
                }
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
        public void OnGamepieceDropdownButtonPressed()
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

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Changed Gamepiece", new Dictionary<string, object> //for analytics tracking
                {
                });
            }
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
        public void OnDefineIntakeButtonPressed()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, true, dpmRobot));
        }
        /// <summary>
        /// Change to release state
        /// </summary>
        public void OnDefineReleaseButtonPressed()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, false, dpmRobot));
        }
        /// <summary>
        /// Change to gamepiece spawnpoint state
        /// </summary>
        public void OnSetSpawnpointButtonPressed()
        {
            StateMachine.SceneGlobal.PushState(new GamepieceSpawnState(gamepieceIndex));
        }
        /// <summary>
        /// Spawn gamepiece clone
        /// </summary>
        public void OnSpawnButtonPressed()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject gamepieceClone = GameObject.Instantiate(Auxiliary.FindGameObject(g.name), g.spawnpoint, UnityEngine.Quaternion.identity); //clone gamepiece - exact clone will keep joints
            gamepieceClone.SetActive(true); //show in case all gamepieces are hidden
            if (gamepieceClone.GetComponent<BFixedConstraintEx>() != null) GameObject.Destroy(gamepieceClone.GetComponent<BFixedConstraintEx>()); //remove joints from clone
            gamepieceClone.name = g.name + "(Clone)"; //add clone tag to allow clear later
            gamepieceClone.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.None;
            gamepieceClone.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Spawned Gamepiece", new Dictionary<string, object> //for analytics tracking
                {
                });
            }

        }
        /// <summary>
        /// Clear gamepiece clones
        /// </summary>
        public void OnClearButtonPressed()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
            dpmRobot.DestroyAllHeld(true, g.name); //destroy clones held by robot
            //Destory all clones
            foreach (GameObject o in gameObjects.Where(o => o.name.Equals(g.name + "(Clone)")))
                GameObject.Destroy(o);
        }

        public void OnHelpButtonPressed()
        {
            helpMenu.SetActive(true);

            //help menu text - CUT DOWN
            helpBodyText.GetComponent<Text>().text = "The workflow of Driver Practice Mode is intended to be left to right " +
                "across the toolbar for initial setup." +
                "\n\n 1. If there are multiple gamepieces, click on the game piece dropdown in the far left and select desired gamepiece.All configuration will be specific to currently selected gamepiece." +
                "\n\n 2. Click DEFINE INTAKE then select the intake node by clicking on appropriate mechanism on robot model, hover over model to highlight available nodes." +
                "\n\n 3. Click DEFINE RELEASE then select the intake node by clicking on appropriate mechanism on robot model, hover over model to highlight available nodes.Intake and release can be the same mechanism." +
                "\n\nChange Gamepiece Motion: Click EDIT TRAJECTORY to change the speed, angle, and release position of the game piece." +
                "\n\nChange Gamepiece Spawnpoint: Click SET SPAWNPOINT and use WASD or drag navigation arrows to edit game piece spawn location" +
                "\n\nAdd Gamepieces: Click SPAWN to add gamepieces to field" +
                "\n\nClear Gamepieces: Click CLEAR to delete spawned gamepieces. To reset all field elements, navigate to the HOME tab, open the RESET dropdown, and select RESET FIELD.";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "DPMToolbar"; //set type - there may not be a reason for this
            overlay.SetActive(true); //set overlay - fades out screen slightly
            tabs.transform.Translate(new Vector3(300, 0, 0)); //translate to the right
            foreach (Transform t in dpmToolbar.transform)
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0)); //translate tabs to the right
                else t.gameObject.SetActive(false); //hide help button

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Driver Practice Help Pressed", new Dictionary<string, object> //for analytics tracking
                {
                });
            }

        }
        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0)); //translate tabs to the left
            foreach (Transform t in dpmToolbar.transform)
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0)); //translate buttons to the left
                else t.gameObject.SetActive(true); //show help button
        }
    }
}