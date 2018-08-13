using Assets.Scripts;
using BulletSharp;
using BulletUnity;
using Synthesis.BUExtensions;
using Synthesis.FSM;
using Synthesis.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 0, sendInterval = 0f)]
public class NetworkRobot : RobotBase, ICollisionCallback
{
    enum SyncState : byte
    {
        ClientPriority,
        ServerPriority
    }

    SyncState syncState;

    const float CorrectionPositionThreshold = 0.05f;
    const float CorrectionRotationThreshold = 15.0f;
    const float StateTransitionTimeout = 0.5f;

    [SyncVar]
    public int RobotID = -1;

    BRigidBody[] rigidBodies;
    NetworkMesh[] networkMeshes;
    bool correctionEnabled = true;
    bool canSendUpdate = true;
    float timeSinceLastContact;

    List<BRigidBody> activeCollisions;

    private MultiplayerState state;

    /// <summary>
    /// Initializes the NetworkRobot instance.
    /// </summary>
    private void Awake()
    {
        syncState = SyncState.ClientPriority;
        activeCollisions = new List<BRigidBody>();
        timeSinceLastContact = 0f;

        StateMachine.SceneGlobal.Link<MultiplayerState>(this);
    }

    /// <summary>
    /// Creates a reference to the active multiplayer state when this <see cref="NetworkRobot"/>
    /// is enabled.
    /// </summary>
    private void OnEnable()
    {
        state = StateMachine.SceneGlobal.CurrentState as MultiplayerState;
    }

    /// <summary>
    /// Loads the Robot and initializes it on the network.
    /// </summary>
    private void Start()
    {
        string directory = PlayerPrefs.GetString("simSelectedRobot");

        if (!string.IsNullOrEmpty(directory))
        {
            state.LoadRobot(this, directory, isLocalPlayer);
            rigidBodies = GetComponentsInChildren<BRigidBody>();
            networkMeshes = new NetworkMesh[rigidBodies.Length];

            for (int i = 0; i < rigidBodies.Length; i++)
            {
                networkMeshes[i] = rigidBodies[i].gameObject.AddComponent<NetworkMesh>();

                if (isServer || isLocalPlayer)
                    rigidBodies[i].gameObject.AddComponent<BMultiCallbacks>().AddCallback(this);
            }

            if (isLocalPlayer)
                CmdSetRobotID(state.Network.ConnectionID);

            UpdateMotors();
        }
    }

    /// <summary>
    /// Ensures that the robot is awake and enables sending the next packet to the server.
    /// </summary>
    protected override void UpdateTransform()
    {
        base.UpdateTransform();

        if (isServer)
        {
            if (activeCollisions.Count == 0)
                timeSinceLastContact += Time.deltaTime;

            if (timeSinceLastContact > StateTransitionTimeout)
                SetSyncState(SyncState.ClientPriority);
        }

        if (Input.GetKey(KeyCode.E))
            correctionEnabled = true;
        else if (Input.GetKey(KeyCode.D))
            correctionEnabled = false;

        BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

        if (rigidBody == null)
            return;

        canSendUpdate = true;
    }

    /// <summary>
    /// Updates pwm information and sends robot information to the server.
    /// </summary>
    private void FixedUpdate()
    {
        base.UpdatePhysics();

        BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

        if (rigidBody == null)
            return;

        if (isLocalPlayer)
        {
            float[] pwm = DriveJoints.GetPwmValues(Packet == null ? emptyDIO : Packet.dio, ControlIndex, false);

            UpdateMotors(pwm);

            if (correctionEnabled)
            {
                if (isServer)
                    RpcUpdateRobotInfo(pwm);
                else
                    CmdUpdateRobotInfo(pwm);
            }
        }

        if (((isServer && syncState == SyncState.ServerPriority) || (isLocalPlayer && syncState == SyncState.ClientPriority)) &&
            canSendUpdate && correctionEnabled)
        {
            float[] transforms = new float[rigidBodies.Length * 13];

            int i = 0;
            foreach (BRigidBody rb in rigidBodies)
            {
                float[] currentTransform = rb.GetCollisionObject().WorldTransform.Serialize();

                for (int j = 0; j < currentTransform.Length; j++)
                    transforms[i * 13 + j] = currentTransform[j];

                float[] currentLinearVelocity = rb.GetCollisionObject().InterpolationLinearVelocity.ToArray();

                for (int j = 0; j < currentLinearVelocity.Length; j++)
                    transforms[i * 13 + currentTransform.Length + j] = currentLinearVelocity[j];

                float[] currentAngularVelocity = rb.GetCollisionObject().InterpolationAngularVelocity.ToArray();

                for (int j = 0; j < currentAngularVelocity.Length; j++)
                    transforms[i * 13 + currentTransform.Length + currentLinearVelocity.Length + j] = currentAngularVelocity[j];

                i++;
            }

            if (isServer)
                RpcUpdateTransforms(transforms);
            else
                CmdUpdateTransforms(transforms);
        }

        canSendUpdate = false;
    }

    /// <summary>
    /// Updates pwm information on the server.
    /// </summary>
    /// <param name="pwm"></param>
    [Command]
    private void CmdUpdateRobotInfo(float[] pwm)
    {
        RemoteUpdateRobotInfo(pwm);
        RpcUpdateRobotInfo(pwm);
    }

    /// <summary>
    /// Updates pwm information on the client.
    /// </summary>
    /// <param name="pwm"></param>
    [ClientRpc]
    private void RpcUpdateRobotInfo(float[] pwm)
    {
        if (!isLocalPlayer)
            RemoteUpdateRobotInfo(pwm);
    }

