using BulletUnity;
using Synthesis.Configuration;
using Synthesis.Field;
using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.Robot;
using Synthesis.States;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.DriverPractice
{
    public class TrajectoryEditor : MonoBehaviour
    {
        MainState mainState;

        DriverPracticeRobot dpmRobot;
        
        GameObject canvas;
        GameObject dpmToolbar;
        GameObject gamepieceDropdownButton;
        GameObject gamepieceDropdownLabel;

        GameObject moveArrows;

        #region TrajectoryPanel
        GameObject trajectoryPanel;
        bool trajectory = false;
        GameObject xOffsetEntry;
        GameObject yOffsetEntry;
        GameObject zOffsetEntry;
        GameObject releaseSpeedEntry;
        GameObject releaseVerticalEntry;
        GameObject releaseHorizontalEntry;
        GameObject showTrajectory;
        bool editing = false;
        bool positiveOffset = true;
        int xyz = 0;
        bool position = true;
        const float offsetIncrement = 0.01f;
        const float speedIncrement = 0.1f;
        const float angleIncrement = 1f;
        #endregion

        #region TrajectoryDrawing
        GameObject trajectoryLine;
        #endregion

        int gamepieceIndex; //IMPORTANT

        DriverPractice dp;

        void Update()
        {
            if (mainState == null)
                mainState = StateMachine.SceneGlobal.FindState<MainState>();
            else
            {
                if (dpmRobot == null)
                {
                    dpmRobot = mainState.ActiveRobot.GetDriverPractice();
                    FindElements();
                }
                if (mainState.ActiveRobot.GetDriverPractice() != dpmRobot) OnActiveRobotChange(); //update active robot
                SetGamepieceIndex();
                if (trajectory && !editing) UpdateTrajectoryValues(); 
                if (dpmRobot.drawing && DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray().Length > 0) DrawTrajectory();
                else trajectoryLine.GetComponent<LineRenderer>().enabled = false;
                if (mainState.ActiveRobot.IsResetting && trajectoryPanel.activeSelf) HideEditor();
                else if (!mainState.ActiveRobot.IsResetting && !trajectoryPanel.activeSelf && trajectory) ShowEditor();
            }
        }
        void FindElements()
        {
            canvas = Auxiliary.FindGameObject("Canvas");

            dpmToolbar = Auxiliary.FindObject(canvas, "DPMToolbar");
            gamepieceDropdownButton = Auxiliary.FindObject(dpmToolbar, "GamepieceDropdownButton");
            gamepieceDropdownLabel = Auxiliary.FindObject(gamepieceDropdownButton, "GamepieceName");
            #region Trajectory Editor init
            trajectoryPanel = Auxiliary.FindObject(canvas, "TrajectoryPanel");
            xOffsetEntry = Auxiliary.FindObject(trajectoryPanel, "XOffsetEntry");
            yOffsetEntry = Auxiliary.FindObject(trajectoryPanel, "YOffsetEntry");
            zOffsetEntry = Auxiliary.FindObject(trajectoryPanel, "ZOffsetEntry");
            releaseSpeedEntry = Auxiliary.FindObject(trajectoryPanel, "ReleaseSpeedEntry");
            releaseVerticalEntry = Auxiliary.FindObject(trajectoryPanel, "ReleaseVerticalEntry");
            releaseHorizontalEntry = Auxiliary.FindObject(trajectoryPanel, "ReleaseHorizontalEntry");
            showTrajectory = Auxiliary.FindObject(trajectoryPanel, "ShowHideTrajectory");
            #endregion
            #region Display Trajectory init
            trajectoryLine = new GameObject("DrawnTrajectory");
            StateMachine.SceneGlobal.Link<MainState>(trajectoryLine);
            trajectoryLine.transform.parent = dpmRobot.transform;
            trajectoryLine.AddComponent<LineRenderer>();
            #endregion
        }
        /// <summary>
        /// Sets gamepiece index of FieldDataHandler.gamepieces
        /// </summary>
        private void SetGamepieceIndex()
        {
            for (int i = 0; i < FieldDataHandler.gamepieces.Count(); i++)
                if (gamepieceDropdownLabel.GetComponent<Text>().text.Equals(FieldDataHandler.gamepieces[i].name)) gamepieceIndex = i;
        }
        public void OpenEditor()
        {
            if (DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).Count() > 0)
            {
                if (moveArrows == null) moveArrows = CreateMoveArrows();
                trajectory = true;
                dpmRobot.drawing = true;
                trajectoryPanel.SetActive(true);
                moveArrows.SetActive(true);
            }
            //else prompt user to define intake and release first

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EditTrajectory,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
        }
        public void CloseEditor()
        {
            trajectoryPanel.SetActive(false);
            trajectory = false;
            dpmRobot.drawing = false;
            DPMDataHandler.WriteRobot();
            moveArrows.SetActive(false);
        }
        public void HideEditor()
        {
            trajectoryPanel.SetActive(false);
            dpmRobot.drawing = false;
            moveArrows.SetActive(false);
        }
        public void ShowEditor()
        {
            trajectoryPanel.SetActive(true);
            dpmRobot.drawing = true;
            moveArrows.SetActive(true);
        }
        /// <summary>
        /// Updates input field values of release position and velocity
        /// </summary>
        private void UpdateTrajectoryValues()
        {
            dp = dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]);
            IncrementTrajectoryValues();
            xOffsetEntry.GetComponent<InputField>().text = dp.releasePosition.x.ToString();
            yOffsetEntry.GetComponent<InputField>().text = dp.releasePosition.y.ToString();
            zOffsetEntry.GetComponent<InputField>().text = dp.releasePosition.z.ToString();
            releaseSpeedEntry.GetComponent<InputField>().text = dp.releaseVelocity.x.ToString();
            releaseVerticalEntry.GetComponent<InputField>().text = dp.releaseVelocity.y.ToString();
            releaseHorizontalEntry.GetComponent<InputField>().text = dp.releaseVelocity.z.ToString();
            RefreshMoveArrows();
        }
        #region increment on +- button pressed
        public void SetPositivePositionIncrement(int xyz)
        {
            positiveOffset = true;
            this.xyz = xyz;
            position = true;
        }
        public void SetNegativePositionIncrement(int xyz)
        {
            positiveOffset = false;
            this.xyz = xyz;
            position = true;
        }
        public void SetPositiveReleaseIncrement(int xyz)
        {
            positiveOffset = true;
            this.xyz = xyz;
            position = false;
        }
        public void SetNegativeReleaseIncrement(int xyz)
        {
            positiveOffset = false;
            this.xyz = xyz;
            position = false;
        }
        private void IncrementTrajectoryValues()
        {
            if (position)
            {
                switch (xyz)
                {
                    case 1:
                        if (positiveOffset) dp.releasePosition.x += offsetIncrement;
                        else dp.releasePosition.x -= offsetIncrement;
                        break;
                    case 2:
                        if (positiveOffset) dp.releasePosition.y += offsetIncrement;
                        else dp.releasePosition.y -= offsetIncrement;
                        break;
                    case 3:
                        if (positiveOffset) dp.releasePosition.z += offsetIncrement;
                        else dp.releasePosition.z -= offsetIncrement;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (xyz)
                {
                    case 1:
                        if (positiveOffset) dp.releaseVelocity.x += speedIncrement;
                        else dp.releaseVelocity.x -= speedIncrement;
                        break;
                    case 2:
                        if (positiveOffset) dp.releaseVelocity.y += angleIncrement;
                        else dp.releaseVelocity.y -= angleIncrement;
                        break;
                    case 3:
                        if (positiveOffset) dp.releaseVelocity.z += angleIncrement;
                        else dp.releaseVelocity.z -= angleIncrement;
                        break;
                    default:
                        break;
                }
            }
            DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0] = dp;
        }
        #endregion
        /// <summary>
        /// Set release position values to input fields and update mode
        /// </summary>
        /// <param name="xyz">coord of vector</param>
        public void PositionInput(int xyz)
        {
            InputControl.freeze = false;

            Vector3 releasePosition = DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0].releasePosition;
            switch (xyz)
            {
                case 1:
                    releasePosition.x = float.Parse(xOffsetEntry.GetComponent<InputField>().text);
                    break;
                case 2:
                    releasePosition.y = float.Parse(yOffsetEntry.GetComponent<InputField>().text);
                    break;
                case 3:
                    releasePosition.z = float.Parse(zOffsetEntry.GetComponent<InputField>().text);
                    break;
                default:
                    break;
            }
            editing = false;
            DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0].releasePosition = releasePosition;
        }
        /// <summary>
        /// Set release velocity values to input fields and update mode
        /// </summary>
        /// <param name="xyz">coord of vector</param>
        public void ReleaseInput(int xyz)
        {
            InputControl.freeze = false;

            Vector3 releaseVelocity = DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0].releaseVelocity;
            switch (xyz)
            {
                case 1:
                    releaseVelocity.x = float.Parse(releaseSpeedEntry.GetComponent<InputField>().text);
                    break;
                case 2:
                    releaseVelocity.y = float.Parse(releaseVerticalEntry.GetComponent<InputField>().text);
                    break;
                case 3:
                    releaseVelocity.z = float.Parse(releaseHorizontalEntry.GetComponent<InputField>().text);
                    break;
                default:
                    break;
            }
            editing = false;
            DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0].releaseVelocity = releaseVelocity;
        }
        /// <summary>
        /// on input field click
        /// </summary>
        public void StartEditing()
        {
            editing = true;
            InputControl.freeze = true; //freeze controls
        }
        public void StopEditing()
        {
            xyz = 0;
        }
        /// <summary>
        /// Reset vector to ZERO
        /// </summary>
        /// <param name="position">position or velocity</param>        
        public void ResetVectors(bool position)
        {
            if (position) DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0].releasePosition = Vector3.zero;
            else DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0].releaseVelocity = Vector3.zero;
        }
        /// <summary>
        /// toggle trajectory line display
        /// </summary>
        public void ToggleTrajectoryDisplay()
        {
            if (dpmRobot.drawing)
            {
                dpmRobot.drawing = false;
                showTrajectory.GetComponentInChildren<Text>().text = "Show Trajectory";
            }
            else
            {
                dpmRobot.drawing = true;
                showTrajectory.GetComponentInChildren<Text>().text = "Hide Trajectory";
            }
        }
        /// <summary>
        /// Draw trajectory line
        /// </summary>
        private void DrawTrajectory()
        {
            LineRenderer line = trajectoryLine.GetComponent<LineRenderer>();

            //look of the line
            line.startWidth = 0.2f;
            line.material = Resources.Load("Materials/Projection") as Material;
            line.startColor = Color.blue;
            line.endColor = Color.cyan;

            //show line
            line.enabled = true;

            DriverPractice dp = DPMDataHandler.dpmodes.Where(d => d.gamepiece.Equals(FieldDataHandler.gamepieces[gamepieceIndex].name)).ToArray()[0];
            GameObject releaseNode = Auxiliary.FindObject(dpmRobot.gameObject, dp.releaseNode);

            int verts = 100; //This determines how far along time the illustration goes.
            line.positionCount = verts;

            UnityEngine.Vector3 pos = releaseNode.transform.position + releaseNode.GetComponent<BRigidBody>().transform.rotation * dp.releasePosition;
            UnityEngine.Vector3 vel = releaseNode.GetComponent<BRigidBody>().velocity + releaseNode.transform.rotation * VelocityToVector3(dp.releaseVelocity);
            UnityEngine.Vector3 grav = GameObject.Find("BulletPhysicsWorld").GetComponent<BPhysicsWorld>().gravity;
            for (int i = 0; i < verts; i++)
            {
                line.SetPosition(i, pos);
                vel = vel + grav * Time.fixedDeltaTime;
                pos = pos + vel * Time.fixedDeltaTime;
            }
        }
        /// <summary>
        /// Convert release velocity vector to actual line velocity
        /// </summary>
        private UnityEngine.Vector3 VelocityToVector3(Vector3 release)
        {
            UnityEngine.Quaternion horVector;
            UnityEngine.Quaternion verVector;
            UnityEngine.Vector3 finalVector = UnityEngine.Vector3.zero;

            horVector = UnityEngine.Quaternion.AngleAxis(release.z, UnityEngine.Vector3.up);
            verVector = UnityEngine.Quaternion.AngleAxis(release.y, UnityEngine.Vector3.right);

            UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(release.y, release.z, 0);

            finalVector = (UnityEngine.Quaternion.LookRotation(UnityEngine.Vector3.forward, UnityEngine.Vector3.up) * horVector * verVector) * UnityEngine.Vector3.forward * release.x;

            return (finalVector);
        }
        /// <summary>
        /// Refreshes the position of the move arrows with the position offsets.
        /// </summary>
        public void RefreshMoveArrows()
        {
            dp = dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]);
            GameObject releaseNode = Auxiliary.FindObject(dpmRobot.gameObject, dp.releaseNode);
            moveArrows.transform.parent = releaseNode.transform;
            moveArrows.transform.localPosition = dp.releasePosition;
        }
        /// <summary>
        /// Creates a <see cref="GameObject"/> instantiated from the MoveArrows prefab.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private GameObject CreateMoveArrows()
        {
            dp = dpmRobot.GetDriverPractice(FieldDataHandler.gamepieces[gamepieceIndex]);
            GameObject releaseNode = Auxiliary.FindObject(dpmRobot.gameObject, dp.releaseNode);
            GameObject arrows = Instantiate(Resources.Load<GameObject>("Prefabs\\MoveArrows"));
            arrows.name = "ReleasePositionMoveArrows";
            arrows.transform.parent = releaseNode.transform;
            arrows.transform.localPosition = dp.releasePosition;
            arrows.transform.localRotation = dpmRobot.gameObject.transform.localRotation;

            arrows.GetComponent<MoveArrows>().Translate = (translation) =>
            {
                arrows.transform.position += translation;
                dp.releasePosition = arrows.transform.localPosition;
            };

            arrows.GetComponent<MoveArrows>().OnClick = () => dpmRobot.gameObject.GetComponent<SimulatorRobot>().LockRobot();
            arrows.GetComponent<MoveArrows>().OnRelease = () => dpmRobot.gameObject.GetComponent<SimulatorRobot>().UnlockRobot();

            StateMachine.SceneGlobal.Link<MainState>(arrows, false);

            return arrows;
        }
        /// <summary>
        /// Change trajectory line and move arrows to new active robot
        /// </summary>
        private void OnActiveRobotChange()
        {
            dpmRobot = mainState.ActiveRobot.GetDriverPractice();
            trajectoryLine.transform.parent = dpmRobot.transform;
            Destroy(moveArrows);
            moveArrows = CreateMoveArrows();
        }
    }
}
