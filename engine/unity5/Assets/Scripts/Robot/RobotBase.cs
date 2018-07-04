using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.DriverPractice;
using Synthesis.FEA;
using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.MixAndMatch;
using Synthesis.RN;
using Synthesis.Camera;
using Synthesis.Sensors;
using Synthesis.StatePacket;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using BulletSharp;
using Synthesis.GUI;

namespace Synthesis.Robot
{
    /// <summary>
    /// To be attached to all robot parent objects.
    /// Handles all robot-specific interaction such as driving joints, resetting, and orienting robot.
    /// </summary>
    public class RobotBase : StateBehaviour<MainState>
    {
        public UnityPacket.OutputStatePacket Packet { get; set; }

        public int ControlIndex { get; set; } = 0;

        public string RobotDirectory { get; private set; }
        public string RobotName { get; private set; }

        public float Speed { get; private set; }
        public float Weight { get; private set; }
        public float AngularVelocity { get; private set; }
        public float Acceleration { get; private set; }

        protected RigidNode_Base RootNode { get; private set; }
        
        protected Vector3 robotStartPosition = new Vector3(0f, 1f, 0f);
        protected BulletSharp.Math.Matrix robotStartOrientation = BulletSharp.Math.Matrix.Identity;

        protected readonly UnityPacket.OutputStatePacket.DIOModule[] emptyDIO = new UnityPacket.OutputStatePacket.DIOModule[2];

        private float oldSpeed;

        /// <summary>
        /// Called once per frame to ensure all rigid bodie components are activated
        /// </summary>
        void Update()
        {
            UpdateTransform();
        }

        /// <summary>
        /// Called once every physics step (framerate independent) to drive motor joints as well as handle the resetting of the robot
        /// </summary>
        void FixedUpdate()
        {
            if (RootNode != null)
                UpdateMotors();

            UpdatePhysics();
        }

        /// <summary>
        /// Initializes physical robot based off of robot directory.
        /// </summary>
        /// <param name="directory">folder directory of robot</param>
        /// <returns></returns>
        public bool InitializeRobot(string directory)
        {
            RobotDirectory = directory;
            RobotName = new DirectoryInfo(directory).Name;

            //Deletes all nodes if any exist, take the old node transforms out from the robot object
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                //If this isn't done, the game object is destroyed but the parent-child transform relationship remains!
                child.parent = null;
                Destroy(child.gameObject);
            }

            transform.position = robotStartPosition; //Sets the position of the object to the set spawn point

            if (!File.Exists(directory + "\\skeleton.bxdj"))
                return false;

            OnInitializeRobot();

            //Loads the node and skeleton data
            RigidNode_Base.NODE_FACTORY = delegate (Guid guid) { return new RigidNode(guid); };

            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            RootNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
            RootNode.ListAllNodes(nodes);

            //Initializes the wheel variables
            int numWheels = nodes.Count(x => x.HasDriverMeta<WheelDriverMeta>() && x.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL);
            float collectiveMass = 0f;

            if (!ConstructRobot(nodes, numWheels, ref collectiveMass))
                return false;

            foreach (BRaycastRobot r in GetComponentsInChildren<BRaycastRobot>())
            {
                r.RaycastRobot.OverrideMass = collectiveMass;
                r.RaycastRobot.RootRigidBody = (RigidBody)((RigidNode)nodes[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            }

            RotateRobot(robotStartOrientation);

            OnRobotSetup();

            return true;
        }

        /// <summary>
        /// Rotates the robot about its origin by a mathematical 4x4 matrix
        /// </summary>
        public void RotateRobot(BulletSharp.Math.Matrix rotationMatrix)
        {
            BulletSharp.Math.Vector3? origin = null;

            foreach (RigidNode n in RootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                if (origin == null)
                    origin = r.CenterOfMassPosition;

                BulletSharp.Math.Matrix rotationTransform = new BulletSharp.Math.Matrix
                {
                    Basis = rotationMatrix,
                    Origin = BulletSharp.Math.Vector3.Zero
                };

                BulletSharp.Math.Matrix currentTransform = r.WorldTransform;
                BulletSharp.Math.Vector3 pos = currentTransform.Origin;
                currentTransform.Origin -= origin.Value;
                currentTransform *= rotationTransform;
                currentTransform.Origin += origin.Value;

                r.WorldTransform = currentTransform;
            }
        }

        /// <summary>
        /// Get the total weight of the robot
        /// </summary>
        /// <returns></returns>
        public float GetWeight()
        {
            float weight = 0;

            foreach (Transform child in gameObject.transform)
                if (child.GetComponent<BRigidBody>() != null)
                    weight += child.GetComponent<BRigidBody>().mass;

            return weight;
        }

        /// <summary>
        /// Returns true if the robot has a mecanum drive.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsMecanum()
        {
            return RootNode.GetDriverMeta<WheelDriverMeta>().type == WheelType.MECANUM;
        }
        
        protected virtual bool ConstructRobot(List<RigidNode_Base> nodes, int numWheels, ref float collectiveMass)
        {
            //Initializes the nodes
            foreach (RigidNode_Base n in nodes)
            {
                RigidNode node = (RigidNode)n;
                node.CreateTransform(transform);

                if (!node.CreateMesh(RobotDirectory + "\\" + node.ModelFileName))
                {
                    Debug.LogError("Robot not loaded!");
                    return false;
                }

                node.CreateJoint(numWheels, false); // TODO: Real mix and match detection.

                if (node.PhysicalProperties != null)
                    collectiveMass += node.PhysicalProperties.mass;
            }

            return true;
        }

        protected virtual void UpdateMotors()
        {
            // TODO: Real mecanum detection.
            if (Packet != null)
                DriveJoints.UpdateAllMotors(RootNode, Packet.dio, ControlIndex, false/*robotIsMecanum*/);
            else
                DriveJoints.UpdateAllMotors(RootNode, emptyDIO, ControlIndex, false/*robotIsMecanum*/);
        }

        /// <summary>
        /// Update the stats for robot depending on whether it's metric or not
        /// </summary>
        protected virtual void UpdatePhysics()
        {
            GameObject mainNode = transform.GetChild(0).gameObject;
            //calculates stats of robot
            if (mainNode != null)
            {
                float currentSpeed = mainNode.GetComponent<BRigidBody>().GetCollisionObject().InterpolationLinearVelocity.Length;

                Speed = (float)Math.Round(Math.Abs(currentSpeed), 3);
                Weight = (float)Math.Round(GetWeight(), 3);
                AngularVelocity = (float)Math.Round(Math.Abs(mainNode.GetComponent<BRigidBody>().angularVelocity.magnitude), 3);
                Acceleration = (float)Math.Round((currentSpeed - oldSpeed) / Time.deltaTime, 3);
                oldSpeed = currentSpeed;
                if (!State.IsMetric)
                {
                    Speed = (float)Math.Round(Speed * 3.28084, 3);
                    Acceleration = (float)Math.Round(Acceleration * 3.28084, 3);
                    Weight = (float)Math.Round(Weight * 2.20462, 3);
                }
            }
        }

        protected virtual void UpdateTransform()
        {
            BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

            if (rigidBody == null)
            {
                AppModel.ErrorToMenu("Could not generate robot physics data.");
                return;
            }

            if (!rigidBody.GetCollisionObject().IsActive)
                rigidBody.GetCollisionObject().Activate();
        }

        protected virtual void OnInitializeRobot() { }

        protected virtual void OnRobotSetup() { }
    }
}
