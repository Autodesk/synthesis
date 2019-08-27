using BulletUnity;
using Synthesis.Configuration;
using Synthesis.DriverPractice;
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
            //create indicator
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

            GameObject goal = color.Equals("Red") ? gm.redGoals[gamepieceIndex][goalIndex] : gm.blueGoals[gamepieceIndex][goalIndex];
            goalIndicator.transform.position = goal.GetComponent<Goal>().position;
            goalIndicator.transform.localScale = goal.GetComponent<Goal>().scale;

            settingGamepieceGoalVertical = false;

            //move arrow attachment
            GameObject moveArrows = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs\\MoveArrows"));
            moveArrows.name = "IndicatorMoveArrows";
            moveArrows.transform.parent = goalIndicator.transform;
            moveArrows.transform.localPosition = UnityEngine.Vector3.zero;

            if (move) moveArrows.GetComponent<MoveArrows>().Translate = (translation) => goalIndicator.transform.Translate(translation, Space.World);
            else moveArrows.GetComponent<MoveArrows>().Translate = (translation) => goalIndicator.transform.localScale += translation;

            StateMachine.SceneGlobal.Link<GoalState>(moveArrows);

            //camera stuff
            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            lastCameraState = dynamicCamera.ActiveState;
            dynamicCamera.SwitchCameraState(new DynamicCamera.ConfigurationState(dynamicCamera, goalIndicator));

            //UI callbacks 
            Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(Reset);
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
            if (goalIndicator != null)
            {
                if (move)
                {
                    if (!UnityEngine.Input.GetMouseButton(0))
                    {
                        const float RESET_SPEED = 0.05f;
                        if (InputControl.GetButton(Controls.Global.GetButtons().cameraLeft, overrideFreeze: true)) goalIndicator.transform.position += UnityEngine.Vector3.forward * RESET_SPEED;
                        if (InputControl.GetButton(Controls.Global.GetButtons().cameraRight, overrideFreeze: true)) goalIndicator.transform.position += UnityEngine.Vector3.back * RESET_SPEED;
                        if (InputControl.GetButton(Controls.Global.GetButtons().cameraForward, overrideFreeze: true)) goalIndicator.transform.position += UnityEngine.Vector3.right * RESET_SPEED;
                        if (InputControl.GetButton(Controls.Global.GetButtons().cameraBackward, overrideFreeze: true)) goalIndicator.transform.position += UnityEngine.Vector3.left * RESET_SPEED;
                        if (InputControl.GetButton(Controls.Global.GetButtons().cameraUp, overrideFreeze: true)) goalIndicator.transform.position += UnityEngine.Vector3.up * RESET_SPEED;
                        if (InputControl.GetButton(Controls.Global.GetButtons().cameraDown, overrideFreeze: true)) goalIndicator.transform.position += UnityEngine.Vector3.down * RESET_SPEED;
                    }
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
                {
                    UserMessageManager.Dispatch("New goal location has been set", 3f);


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
            StateMachine.PopState();
        }
        private void Reset()
        {
            if (move) goalIndicator.transform.position = new Vector3(0f, 4f, 0f);
            else goalIndicator.transform.localScale = Vector3.one;
        }
    }
}