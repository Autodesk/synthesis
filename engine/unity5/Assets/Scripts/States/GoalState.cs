using BulletUnity;
using Synthesis.DriverPractice;
using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.States
{
    public class GoalState : State
    {
        GameObject goalIndicator;
        GoalManager gm;

        string color;
        int gamepieceIndex;
        int goalIndex;

        bool settingGamepieceGoalVertical = false;
        DynamicCamera.CameraState lastCameraState;

        public GoalState(string color, int gamepieceIndex, int goalIndex, GoalManager gm)
        {
            this.color = color;
            this.gamepieceIndex = gamepieceIndex;
            this.goalIndex = goalIndex;
            this.gm = gm;
        }
        // Use this for initialization
        public override void Start()
        {
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

            settingGamepieceGoalVertical = false;

            DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
            lastCameraState = dynamicCamera.cameraState;

            DynamicCamera.OrthographicSateliteState satellite = new DynamicCamera.OrthographicSateliteState(dynamicCamera);
            satellite.target = goalIndicator;
            satellite.targetOffset = new UnityEngine.Vector3(0f, 6f, 0f);
            satellite.rotationVector = new UnityEngine.Vector3(90f, 90f, 0f);
            satellite.orthoSize = 4;
            dynamicCamera.SwitchCameraState(satellite);
        }

        // Update is called once per frame
        public override void Update()
        {
            if (goalIndicator != null)
            {
                if (!settingGamepieceGoalVertical)
                {
                    if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) goalIndicator.transform.position += UnityEngine.Vector3.forward * 0.04f;
                    if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) goalIndicator.transform.position += UnityEngine.Vector3.back * 0.04f;
                    if (UnityEngine.Input.GetKey(KeyCode.UpArrow)) goalIndicator.transform.position += UnityEngine.Vector3.right * 0.04f;
                    if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) goalIndicator.transform.position += UnityEngine.Vector3.left * 0.04f;
                    //if (Input.GetKey(KeyCode.Comma)) goalIndicator.transform.localScale /= 1.03f;
                    //if (Input.GetKey(KeyCode.Period)) goalIndicator.transform.localScale *= 1.03f;
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
                    {
                        DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
                        DynamicCamera.SateliteState newSatelliteState = new DynamicCamera.SateliteState(dynamicCamera);
                        newSatelliteState.target = goalIndicator;
                        newSatelliteState.rotationVector = new UnityEngine.Vector3(15f, 0f, 0f); // Downward tilt of camera to view slightly from above

                        float offsetDist = goalIndicator.transform.localScale.magnitude + 2; // Set distance of camera to two units further than size of box
                        newSatelliteState.targetOffset = new UnityEngine.Vector3(0f, 0f, -offsetDist);// offsetDist / 32f, -offsetDist);
                        newSatelliteState.targetOffset = UnityEngine.Quaternion.Euler(newSatelliteState.rotationVector) * newSatelliteState.targetOffset; // Rotate camera offset to face block
                        newSatelliteState.targetOffset += new UnityEngine.Vector3(0f, -offsetDist / 10, 0f);

                        dynamicCamera.SwitchCameraState(newSatelliteState);

                        settingGamepieceGoalVertical = true;
                    }
                }
                else
                {
                    DynamicCamera.SateliteState satellite = ((DynamicCamera.SateliteState)UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>().cameraState);

                    if (UnityEngine.Input.GetKey(KeyCode.LeftArrow)) satellite.rotationVector += UnityEngine.Vector3.up * 1f;
                    if (UnityEngine.Input.GetKey(KeyCode.RightArrow)) satellite.rotationVector += UnityEngine.Vector3.down * 1f;
                    if (UnityEngine.Input.GetKey(KeyCode.UpArrow)) goalIndicator.transform.position += UnityEngine.Vector3.up * 0.03f;
                    if (UnityEngine.Input.GetKey(KeyCode.DownArrow)) goalIndicator.transform.position += UnityEngine.Vector3.down * 0.03f;
                    //if (Input.GetKey(KeyCode.Comma)) goalIndicator.transform.localScale /= 1.03f;
                    //if (Input.GetKey(KeyCode.Period)) goalIndicator.transform.localScale *= 1.03f;
                    if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
                    {
                        UserMessageManager.Dispatch("New goal location has been set!", 3f);


                        if (color.Equals("Red"))
                        {
                            gm.redGoals[gamepieceIndex][goalIndex].GetComponent<BRigidBody>().SetPosition(goalIndicator.transform.position);
                            gm.redGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().position = goalIndicator.transform.position;

                        }
                        else
                        {
                            gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<BRigidBody>().SetPosition(goalIndicator.transform.position);
                            gm.blueGoals[gamepieceIndex][goalIndex].GetComponent<Goal>().position = goalIndicator.transform.position;
                        }

                        if (goalIndicator != null) GameObject.Destroy(goalIndicator);
                        DynamicCamera dynamicCamera = UnityEngine.Camera.main.transform.GetComponent<DynamicCamera>();
                        dynamicCamera.SwitchCameraState(lastCameraState);
                        gm.WriteGoals();

                        ReturnToMainState();
                        return;
                    }

                    float offsetDist = goalIndicator.transform.localScale.magnitude + 2; // Set distance of camera to two units further than size of box
                    satellite.targetOffset = new UnityEngine.Vector3(0f, 0f, -offsetDist);// offsetDist / 32f, -offsetDist);
                    satellite.targetOffset = UnityEngine.Quaternion.Euler(satellite.rotationVector) * satellite.targetOffset; // Rotate camera offset to face block
                    satellite.targetOffset += new UnityEngine.Vector3(0f, -offsetDist / 10, 0f);
                }
            }
        }
        private void ReturnToMainState()
        {
            StateMachine.PopState();
        }
    }
}
