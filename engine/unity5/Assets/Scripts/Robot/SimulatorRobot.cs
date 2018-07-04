using BulletSharp;
using BulletUnity;
using Synthesis.Camera;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.MixAndMatch;
using Synthesis.RN;
using Synthesis.Sensors;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.Robot
{
    public class SimulatorRobot : RobotBase
    {
        public bool IsResetting { get; private set; } = false;

        private const float ResetVelocity = 0.05f;
        private const float HoldTime = 0.8f;

        private DriverPracticeRobot dpmRobot;

        private Vector3 nodeToRobotOffset;

        private float keyDownTime = 0f;
        private RobotCameraManager robotCameraManager;

        private SensorManager sensorManager;

        protected override void OnInitializeRobot()
        {
            //Detach and destroy all sensors on the original robot
            SensorManager sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            sensorManager.ResetSensorLists();

            //Removes Driver Practice component if it exists
            if (dpmRobot != null)
                Destroy(dpmRobot);
        }

        protected override void OnRobotSetup()
        {
            //Get the offset from the first node to the robot for new robot start position calculation
            //This line is CRITICAL to new reset position accuracy! DON'T DELETE IT!
            nodeToRobotOffset = gameObject.transform.GetChild(0).localPosition - robotStartPosition;

            //Initializing robot cameras
            bool hasRobotCamera = false;
            //If you are getting an error referencing this line, it is likely that the Game Object "RobotCameraList" in Scene.unity does not have the RobotCameraManager script attached to it.
            robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();

            //Loop through robotCameraList and check if any existing camera should attach to this robot
            foreach (GameObject robotCamera in robotCameraManager.GetRobotCameraList())
            {
                if (robotCamera.GetComponent<RobotCamera>().robot.Equals(this))
                {
                    //Recover the robot camera configurations
                    robotCamera.GetComponent<RobotCamera>().RecoverConfiguration();
                    hasRobotCamera = true;
                }

            }

            //Add new cameras to the robot if there is none robot camera belong to the current robot (which means it is a new robot)
            if (!hasRobotCamera)
            {
                //Attached to the main frame and face the front
                robotCameraManager.AddCamera(this, transform.GetChild(0).transform, new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0));
                ////Attached to main frame and face the back
                robotCameraManager.AddCamera(this, transform.GetChild(0).transform, new Vector3(0, 0.5f, 0), new Vector3(0, 180, 0));
                robotCameraManager.AddCamera(this, transform.GetChild(0).transform);
            }
        }

        protected override bool ConstructRobot(List<RigidNode_Base> nodes, int numWheels, ref float collectiveMass)
        {
            if (!base.ConstructRobot(nodes, numWheels, ref collectiveMass))
                return false;

            foreach (RigidNode_Base n in nodes)
            {
                RigidNode node = (RigidNode)n;

                if (node.MainObject.GetComponent<BRigidBody>() != null)
                    node.MainObject.AddComponent<Tracker>().Trace = true;
            }

            return true;
        }

        protected override void UpdateTransform()
        {
            base.UpdateTransform();

            if (IsResetting)
                return;

            // TODO: Utilize the state machine here if possible
            if (InputControl.GetButtonDown(Controls.buttons[ControlIndex].resetRobot) && !MixAndMatchMode.setPresetPanelOpen)
            {
                keyDownTime = Time.time;
            }
            else if (InputControl.GetButtonDown(Controls.buttons[ControlIndex].resetField))
            {
                Auxiliary.FindObject(GameObject.Find("Canvas"), "LoadingPanel").SetActive(true);
                SceneManager.LoadScene("Scene");
            }
            else if (InputControl.GetButton(Controls.buttons[ControlIndex].resetRobot) && !MixAndMatchMode.setPresetPanelOpen &&
                !State.DynamicCameraObject.GetComponent<DynamicCamera>().cameraState.GetType().Equals(typeof(DynamicCamera.ConfigurationState)))
            {
                if (Time.time - keyDownTime > HoldTime)
                    BeginReset();
            }
            else if (InputControl.GetButtonUp(Controls.buttons[ControlIndex].resetRobot) && !MixAndMatchMode.setPresetPanelOpen)
            {
                BeginReset();
                EndReset();
            }
        }

        protected override void UpdatePhysics()
        {
            if (IsResetting)
                Resetting();
        }

        /// <summary>
        /// Return the robot to robotStartPosition and destroy extra game pieces
        /// </summary>
        /// <param name="resetTransform"></param>
        public void BeginReset()
        {
            BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

            if (rigidBody != null && !rigidBody.GetCollisionObject().IsActive)
                rigidBody.GetCollisionObject().Activate();

            if (!State.DynamicCameraObject.GetComponent<DynamicCamera>().cameraState.GetType().Equals(typeof(DynamicCamera.ConfigurationState)))
            {
                Debug.Log(State.DynamicCameraObject.GetComponent<DynamicCamera>().cameraState);
                IsResetting = true;

                foreach (RigidNode n in RootNode.ListAllNodes())
                {
                    BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                    if (br == null)
                        continue;

                    RigidBody r = (RigidBody)br.GetCollisionObject();

                    r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                    r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;

                    BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                    newTransform.Origin = (robotStartPosition + n.ComOffset).ToBullet();
                    newTransform.Basis = BulletSharp.Math.Matrix.Identity;
                    r.WorldTransform = newTransform;
                }

                OnBeginReset();

                //Where "save orientation" works
                RotateRobot(robotStartOrientation);

                GameObject.Find("Robot").transform.GetChild(0).transform.position = new Vector3(10, 20, 5);
                if (IsResetting)
                {
                    Debug.Log("is resetting!");
                }
            }
            else
            {
                UserMessageManager.Dispatch("Please don't reset robot during configuration!", 5f);
            }
        }

        /// <summary>
        /// Can move robot around in this state using WASD, update robotStartPosition if hit enter
        /// </summary>
        void Resetting()
        {
            if (UnityEngine.Input.GetMouseButton(1))
            {
                //Transform rotation along the horizontal plane
                Vector3 rotation = new Vector3(0f,
                    UnityEngine.Input.GetKey(KeyCode.D) ? ResetVelocity : UnityEngine.Input.GetKey(KeyCode.A) ? -ResetVelocity : 0f,
                    0f);
                if (!rotation.Equals(Vector3.zero))
                    RotateRobot(rotation);
            }
            else
            {
                //Transform position
                Vector3 transposition = new Vector3(
                    UnityEngine.Input.GetKey(KeyCode.W) ? ResetVelocity : UnityEngine.Input.GetKey(KeyCode.S) ? -ResetVelocity : 0f,
                    0f,
                    UnityEngine.Input.GetKey(KeyCode.A) ? ResetVelocity : UnityEngine.Input.GetKey(KeyCode.D) ? -ResetVelocity : 0f);

                if (!transposition.Equals(Vector3.zero))
                    TransposeRobot(transposition);
            }

            //Update robotStartPosition when hit enter
            if (UnityEngine.Input.GetKey(KeyCode.Return))
            {
                robotStartOrientation = ((RigidNode)RootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;

                robotStartPosition = new Vector3(transform.GetChild(0).transform.localPosition.x - nodeToRobotOffset.x, robotStartPosition.y,
                    transform.GetChild(0).transform.localPosition.z - nodeToRobotOffset.z);
                EndReset();
            }
        }

        /// <summary>
        /// End the reset process and puts the robot back down
        /// </summary>
        public void EndReset()
        {
            IsResetting = false;

            foreach (RigidNode n in RootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
            }

            OnEndReset();

            foreach (Tracker t in GetComponentsInChildren<Tracker>())
                t.Clear();
        }

        /// <summary>
        /// Shifts the robot by a set position vector
        /// </summary>
        public void TransposeRobot(Vector3 transposition)
        {
            foreach (RigidNode n in RootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                newTransform.Origin += transposition.ToBullet();
                r.WorldTransform = newTransform;
            }

            OnTransposeRobot(transposition);
        }

        /// <summary>
        /// Rotates the robot about its origin by a set vector
        /// </summary>
        public void RotateRobot(Vector3 rotation)
        {
            RotateRobot(BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.y, rotation.z, rotation.x));
        }

        /// <summary>
        /// Resets the robot orientation to how the CAD model was originally defined (should be standing upright and facing forward if CAD was done properly)
        /// </summary>
        public void ResetRobotOrientation()
        {
            robotStartOrientation = BulletSharp.Math.Matrix.Identity;
            BeginReset();
            EndReset();
        }

        /// <summary>
        /// Saves the robot's current orientation to be used whenever robot is reset
        /// </summary>
        public void SaveRobotOrientation()
        {
            robotStartOrientation = ((RigidNode)RootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
            robotStartOrientation.ToUnity();
        }

        /// <summary>
        /// Cancel current orientation & spawnpoint changes
        /// </summary>
        public void CancelRobotOrientation()
        {
            if (IsResetting)
            {
                BeginReset();
                EndReset();
            }
        }

        /// <summary>
        /// Returns the driver practice component of this robot
        /// </summary>
        public DriverPracticeRobot GetDriverPractice()
        {
            return GetComponent<DriverPracticeRobot>();
        }

        protected virtual void OnBeginReset() { }

        protected virtual void OnEndReset() { }

        protected virtual void OnTransposeRobot(Vector3 transposition) { }
    }
}
