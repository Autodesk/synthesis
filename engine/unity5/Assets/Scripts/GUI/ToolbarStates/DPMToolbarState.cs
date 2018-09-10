using BulletSharp;
using BulletUnity;
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
using UnityEngine.Analytics;
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

        int gamepieceIndex;

        MainState mainState;
        DriverPracticeRobot dpmRobot;

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

            gamepieceIndex = FieldDataHandler.gamepieceIndex;

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);

            InitGamepieceDropdown();
        }
        public override void Update()
        {
            if (dpmRobot == null) dpmRobot = mainState.ActiveRobot.GetDriverPractice();
            if (mainState.ActiveRobot.GetDriverPractice() != dpmRobot) dpmRobot = mainState.ActiveRobot.GetDriverPractice();
            if (dropdown && buffer)
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
        private void SetGamepieceDropdownName()
        {
            gamepieceDropdownLabel.GetComponent<Text>().text = FieldDataHandler.gamepieces.Count() > 0 ? FieldDataHandler.gamepieces[gamepieceIndex].name : "No Gamepieces";
        }
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
                        GameObject gamepieceDropdownElement = GameObject.Instantiate(gamepieceDropdownPrefab);
                        gamepieceDropdownElement.name = "Gamepiece " + id.ToString() + ": " + FieldDataHandler.gamepieces[id].name;
                        gamepieceDropdownElement.transform.parent = dropdownLocation;

                        Auxiliary.FindObject(gamepieceDropdownElement, "Name").GetComponent<Text>().text = FieldDataHandler.gamepieces[id].name;

                        Button change = Auxiliary.FindObject(gamepieceDropdownElement, "Change").GetComponent<Button>();
                        change.onClick.AddListener(delegate { gamepieceIndex = id; SetGamepieceDropdownName(); HideGamepieceDropdown(); dropdown = false; buffer = false; });

                        gamepieceDropdownElements.Add(gamepieceDropdownElement);
                    }
                }
                gamepieceDropdownExtension.SetActive(true);
            }

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Changed Gamepiece", new Dictionary<string, object> //for analytics tracking
                {
                });
            }
        }
        private void HideGamepieceDropdown()
        {
            if (gamepieceDropdownElements == null)
                gamepieceDropdownElements = new List<GameObject>();

            while (gamepieceDropdownElements.Count > 0)
            {
                GameObject.Destroy(gamepieceDropdownElements[0]);
                gamepieceDropdownElements.RemoveAt(0);
            }
            gamepieceDropdownExtension.SetActive(false);
        }
        public void OnDefineIntakeButtonClicked()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, true, dpmRobot), true);
        }
        public void OnDefineReleaseButtonClicked()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, false, dpmRobot), true);
        }
        public void OnSetSpawnpointButtonClicked()
        {
            StateMachine.SceneGlobal.PushState(new GamepieceSpawnState(gamepieceIndex), true);
        }
        public void OnSpawnButtonClicked()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject gamepieceClone = GameObject.Instantiate(GameObject.Find(g.name).GetComponentInParent<BRigidBody>().gameObject, g.spawnpoint, UnityEngine.Quaternion.identity);
            gamepieceClone.name = g.name + "(Clone)";
            gamepieceClone.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.None;
            gamepieceClone.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Spawned Gamepiece", new Dictionary<string, object> //for analytics tracking
                {
                });
            }

        }
        public void OnClearButtonClicked()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
            dpmRobot.DestroyAllHeld(true, g.name);
            foreach (GameObject o in gameObjects.Where(o => o.name.Equals(g.name + "(Clone)")))
            {
                GameObject.Destroy(o);
            }
        }
        public void OnHelpButtonClicked()
        {
            helpMenu.SetActive(true);

            helpBodyText.GetComponent<Text>().text = "The workflow of Driver Practice Mode is intended to be left to right " +
                "across the toolbar for initial setup." +
                "\n\n 1. If there are multiple gamepieces, click on the game piece dropdown in the far left and select desired gamepiece.All configuration will be specific to currently selected gamepiece." +
                "\n\n 2. Click DEFINE INTAKE then select the intake node by clicking on appropriate mechanism on robot model, hover over model to highlight available nodes." +
                "\n\n 3. Click DEFINE RELEASE then select the intake node by clicking on appropriate mechanism on robot model, hover over model to highlight available nodes.Intake and release can be the same mechanism." +
                "\n\nChange Gamepiece Motion: Click EDIT TRAJECTORY to change the speed, angle, and release position of the game piece." +
                "\n\nChange Gamepiece Spawnpoint: Click SET SPAWNPOINT and use WASD or drag navigation arrows to edit game piece spawn location" +
                "\n\nAdd Gamepieces: Click SPAWN to add gamepieces to field" +
                "\n\nClear Gamepieces: Click CLEAR to delete spawned gamepieces. To reset all field elements, navigate to the HOME tab, open the RESET dropdown, and select RESET FIELD.";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "DPMToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(300, 0, 0));
            foreach (Transform t in dpmToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0));
                else t.gameObject.SetActive(false);
            }

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
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in dpmToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }
        public override void End()
        {
            FieldDataHandler.gamepieceIndex = gamepieceIndex;
        }
    }
}