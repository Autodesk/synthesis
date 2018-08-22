using BulletUnity;
using Synthesis.Configuration;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class GamepieceSpawnState : State
    {
        private int gamepieceIndex;
        private DynamicCamera.CameraState lastCameraState;
        private GameObject spawnIndicator;

        #region help ui variables
        GameObject ui;
        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        #endregion

        public GamepieceSpawnState(int gamepieceIndex)
        {
            this.gamepieceIndex = gamepieceIndex;
        }
        // Use this for initialization
        public override void Start()
        {
            #region init
            ui = GameObject.Find("ResetGamepieceSpawnpointUI");
            helpMenu = Auxiliary.FindObject(ui, "Help");
            toolbar = Auxiliary.FindObject(ui, "ResetStateToolbar");
            overlay = Auxiliary.FindObject(ui, "Overlay");
            #endregion

            Gamepiece gamepiece = FieldDataHandler.gamepieces[gamepieceIndex];
            //gamepiece indicator
            if (spawnIndicator != null) GameObject.Destroy(spawnIndicator);
            if (spawnIndicator == null)
            {
                spawnIndicator = GameObject.Instantiate(Auxiliary.FindGameObject(gamepiece.name), new UnityEngine.Vector3(0, 3, 0), UnityEngine.Quaternion.identity);
                spawnIndicator.SetActive(true);
                spawnIndicator.name = "SpawnIndicator";
                GameObject.Destroy(spawnIndicator.GetComponent<BRigidBody>());
                GameObject.Destroy(spawnIndicator.GetComponent<BCollisionShape>());
                GameObject.Destroy(spawnIndicator.GetComponent<Tracker>());
                if (spawnIndicator.transform.GetChild(0) != null) spawnIndicator.transform.GetChild(0).name = "SpawnIndicatorMesh";
                Renderer render = spawnIndicator.GetComponentInChildren<Renderer>();
                render.material.shader = Shader.Find("Transparent/Diffuse");
                Color newColor = render.material.color;
                newColor.a = 0.6f;
                render.material.color = newColor;
            }
            spawnIndicator.transform.position = gamepiece.spawnpoint;

            //move arrow attachment
            GameObject moveArrows = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs\\MoveArrows"));
            moveArrows.name = "IndicatorMoveArrows";
            moveArrows.transform.parent = spawnIndicator.transform;
            moveArrows.transform.localPosition = UnityEngine.Vector3.zero;

            //IMPORTANT
            moveArrows.GetComponent<MoveArrows>().Translate = (translation) =>
                spawnIndicator.transform.Translate(translation, Space.World);

            StateMachine.SceneGlobal.Link<GamepieceSpawnState>(moveArrows);

            //camera stuff
            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            lastCameraState = dynamicCamera.cameraState;
            dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, spawnIndicator));

            //help menu
            Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetSpawn);
            Button helpButton = GameObject.Find("HelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(HelpMenu);
            Button returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToMainState);
            Button closeHelp = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            closeHelp.onClick.RemoveAllListeners();
            closeHelp.onClick.AddListener(CloseHelpMenu);
        }

        // Update is called once per frame
        public override void Update()
        {
            if (spawnIndicator != null)
            {
                if (UnityEngine.Input.GetKey(KeyCode.A)) spawnIndicator.transform.position += UnityEngine.Vector3.forward * 0.1f;
                if (UnityEngine.Input.GetKey(KeyCode.D)) spawnIndicator.transform.position += UnityEngine.Vector3.back * 0.1f;
                if (UnityEngine.Input.GetKey(KeyCode.W)) spawnIndicator.transform.position += UnityEngine.Vector3.right * 0.1f;
                if (UnityEngine.Input.GetKey(KeyCode.S)) spawnIndicator.transform.position += UnityEngine.Vector3.left * 0.1f;
                if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    UserMessageManager.Dispatch("New gamepiece spawn location has been set!", 3f);
                    FieldDataHandler.gamepieces[gamepieceIndex].spawnpoint = spawnIndicator.transform.position;
                    FieldDataHandler.WriteField();
                    ReturnToMainState();
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                {
                    ReturnToMainState();
                }
            }
        }
        private void ReturnToMainState()
        {
            if (helpMenu.activeSelf) CloseHelpMenu();
            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            dynamicCamera.SwitchCameraState(lastCameraState);
            GameObject.Destroy(spawnIndicator);
            StateMachine.PopState();
        }
        private void ResetSpawn()
        {
            spawnIndicator.transform.position = new Vector3(0f, 3f, 0f);
        }
        private void HelpMenu()
        {
            helpMenu.SetActive(true);
            overlay.SetActive(true);
            toolbar.transform.Translate(new Vector3(100, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(100, 0, 0));
                else t.gameObject.SetActive(false);
            }
        }
        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            toolbar.transform.Translate(new Vector3(-100, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-100, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }
    }
}
