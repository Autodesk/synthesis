using Assets.Scripts;
using BulletSharp;
using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.FSM;
using Synthesis.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// TODO: Make it so nothing happens when colliding with the server bot (Connection ID 0)
// Also the collisions are kind of inconsistent, so the client should take ownership of collision
// scanning when it has ownership of the game element's position.

namespace Synthesis.Network
{
    [NetworkSettings(channel = 1, sendInterval = 0f)]
    public class NetworkElement : NetworkBehaviour
    {
        const float CorrectionPositionThreshold = 0.05f;
        const float CorrectionRotationThreshold = 15.0f;

        bool correctionEnabled = true;

        [SyncVar]
        public string NodeID = string.Empty;

        RigidBody rigidBody;
        NetworkMesh networkMesh;
        MultiplayerState state;

        bool canSendUpdate;

        private void Awake()
        {
            canSendUpdate = true;
        }

        private void Start()
        {
            if (!isServer)
                networkMesh = gameObject.AddComponent<NetworkMesh>();

            state = StateMachine.SceneGlobal.FindState<MultiplayerState>();

            //if (isServer && !isLocalPlayer)
            //    gameObject.AddComponent<BMultiCallbacks>().AddCallback(this);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKey(KeyCode.E))
                correctionEnabled = true;
            else if (UnityEngine.Input.GetKey(KeyCode.D))
                correctionEnabled = false;

            //if (isServer && lostContact)
            //{
            //    timeSinceLastContact += Time.deltaTime;

            //    if (timeSinceLastContact > OwnershipTimeout)
            //    {
            //        ParentRobotID = -1;
            //        lostContact = false;
            //    }
            //}

            canSendUpdate = true;

            if (rigidBody == null)
            {
                BRigidBody bRigidBody = gameObject.GetComponent<BRigidBody>();

                if (bRigidBody != null)
                    rigidBody = (RigidBody)bRigidBody.GetCollisionObject();
            }
        }

        private void FixedUpdate()
        {
            if (rigidBody == null)
                return;

            if (isServer && correctionEnabled && canSendUpdate)
            {
                float[] currentTransform = new float[13];

                Array.Copy(rigidBody.WorldTransform.Serialize(), currentTransform, 7);
                Array.Copy(rigidBody.LinearVelocity.ToArray(), 0, currentTransform, 7, 3);
                Array.Copy(rigidBody.AngularVelocity.ToArray(), 0, currentTransform, 10, 3);

                RpcUpdateTransform(currentTransform);
                //if (isServer)
                //    RpcUpdateTransform(currentTransform);
                //else
                //    CmdUpdateTransform(currentTransform);
            }

            canSendUpdate = false;
        }

        [Command]
        void CmdUpdateTransform(float[] transform)
        {
            UpdateTransform(transform);
            RpcUpdateTransform(transform);
        }

        [ClientRpc]
        void RpcUpdateTransform(float[] transform)
        {
            if (!isServer)
                UpdateTransform(transform);
        }

        private void UpdateTransform(float[] transform)
        {
            if (rigidBody == null)
                return;

            BulletSharp.Math.Matrix bmTransform = BulletExtensions.DeserializeTransform(transform.Take(7).ToArray());
            BulletSharp.Math.Matrix rbTransform = rigidBody.WorldTransform;

            //if ((bmTransform.Origin - rbTransform.Origin).Length > CorrectionPositionThreshold ||
            //    Math.Abs(Quaternion.Angle(bmTransform.Orientation.ToUnity(), rbTransform.Orientation.ToUnity())) > CorrectionRotationThreshold)
            //{
                BulletSharp.Math.Vector3 linearVelocity = new BulletSharp.Math.Vector3(transform.Skip(7).Take(3).ToArray());
                BulletSharp.Math.Vector3 angularVelocity = new BulletSharp.Math.Vector3(transform.Skip(10).Take(3).ToArray());

                networkMesh.TargetLinearVelocity = linearVelocity.ToUnity();
                //networkMesh.UpdateMeshTransform(bmTransform.Origin.ToUnity(), bmTransform.Orientation.ToUnity());

                rigidBody.WorldTransform = bmTransform;
                rigidBody.LinearVelocity = linearVelocity;
                rigidBody.AngularVelocity = angularVelocity;
            //}
        }

        //public void BOnCollisionEnter(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        //{
        //    Transform root = ((BRigidBody)other.UserObject).transform.root;

        //    if (root.GetComponent<NetworkIdentity>() == null)
        //        return;

        //    if (!activeCollisions.Contains(root.gameObject))
        //    {
        //        NetworkRobot robot = root.GetComponent<NetworkRobot>();

        //        if (robot != null && activeCollisions.Count == 0)
        //            ParentRobotID = robot.RobotID;
        //        else
        //            ParentRobotID = -1;

        //        activeCollisions.Add(root.gameObject);
        //    }
        //}

        //public void BOnCollisionExit(CollisionObject other)
        //{
        //    Transform root = ((BRigidBody)other.UserObject).transform.root;

        //    if (activeCollisions.Remove(root.gameObject) && activeCollisions.Count == 1)
        //    {
        //        NetworkRobot robot = activeCollisions[0].GetComponent<NetworkRobot>();

        //        if (robot != null)
        //        {
        //            ParentRobotID = robot.RobotID;
        //            return;
        //        }
        //    }

        //    lostContact = true;
        //}

        //public void BOnCollisionStay(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        //{
        //    // Not implemented
        //}

        //public void OnFinishedVisitingManifolds()
        //{
        //    // Not implemented
        //}

        //public void OnVisitPersistentManifold(PersistentManifold pm)
        //{
        //    // Not implemented
        //}
    }
}
