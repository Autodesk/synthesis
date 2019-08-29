//using Assets.Scripts;
//using BulletSharp;
//using BulletUnity;
//using Synthesis.BUExtensions;
//using Synthesis.FSM;
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
//    public class NetworkElement : NetworkBehaviour
//    {
//        bool correctionEnabled = true;

//        [SyncVar]
//        public string NodeID = string.Empty;

//        RigidBody rigidBody;
//        NetworkMesh networkMesh;
//        MultiplayerState state;

//        bool canSendUpdate;

//        /// <summary>
//        /// Initializes this instance.
//        /// </summary>
//        private void Awake()
//        {
//            canSendUpdate = true;
//        }

//        /// <summary>
//        /// Creates a reference to the active <see cref="MultiplayerState"/>.
//        /// </summary>
//        private void Start()
//        {
//            state = StateMachine.SceneGlobal.FindState<MultiplayerState>();
//        }

//        /// <summary>
//        /// When called, the <see cref="NetworkElement"/>'s ability to send transform
//        /// updates is re-enabled.
//        /// </summary>
//        private void Update()
//        {
//            if (networkMesh == null)
//                networkMesh = GetComponent<NetworkMesh>();

//            if (UnityEngine.Input.GetKey(KeyCode.E))
//                correctionEnabled = true;
//            else if (UnityEngine.Input.GetKey(KeyCode.D))
//                correctionEnabled = false;

//            canSendUpdate = true;

//            if (rigidBody == null)
//            {
//                BRigidBody bRigidBody = gameObject.GetComponent<BRigidBody>();

//                if (bRigidBody != null)
//                    rigidBody = (RigidBody)bRigidBody.GetCollisionObject();
//            }
//        }

//        /// <summary>
//        /// Sends transform updates from the server to the client.
//        /// </summary>
//        private void FixedUpdate()
//        {
//            if (rigidBody == null)
//                return;

//            if (isServer && correctionEnabled && canSendUpdate)
//            {
//                float[] currentTransform = new float[13];

//                Array.Copy(rigidBody.WorldTransform.Serialize(), currentTransform, 7);
//                Array.Copy(rigidBody.InterpolationLinearVelocity.ToArray(), 0, currentTransform, 7, 3);
//                Array.Copy(rigidBody.InterpolationAngularVelocity.ToArray(), 0, currentTransform, 10, 3);

//                RpcUpdateTransform(currentTransform);
//            }

//            canSendUpdate = false;
//        }

//        /// <summary>
//        /// Updates the transform of this instance on the client.
//        /// </summary>
//        /// <param name="transform"></param>
//        [ClientRpc]
//        void RpcUpdateTransform(float[] transform)
//        {
//            if (isServer || rigidBody == null)
//                return;

//            BulletSharp.Math.Matrix bmTransform = BulletExtensions.DeserializeTransform(transform.Take(7).ToArray());
//            BulletSharp.Math.Matrix rbTransform = rigidBody.WorldTransform;

//            BulletSharp.Math.Vector3 linearVelocity = new BulletSharp.Math.Vector3(transform.Skip(7).Take(3).ToArray());
//            BulletSharp.Math.Vector3 angularVelocity = new BulletSharp.Math.Vector3(transform.Skip(10).Take(3).ToArray());

//            if (networkMesh != null)
//                networkMesh.TargetLinearVelocity = linearVelocity.ToUnity();

//            rigidBody.WorldTransform = bmTransform;
//            rigidBody.LinearVelocity = linearVelocity;
//            rigidBody.AngularVelocity = angularVelocity;
//        }
//    }
//}
