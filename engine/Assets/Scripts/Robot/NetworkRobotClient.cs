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
//        /// <summary>
//        /// Updates pwm information on the client.
//        /// </summary>
//        /// <param name="pwm"></param>
//        [ClientRpc]
//        private void RpcUpdateRobotInfo(float[] pwm)
//        {
//            RemoteUpdateRobotInfo(pwm);
//        }

//        /// <summary>
//        /// Updates the robot position on the client.
//        /// </summary>
//        /// <param name="transforms"></param>
//        [ClientRpc]
//        void RpcUpdateTransforms(float[] transforms)
//        {
//            if (isServer || rigidBodies == null)
//                return;

//            BulletSharp.Math.Matrix[] bmTransforms = new BulletSharp.Math.Matrix[rigidBodies.Length];

//            // TODO: Combine these loops.

//            for (int i = 0; i < bmTransforms.Length; i++)
//            {
//                float[] rawTransform = new float[7];

//                for (int j = 0; j < rawTransform.Length; j++)
//                    rawTransform[j] = transforms[i * 13 + j];

//                bmTransforms[i] = BulletExtensions.DeserializeTransform(rawTransform);

//                rigidBodies[i].GetCollisionObject().Activate();
//                BulletSharp.Math.Matrix rbTransform = rigidBodies[i].GetCollisionObject().WorldTransform;
//            }

//            for (int i = 0; i < bmTransforms.Length; i++)
//            {
//                float[] rawLinearVelocity = new float[3];

//                for (int j = 0; j < rawLinearVelocity.Length; j++)
//                    rawLinearVelocity[j] = transforms[i * 13 + 7 + j];

//                float[] rawAngularVelocity = new float[3];

//                for (int j = 0; j < rawAngularVelocity.Length; j++)
//                    rawAngularVelocity[j] = transforms[i * 13 + 7 + rawLinearVelocity.Length + j];

//                BulletSharp.Math.Vector3 linearVelocity = new BulletSharp.Math.Vector3(rawLinearVelocity);
//                BulletSharp.Math.Vector3 angularVelocity = new BulletSharp.Math.Vector3(rawAngularVelocity);

//                networkMeshes[i].TargetLinearVelocity = linearVelocity.ToUnity();

//                RigidBody rbCo = (RigidBody)rigidBodies[i].GetCollisionObject();

//                rbCo.WorldTransform = bmTransforms[i];
//                rbCo.LinearVelocity = linearVelocity;
//                rbCo.AngularVelocity = angularVelocity;
//            }
//        }
//    }
//}
