//using Assets.Scripts;
//using BulletSharp;
//using BulletUnity;
//using Synthesis.BUExtensions;
//using Synthesis.FSM;
//using Synthesis.Input;
//using Synthesis.Network;
//using Synthesis.Robot;
//using Synthesis.States;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace Synthesis.Network
//{
//    [NetworkSettings(channel = 0, sendInterval = 0.1f)]
//    public partial class NetworkRobot : RobotBase
//    {
//        private const float CorrectionPositionThreshold = 0.05f;
//        private const float CorrectionRotationThreshold = 15.0f;

//        private BRigidBody[] rigidBodies;
//        private NetworkMesh[] networkMeshes;

//        //private MultiplayerState state;

//        /// <summary>
//        /// Initializes the NetworkRobot instance.
//        /// </summary>
//        private void Awake()
//        {
//            StateMachine.SceneGlobal.Link<MultiplayerState>(this, false);
//        }

//        /// <summary>
//        /// Creates a reference to the active multiplayer state when this <see cref="NetworkRobot"/>
//        /// is enabled.
//        /// </summary>
//        private void OnEnable()
//        {
//            state = StateMachine.SceneGlobal.CurrentState as MultiplayerState;
//        }

//        /// <summary>
//        /// Loads the Robot and initializes it on the network.
//        /// </summary>
//        private void Start()
//        {
//            string directory = GetComponent<PlayerIdentity>().RobotFolder;

//            if (!string.IsNullOrEmpty(directory))
//            {
//                state.LoadRobot(this, directory, isLocalPlayer);
//                rigidBodies = GetComponentsInChildren<BRigidBody>();

//                if (!isServer)
//                {
//                    networkMeshes = new NetworkMesh[rigidBodies.Length];

//                    for (int i = 0; i < rigidBodies.Length; i++)
//                        networkMeshes[i] = rigidBodies[i].gameObject.AddComponent<NetworkMesh>();
//                }

//                if (isLocalPlayer)
//                    CmdSetRobotID(state.Network.ConnectionID);

//                UpdateMotors();
//            }
//        }

//        /// <summary>
//        /// Ensures that the robot is awake and enables sending the next packet to the server.
//        /// </summary>
//        protected override void UpdateTransform()
//        {
//            base.UpdateTransform();

//            BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

//            if (rigidBody == null)
//                return;

//            serverCanSendUpdate = true;
//        }

//        /// <summary>
//        /// Updates pwm information and sends robot information to the server.
//        /// </summary>
//        private void FixedUpdate()
//        {
//            base.UpdatePhysics();

//            BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

//            if (rigidBody == null)
//                return;

//            if (isLocalPlayer)
//            {
//                float[] pwm = DriveJoints.GetPwmValues(Packet == null ? emptyDIO : Packet.dio, ControlIndex, false);

//                if (isServer)
//                    RpcUpdateRobotInfo(pwm);
//                else
//                    CmdUpdateRobotInfo(pwm);

//                if (InputControl.GetButton(Controls.buttons[ControlIndex].resetRobot))
//                    CmdResetRobot();
//            }

//            if (isServer && serverCanSendUpdate)
//                RemoteUpdateTransforms();
//        }

//        /// <summary>
//        /// Called when this instance is destroyed.
//        /// </summary>
//        private void OnDestroy()
//        {
//            if (state?.ActiveRobot == this)
//                state.ActiveRobot = null;
//        }
//    }
//}
