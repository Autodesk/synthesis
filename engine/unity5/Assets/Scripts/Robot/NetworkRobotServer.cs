//using Assets.Scripts;
//using BulletSharp;
//using BulletUnity;
//using Synthesis.BUExtensions;
//using Synthesis.FSM;
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
//    public partial class NetworkRobot : RobotBase
//    {
//        [SyncVar]
//        public int RobotID = -1;

//        private bool serverCanSendUpdate = true;

//        /// <summary>
//        /// Sends a transform update to all clients.
//        /// </summary>
//        private void RemoteUpdateTransforms()
//        {
//            float[] transforms = new float[rigidBodies.Length * 13];

//            int i = 0;
//            foreach (BRigidBody rb in rigidBodies)
//            {
//                float[] currentTransform = rb.GetCollisionObject().WorldTransform.Serialize();

//                for (int j = 0; j < currentTransform.Length; j++)
//                    transforms[i * 13 + j] = currentTransform[j];

//                float[] currentLinearVelocity = rb.GetCollisionObject().InterpolationLinearVelocity.ToArray();

//                for (int j = 0; j < currentLinearVelocity.Length; j++)
//                    transforms[i * 13 + currentTransform.Length + j] = currentLinearVelocity[j];

//                float[] currentAngularVelocity = rb.GetCollisionObject().InterpolationAngularVelocity.ToArray();

//                for (int j = 0; j < currentAngularVelocity.Length; j++)
//                    transforms[i * 13 + currentTransform.Length + currentLinearVelocity.Length + j] = currentAngularVelocity[j];

//                i++;
//            }

//            RpcUpdateTransforms(transforms);

//            serverCanSendUpdate = false;
//        }

//        /// <summary>
//        /// Updates pwm information on the server.
//        /// </summary>
//        /// <param name="pwm"></param>
//        [Command]
//        private void CmdUpdateRobotInfo(float[] pwm)
//        {
//            RemoteUpdateRobotInfo(pwm);
//            RpcUpdateRobotInfo(pwm);
//        }

//        /// <summary>
//        /// Sends the pwm information of this robot to the server and other clients.
//        /// </summary>
//        /// <param name="pwm"></param>
//        private void RemoteUpdateRobotInfo(float[] pwm)
//        {
//            if (RootNode != null)
//                DriveJoints.UpdateAllMotors(RootNode, pwm, emuList);
//        }

//        /// <summary>
//        /// Changes the RobotID value as sent from the client.
//        /// </summary>
//        /// <param name="robotID"></param>
//        [Command]
//        private void CmdSetRobotID(int robotID)
//        {
//            RobotID = robotID;
//        }

//        /// <summary>
//        /// Resets the robot to its start position.
//        /// </summary>
//        [Command]
//        public void CmdResetRobot()
//        {
//            BeginRobotReset();
//            EndRobotReset();
//        }
//    }
//}