    /// <summary>
    /// Sends the pwm information of this robot to the server and other clients.
    /// </summary>
    /// <param name="pwm"></param>
    private void RemoteUpdateRobotInfo(float[] pwm)
    {
        if (RootNode != null/* && ControlsEnabled*/)
            DriveJoints.UpdateAllMotors(RootNode, pwm);
    }

    /// <summary>
    /// Updates the robot position on the server.
    /// </summary>
    /// <param name="transforms"></param>
    [Command]
    void CmdUpdateTransforms(float[] transforms)
    {
        UpdateTransforms(transforms);
        RpcUpdateTransforms(transforms);
    }

    /// <summary>
    /// Updates the robot position on the client.
    /// </summary>
    /// <param name="transforms"></param>
    [ClientRpc]
    void RpcUpdateTransforms(float[] transforms)
    {
        if (!isServer && (!isLocalPlayer || syncState == SyncState.ServerPriority))
            UpdateTransforms(transforms);
    }

    /// <summary>
    /// Sets the SyncState of this robot on the server if called from the local player and
    /// on the local player if called from the server.
    /// </summary>
    /// <param name="state"></param>
    void SetSyncState(SyncState state)
    {
        syncState = state;

        if (isServer)
            RpcSetSyncState((byte)syncState);
        else
            CmdSetSyncState((byte)syncState);
    }

    /// <summary>
    /// Sets the SyncState of the robot on the server.
    /// </summary>
    /// <param name="state"></param>
    [Command]
    void CmdSetSyncState(byte state)
    {
        syncState = (SyncState)state;
    }
        
    /// <summary>
    /// Sets the SyncState of the local robot instance.
    /// </summary>
    /// <param name="state"></param>
    [ClientRpc]
    void RpcSetSyncState(byte state)
    {
        if (isLocalPlayer)
            syncState = (SyncState)state;
    }

    /// <summary>
    /// Changes the RobotID value as sent from the client.
    /// </summary>
    /// <param name="robotID"></param>
    [Command]
    void CmdSetRobotID(int robotID)
    {
        RobotID = robotID;
    }

    /// <summary>
    /// Updates the robot's transform from the given array of transform information.
    /// </summary>
    /// <param name="transforms"></param>
    void UpdateTransforms(float[] transforms)
    {
        if (!correctionEnabled)
            return;

        BulletSharp.Math.Matrix[] bmTransforms = new BulletSharp.Math.Matrix[rigidBodies.Length];

        bool correctionRequired = false;

        for (int i = 0; i < bmTransforms.Length; i++)
        {
            float[] rawTransform = new float[7];

            for (int j = 0; j < rawTransform.Length; j++)
                rawTransform[j] = transforms[i * 13 + j];

            bmTransforms[i] = BulletExtensions.DeserializeTransform(rawTransform);

            BulletSharp.Math.Matrix rbTransform = rigidBodies[i].GetCollisionObject().WorldTransform;

            if (!correctionRequired &&
                ((bmTransforms[i].Origin - rbTransform.Origin).Length > CorrectionPositionThreshold) ||
                Math.Abs(Quaternion.Angle(bmTransforms[i].Orientation.ToUnity(), rbTransform.Orientation.ToUnity())) > CorrectionRotationThreshold)
                correctionRequired = true;
        }

        if (!correctionRequired)
            return;
        
        for (int i = 0; i < bmTransforms.Length; i++)
        {
            float[] rawLinearVelocity = new float[3];

            for (int j = 0; j < rawLinearVelocity.Length; j++)
                rawLinearVelocity[j] = transforms[i * 13 + 7 + j];

            float[] rawAngularVelocity = new float[3];

            for (int j = 0; j < rawAngularVelocity.Length; j++)
                rawAngularVelocity[j] = transforms[i * 13 + 7 + rawLinearVelocity.Length + j];

            BulletSharp.Math.Vector3 linearVelocity = new BulletSharp.Math.Vector3(rawLinearVelocity);
            BulletSharp.Math.Vector3 angularVelocity = new BulletSharp.Math.Vector3(rawAngularVelocity);

            networkMeshes[i].UpdateMeshTransform(bmTransforms[i].Origin.ToUnity(), bmTransforms[i].Orientation.ToUnity());

            RigidBody rbCo = (RigidBody)rigidBodies[i].GetCollisionObject();

            rbCo.WorldTransform = bmTransforms[i];  
            rbCo.LinearVelocity = linearVelocity;
            rbCo.AngularVelocity = angularVelocity;
        }
    }

    /// <summary>
    /// Called when a collision initially occurs between a node from this robot and a node from another robot.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="manifoldList"></param>
    public void BOnCollisionEnter(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
    {
        BRigidBody rb = other.UserObject as BRigidBody;

        if (rb == null || !rb.gameObject.name.StartsWith("node_"))
            return;

        SetSyncState(SyncState.ServerPriority);

        if (isServer)
        {
            activeCollisions.Add(rb);
            timeSinceLastContact = 0.0f;
        }
    }

    /// <summary>
    /// Called when an already established collision exits.
    /// </summary>
    /// <param name="other"></param>
    public void BOnCollisionExit(CollisionObject other)
    {
        if (!isServer)
            return;

        BRigidBody rb = other.UserObject as BRigidBody;

        if (rb != null)
            activeCollisions.Remove(rb);
    }

    public void BOnCollisionStay(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
    {
        // Not implemented
    }

    public void OnVisitPersistentManifold(PersistentManifold pm)
    {
        // Not implemented
    }

    public void OnFinishedVisitingManifolds()
    {
        // Not implemented
    }
}
