using BulletSharp;
using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.FEA;
using Synthesis.MixAndMatch;
using Synthesis.RN;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Robot
{
    public class MaMRobot : SimulatorRobot
    {
        /// <summary>
        /// The manipulator <see cref="GameObject"/> reference.
        /// </summary>
        public GameObject ManipulatorObject { get; private set; }

        /// <summary>
        /// If true, this robot has a manipulator.
        /// </summary>
        public bool RobotHasManipulator { get; set; }
        

        private RigidNode manipulatorNode;

        private string wheelPath;

        private float wheelRadius;
        private float wheelFriction;
        private float wheelLateralFriction;
        private float wheelMass;

        private Vector3 manipulatorOffset;

        private bool robotIsMecanum;

        // TODO: Something weird is going on with the spawn, at least with robots with manipulators. Reset is fine.

        /// <summary>
        /// Loads and initializes the physical manipulator object (used in Mix and Match mode)
        /// </summary>
        /// <param name="directory">Folder directory of the manipulator</param>
        /// <param name="robotGameObject">GameObject of the robot the manipulator will be attached to</param>
        public bool InitializeManipulator(string directory)
        {
            ManipulatorObject = new GameObject("Manipulator");
            ManipulatorObject.transform.position = robotStartPosition + manipulatorOffset;

            RigidNode_Base.NODE_FACTORY = delegate (Guid guid) { return new RigidNode(guid); };

            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            //TO-DO: Read .robot instead (from the new exporters if they are implemented). Maybe need a RobotSkeleton class
            
            manipulatorNode = BXDExtensions.ReadSkeletonSafe(directory + Path.DirectorySeparatorChar + "skeleton") as RigidNode ;
            
            manipulatorNode.ListAllNodes(nodes);

            //Load node_0 for attaching manipulator to robot
            RigidNode node = (RigidNode)nodes[0];

            node.CreateTransform(ManipulatorObject.transform);
            if (!node.CreateMesh(directory + Path.DirectorySeparatorChar + node.ModelFileName))
            {
                Destroy(ManipulatorObject);
                return false;
            }

            node.CreateManipulatorJoint(gameObject);
            node.MainObject.AddComponent<Tracker>().Trace = true;

            //Load other nodes associated with the manipulator
            for (int i = 1; i < nodes.Count; i++)
            {
                RigidNode otherNode = (RigidNode)nodes[i];
                otherNode.CreateTransform(ManipulatorObject.transform);

                if (!otherNode.CreateMesh(directory + Path.DirectorySeparatorChar + otherNode.ModelFileName))
                {
                    Destroy(ManipulatorObject);
                    return false;
                }

                otherNode.MainObject.AddComponent<Tracker>().Trace = true;
            }

            RootNode.GenerateWheelInfo();

            for (int i = 1; i < nodes.Count; i++)
            {
                RigidNode otherNode = (RigidNode)nodes[i];
                otherNode.CreateJoint(this);
            }

            RotateRobot(robotStartOrientation);
            RobotHasManipulator = true;

            return true;
        }

        /// <summary>
        /// Deletes robot manipulator (used for Mix and Match mode)
        /// </summary>
        public void DeleteManipulatorNodes()
        {
            //Deletes all nodes if any exist, take the old node transforms out from the robot object
            int childCount = ManipulatorObject.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = ManipulatorObject.transform.GetChild(i);

                //If this isn't done, the game object is destroyed but the parent-child transform relationship remains!
                child.parent = null;
                Destroy(child.gameObject);
            }

            Destroy(ManipulatorObject);
        }

        /// <summary>
        /// Sets the wheel and drivetrain properties of the robot just before generation.
        /// </summary>
        protected override void OnInitializeRobot()
        {
            base.OnInitializeRobot();

            wheelPath = RobotTypeManager.WheelPath;
            wheelFriction = RobotTypeManager.WheelFriction;
            wheelLateralFriction = RobotTypeManager.WheelLateralFriction;
            wheelRadius = RobotTypeManager.WheelRadius;
            wheelMass = RobotTypeManager.WheelMass;
            robotIsMecanum = RobotTypeManager.IsMecanum;
        }

        /// <summary>
        /// Reads the robot's manipulator offset just after the robot is generated.
        /// </summary>
        protected override void OnRobotSetup()
        {
            base.OnRobotSetup();

            manipulatorOffset = Vector3.zero;

            try
            {
                using (TextReader reader = File.OpenText(RobotDirectory + Path.DirectorySeparatorChar + "position.txt"))
                {
                    manipulatorOffset.x = float.Parse(reader.ReadLine());
                    manipulatorOffset.y = float.Parse(reader.ReadLine());
                    manipulatorOffset.z = float.Parse(reader.ReadLine());
                }
            }
            catch
            {
                manipulatorOffset = Vector3.zero;
            }
        }

        /// <summary>
        /// Generates the robot from the list of <see cref="RigidNode_Base"/>s and the
        /// number of wheels, and updates the collective mass.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="numWheels"></param>
        /// <param name="collectiveMass"></param>
        /// <returns></returns>
        protected override bool ConstructRobot(List<RigidNode_Base> nodes, ref float collectiveMass)
        {
            if (IsMecanum())
                return base.ConstructRobot(nodes, ref collectiveMass);

            //Load Node_0, the base of the robot
            RigidNode node = (RigidNode)nodes[0];
            node.CreateTransform(transform);

            if (!node.CreateMesh(RobotDirectory + Path.DirectorySeparatorChar + node.ModelFileName, true, wheelMass))
                return false;

            node.CreateJoint(this);

            if (node.PhysicalProperties != null)
                collectiveMass += node.PhysicalProperties.mass;

            if (node.MainObject.GetComponent<BRigidBody>() != null)
                node.MainObject.AddComponent<Tracker>().Trace = true;

            //Get the wheel mesh data from the file they are stored in. They are stored as .bxda files. This may need to update if exporters/file types change.
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(wheelPath + Path.DirectorySeparatorChar + "node_0.bxda");

            List<Mesh> meshList = new List<Mesh>();
            List<Material[]> materialList = new List<Material[]>();

            RigidNode wheelNode = (RigidNode)BXDJSkeleton.ReadSkeleton(wheelPath + Path.DirectorySeparatorChar + "skeleton.bxdj");

            Material[] materials = new Material[0];
            Auxiliary.ReadMeshSet(mesh.meshes, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
            {
                meshList.Add(meshu);

                materials = new Material[meshu.subMeshCount];

                for (int i = 0; i < materials.Length; i++)
                    materials[i] = sub.surfaces[i].AsMaterial(true);

                materialList.Add(materials);
            }, true);

            //Loads the other nodes from the original robot
            for (int i = 1; i < nodes.Count; i++)
            {
                node = (RigidNode)nodes[i];
                node.CreateTransform(transform);

                if (!node.CreateMesh(RobotDirectory + Path.DirectorySeparatorChar + node.ModelFileName, true, wheelMass))
                    return false;

                //If the node is a wheel, destroy the original wheel mesh and replace it with the wheels selected in MaM
                if (node.HasDriverMeta<WheelDriverMeta>())
                {
                    int chldCount = node.MainObject.transform.childCount;

                    for (int j = 0; j < chldCount; j++)
                        Destroy(node.MainObject.transform.GetChild(j).gameObject);

                    int k = 0;

                    Vector3? offset = null;
                    foreach (Mesh meshObject in meshList)
                    {
                        GameObject meshObj = new GameObject(node.MainObject.name + "_mesh");
                        meshObj.transform.parent = node.MainObject.transform;
                        meshObj.AddComponent<MeshFilter>().mesh = meshObject;

                        if (!offset.HasValue)
                            offset = meshObject.bounds.center;

                        meshObj.transform.localPosition = -offset.Value;

                        //Take out this line if you want some snazzy pink wheels
                        meshObj.AddComponent<MeshRenderer>().materials = materialList[k];

                        k++;
                    }
                    node.MainObject.GetComponentInChildren<MeshRenderer>().materials = materials;
                }
            }

            RootNode.GenerateWheelInfo();

            //Create the joints that interact with physics
            for (int i = 1; i < nodes.Count; i++)
            {
                node = (RigidNode)nodes[i];
                node.CreateJoint(this, wheelFriction, wheelLateralFriction);

                if (node.HasDriverMeta<WheelDriverMeta>())
                    node.MainObject.GetComponent<BRaycastWheel>().Radius = wheelRadius;

                if (node.PhysicalProperties != null)
                    collectiveMass += node.PhysicalProperties.mass;

                if (node.MainObject.GetComponent<BRigidBody>() != null)
                    node.MainObject.AddComponent<Tracker>().Trace = true;

            }

            return true;
        }

        /// <summary>
        /// Returns true if the robot has a mecanum drive.
        /// </summary>
        /// <returns></returns>
        public override bool IsMecanum()
        {
            return robotIsMecanum;
        }

        /// <summary>
        /// Updates the motors of the robot.
        /// </summary>
        protected override void UpdateMotors(float[] pwm = null)
        {
            base.UpdateMotors(pwm);

            if (RobotHasManipulator)
                DriveJoints.UpdateManipulatorMotors(manipulatorNode, emptyDIO, ControlIndex);
        }

        /// <summary>
        /// Called when the robot begins to reset.
        /// </summary>
        protected override void OnBeginReset()
        {
            if (!RobotHasManipulator)
                return;

            int i = 0;
            foreach (RigidNode n in manipulatorNode.ListAllNodes())
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

                if (i == 0)
                    Debug.Log("Transform Origin" + newTransform.Origin);

                i++;
            }
        }

        /// <summary>
        /// Called when resetting is complete.
        /// </summary>
        protected override void OnEndReset()
        {
            if (!RobotHasManipulator)
                return;

            foreach (RigidNode n in manipulatorNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
            }
        }

        /// <summary>
        /// Called when the robot is moved.
        /// </summary>
        /// <param name="transposition"></param>
        protected override void OnTransposeRobot(Vector3 transposition)
        {
            if (!RobotHasManipulator)
                return;

            foreach (RigidNode n in manipulatorNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                newTransform.Origin += transposition.ToBullet();
                r.WorldTransform = newTransform;
            }
        }
    }
}