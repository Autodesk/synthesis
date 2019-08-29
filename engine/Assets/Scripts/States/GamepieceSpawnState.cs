using BulletUnity;
using Synthesis.Configuration;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Input;
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

        public GamepieceSpawnState(int gamepieceIndex)
        {
            this.gamepieceIndex = gamepieceIndex;
        }
        // Use this for initialization
        public override void Start()
        {
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
            spawnIndicator.transform.eulerAngles = gamepiece.spawnorientation;

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
            lastCameraState = dynamicCamera.ActiveState;
            dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, spawnIndicator));

            //help menu
            Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetSpawn);
            Button returnButton = GameObject.Find("ReturnButton").GetComponent<Button>();
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToMainState);

            SimUI.getSimUI().OpenNavigationTooltip();
        }

        public override void End()
        {
            base.End();

            SimUI.getSimUI().CloseNavigationTooltip();
        }

        // Update is called once per frame
        public override void Update()
        {
            if (spawnIndicator != null)
            {
                if (!UnityEngine.Input.GetMouseButton(0)) {
                    const float RESET_TRANSLATE_SPEED = 0.05f;
                    const float RESET_ROTATE_SPEED = 0.5f;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraLeft, overrideFreeze: true)) spawnIndicator.transform.position += UnityEngine.Vector3.forward * RESET_TRANSLATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraRight, overrideFreeze: true)) spawnIndicator.transform.position += UnityEngine.Vector3.back * RESET_TRANSLATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraForward, overrideFreeze: true)) spawnIndicator.transform.position += UnityEngine.Vector3.right * RESET_TRANSLATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraBackward, overrideFreeze: true)) spawnIndicator.transform.position += UnityEngine.Vector3.left * RESET_TRANSLATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraUp, overrideFreeze: true)) spawnIndicator.transform.position += UnityEngine.Vector3.up * RESET_TRANSLATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraDown, overrideFreeze: true)) spawnIndicator.transform.position += UnityEngine.Vector3.down * RESET_TRANSLATE_SPEED;

                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraRotateRight, overrideFreeze: true)) spawnIndicator.transform.eulerAngles += UnityEngine.Vector3.up * RESET_ROTATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraRotateLeft, overrideFreeze: true)) spawnIndicator.transform.eulerAngles += UnityEngine.Vector3.down * RESET_ROTATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraTiltDown, overrideFreeze: true)) spawnIndicator.transform.eulerAngles += UnityEngine.Vector3.back * RESET_ROTATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraTiltUp, overrideFreeze: true)) spawnIndicator.transform.eulerAngles += UnityEngine.Vector3.forward * RESET_ROTATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraRollLeft, overrideFreeze: true)) spawnIndicator.transform.eulerAngles += UnityEngine.Vector3.left * RESET_ROTATE_SPEED;
                    if (InputControl.GetButton(Controls.Global.GetButtons().cameraRollRight, overrideFreeze: true)) spawnIndicator.transform.eulerAngles += UnityEngine.Vector3.right * RESET_ROTATE_SPEED;

                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    UserMessageManager.Dispatch("New gamepiece spawn location has been set!", 3f);
                    FieldDataHandler.gamepieces[gamepieceIndex].spawnpoint = spawnIndicator.transform.position;
                    FieldDataHandler.gamepieces[gamepieceIndex].spawnorientation = spawnIndicator.transform.eulerAngles;
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
            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            dynamicCamera.SwitchCameraState(lastCameraState);
            GameObject.Destroy(spawnIndicator);
            StateMachine.PopState();
        }
        private void ResetSpawn()
        {
            spawnIndicator.transform.position = new Vector3(0f, 3f, 0f);
            spawnIndicator.transform.eulerAngles = Vector3.zero;
        }
    }
}