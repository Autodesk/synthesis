using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;
using UnityEngine.UI;
using Synthesis.FSM;
using System;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Robot;
using Synthesis.Configuration;

namespace Synthesis.Sensors
{
    /// <summary>
    /// This is the template/parent class for all sensors within Synthesis.
    /// </summary>
    public abstract class SensorBase : MonoBehaviour
    {
        public bool IsChangingPosition { get; set; }
        public bool IsChangingHeight { get; set; }
        public bool IsChangingAngle { get; set; }
        public bool IsChangingRange { get; set; }
        private static float positionSpeed = 0.5f;
        private static float rotationSpeed = 25;
        public bool IsVisible = true;
        protected bool IsMetric = false;
        protected MainState main;
        public SimulatorRobot Robot { get; set; }

        public string sensorType; //Ultrasonic, Beam Break, or Gyro. Used for when the sensor gets deleted

        // Use this for initialization
        void Start()
        {
            IsVisible = true;

            MoveArrows moveArrows = GetComponentInChildren<MoveArrows>();
            moveArrows.Translate = (translation) => transform.Translate(translation, Space.World);
            moveArrows.OnClick = () => Robot.LockRobot();
            moveArrows.OnRelease = () => Robot.UnlockRobot();

            StateMachine.SceneGlobal.Link<MainState>(moveArrows.gameObject, false);
        }

        // Update is called once per frame
        void Update()
        {
            if (main == null)
            {
                main = StateMachine.SceneGlobal.FindState<MainState>();
            }
        }

        public abstract float ReturnOutput();

        /// <summary>
        /// Update the configuration transform of the sensor
        /// </summary>
        public void UpdateTransform()
        {
            if (IsChangingPosition)
            {
                if (IsChangingRange) //Control range transform
                {
                    UpdateRangeTransform();
                }
                else if (IsChangingAngle) //Control rotation (only when the angle panel is active)
                {
                    UpdateAngleTransform();
                }
                else if (!IsChangingHeight) //Control horizontal plane transform
                {
                    UpdateHorizontalPlaneTransform();
                }
                else //Control height transform
                {
                    UpdateHeightTransform();
                }
            }
        }

        /// <summary>
        /// Return the range of sensor
        /// </summary>
        /// <returns></returns> range of sensor
        public virtual float GetSensorRange()
        {
            return 0;
        }

        /// <summary>
        /// Set the range of sensor
        /// </summary>
        /// <param name="range"></param>
        public virtual void SetSensorRange(float range, bool isEditing = false)
        {
        }

        /// <summary>
        /// Change angle by a specified amount
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void RotateTransform(float x, float y, float z)
        {
            //restrict angles to between 0 and 360
            x = x < 0 ? 360 + x : x;
            x = x > 360 ? x - 360 : x;

            y = y < 0 ? 360 + y : y;
            y = y > 360 ? y - 360 : y;

            z = z < 0 ? 360 + z : z;
            z = z > 360 ? z - 360 : z;

            //actually rotate the sensor
            transform.Rotate(new Vector3(x, y, z));
            UpdateAngleTransform();
        }

        /// <summary>
        /// Change angle using W/S
        /// </summary>
        public virtual void UpdateAngleTransform()
        {
            transform.Rotate(new Vector3(-Input.InputControl.GetAxis(Input.Controls.Global.GetAxes().cameraVertical) * rotationSpeed, Input.InputControl.GetAxis(Input.Controls.Global.GetAxes().cameraHorizontal) * rotationSpeed, 0) * Time.deltaTime);
        }

        /// <summary>
        /// Change height using W/S
        /// </summary>
        public virtual void UpdateHeightTransform()
        {
            transform.Translate(new Vector3(0, Input.InputControl.GetAxis(Input.Controls.Global.GetAxes().cameraVertical) * positionSpeed, 0) * Time.deltaTime);
        }

        /// <summary>
        /// Change horizontal plane position using WASD
        /// </summary>
        public virtual void UpdateHorizontalPlaneTransform()
        {
            transform.Translate(new Vector3(Input.InputControl.GetAxis(Input.Controls.Global.GetAxes().cameraHorizontal) * positionSpeed, 0, Input.InputControl.GetAxis(Input.Controls.Global.GetAxes().cameraVertical) * positionSpeed) * Time.deltaTime);
        }

        /// <summary>
        /// Change the range of the sensor depending on how range is defined for each type of sensor, mostly using W/S
        /// </summary>
        public virtual void UpdateRangeTransform()
        {
        }

        /// <summary>
        /// Update the sensor output at the corresponding sensorOutputPanel
        /// </summary>
        public virtual void UpdateOutputDisplay()
        {
            GameObject outputPanel = GameObject.Find(gameObject.name + "_Panel");
            if (outputPanel != null)
            {
                GameObject inputField = Auxiliary.FindObject(outputPanel, "Entry");
                inputField.GetComponent<InputField>().text = Math.Round(ReturnOutput(), 3).ToString();
            }
        }

        /// <summary>
        /// Only used for gyro right now
        /// </summary>
        public virtual void ResetSensorReading()
        {
        }

        /// <summary>
        /// Terminate all configuration state
        /// </summary>
        public void ResetConfigurationState()
        {
            IsChangingPosition = IsChangingAngle = IsChangingHeight = IsChangingRange = false;
        }

        /// <summary>
        /// Change the visibility of the sensor
        /// </summary>
        /// <param name="visible"></param>
        public void ChangeVisibility(bool visible)
        {
            IsVisible = visible;
            SyncVisibility();
        }

        /// <summary>
        /// Set the sensor to be visible temporarily, for choosing sensor option
        /// </summary>
        public void SetTemporaryVisible()
        {
            if (gameObject.GetComponent<Renderer>() != null) gameObject.GetComponent<Renderer>().enabled = true;
            foreach (Transform child in gameObject.transform)
            {
                if (child.GetComponent<Renderer>() != null) child.GetComponent<Renderer>().enabled = true;
            }
        }

        /// <summary>
        /// Update the sensor visibility with its state
        /// </summary>
        public void SyncVisibility()
        {
            if (gameObject.GetComponent<Renderer>() != null) gameObject.GetComponent<Renderer>().enabled = IsVisible;
            foreach (Transform child in gameObject.transform)
            {
                if (child.GetComponent<Renderer>() != null) child.GetComponent<Renderer>().enabled = IsVisible;
            }
        }
    }
}
