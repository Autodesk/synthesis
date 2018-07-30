using BulletUnity;
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

        int gamepieceIndex;

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

            gamepieceIndex = 0;
            
            InitGamepieceDropdown();    
        }
        public override void Update()
        {
            if (dpmRobot == null) dpmRobot = mainState.ActiveRobot.GetDriverPractice();
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
        public void OnDefineIntakeButtonPressed()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, true, dpmRobot));
        }
        public void OnDefineReleaseButtonPressed()
        {
            StateMachine.SceneGlobal.PushState(new DefineNodeState(dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]), dpmRobot.transform, false, dpmRobot));
        }
        public void OnSetSpawnpointButtonPressed()
        {
            StateMachine.SceneGlobal.PushState(new GamepieceSpawnState(gamepieceIndex));
        }
        public void OnSpawnButtonPressed()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject gamepieceClone = GameObject.Instantiate(GameObject.Find(g.name).GetComponentInParent<BRigidBody>().gameObject, g.spawnpoint, UnityEngine.Quaternion.identity);
            gamepieceClone.name = g.name + "(Clone)";
            gamepieceClone.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.None;
            gamepieceClone.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
        }
        public void OnClearButtonPressed()
        {
            Gamepiece g = FieldDataHandler.gamepieces[gamepieceIndex];
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
            dpmRobot.DestroyAllHeld(true, g.name);
            foreach (GameObject o in gameObjects.Where(o => o.name.Equals(g.name + "(Clone)")))
            {
                GameObject.Destroy(o);
            }
        }
    }
}