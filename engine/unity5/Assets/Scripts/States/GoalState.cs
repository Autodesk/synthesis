using BulletUnity;
using Synthesis.Configuration;
using Synthesis.DriverPractice;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class GoalState : State
    {
        GameObject goalIndicator;
        GoalManager gm;

        string color;
        int gamepieceIndex;
        int goalIndex;
        bool move;

        bool settingGamepieceGoalVertical = false;
        DynamicCamera.CameraState lastCameraState;

        #region help ui variables
        GameObject ui;
        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        #endregion

        public GoalState(string color, int gamepieceIndex, int goalIndex, GoalManager gm, bool move)
        {
            this.color = color;
            this.gamepieceIndex = gamepieceIndex;
            this.goalIndex = goalIndex;
            this.gm = gm;
            this.move = move;
        }
        // Use this for initialization
        public override void Start()
        {
            #region init
            ui = GameObject.Find("GoalStateUI");
            helpMenu = Auxiliary.FindObject(ui, "Help");
            toolbar = Auxiliary.FindObject(ui, "ResetStateToolbar");
            overlay = Auxiliary.FindObject(ui, "Overlay");
            #endregion

            if (goalIndicator != null) GameObject.Destroy(goalIndicator);
            if (goalIndicator == null)
            {
                goalIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube); // Create cube to show goal region
                goalIndicator.name = "GoalIndicator";
                Renderer render = goalIndicator.GetComponentInChildren<Renderer>();
                render.material.shader = Shader.Find("Transparent/Diffuse");
                Color newColor = new Color(0, 0.88f, 0, 0.6f);
                render.material.color = newColor;
            }
            goalIndicator.transform.position = color.Equals("Red") ? gm.redGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().position : gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().position;
            goalIndicator.transform.localScale = color.Equals("Red") ? gm.redGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().scale : gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().scale;

            settingGamepieceGoalVertical = false;

            GameObject moveArrows = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs\\MoveArrows"));
            moveArrows.name = "IndicatorMoveArrows";
            moveArrows.transform.parent = goalIndicator.transform;
            moveArrows.transform.localPosition = UnityEngine.Vector3.zero;

            if (move) moveArrows.GetComponent<MoveArrows>().Translate = (translation) => goalIndicator.transform.Translate(translation, Space.World);
            else moveArrows.GetComponent<MoveArrows>().Translate = (translation) => goalIndicator.transform.localScale += translation;//goalIndicator.transform.localScale.Scale(translation);

            StateMachine.SceneGlobal.Link<GoalState>(moveArrows);

            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            lastCameraState = dynamicCamera.cameraState;

            dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, goalIndicator));

            Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(Reset);
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
            if (goalIndicator != null)
            {
                if (move)
                {
                    if (UnityEngine.Input.GetKey(KeyCode.A)) goalIndicator.transform.position += UnityEngine.Vector3.forward * 0.1f;
                    if (UnityEngine.Input.GetKey(KeyCode.D)) goalIndicator.transform.position += UnityEngine.Vector3.back * 0.1f;
                    if (UnityEngine.Input.GetKey(KeyCode.W)) goalIndicator.transform.position += UnityEngine.Vector3.right * 0.1f;
                    if (UnityEngine.Input.GetKey(KeyCode.S)) goalIndicator.transform.position += UnityEngine.Vector3.left * 0.1f;
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    UserMessageManager.Dispatch("New goal location has been set!", 3f);


                    if (color.Equals("Red"))
                    {
                        gm.redGoals[gamepieceIndex][goalIndex].GetComponent<BRigidBody>().SetPosition(goalIndicator.transform.position);
                        gm.redGoals[gamepieceIndex][goalIndex].GetComponent<BBoxShape>().LocalScaling = goalIndicator.transform.localScale;
                        gm.redGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().position = goalIndicator.transform.position;
                        gm.redGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().scale = goalIndicator.transform.localScale;
                    }
                    else
                    {
                        gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<BRigidBody>().SetPosition(goalIndicator.transform.position);
                        gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<BBoxShape>().LocalScaling = goalIndicator.transform.localScale;
                        gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().position = goalIndicator.transform.position;
                        gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().scale = goalIndicator.transform.localScale;
                    }

                    gm.WriteGoals();

                    ReturnToMainState();
                    return;
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
            GameObject.Destroy(goalIndicator);
            if (helpMenu.activeSelf) CloseHelpMenu();
            StateMachine.PopState();
        }
        private void Reset()
        {
            if (move) goalIndicator.transform.position = new Vector3(0f, 4f, 0f);
            else goalIndicator.transform.localScale = Vector3.one;
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
