using Synthesis.FSM;
using Synthesis.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    [NetworkSettings(channel = 0, sendInterval = 0f)]
    public class MatchManager : NetworkBehaviour
    {
        /// <summary>
        /// The global <see cref="MatchManager"/> instance.
        /// </summary>
        public static MatchManager Instance { get; private set; }

        /// <summary>
        /// The name of the selected field.
        /// </summary>
        [SyncVar]
        public string fieldName;

        /// <summary>
        /// True if synchronization is occurring.
        /// </summary>
        [SyncVar]
        public bool syncing;

        /// <summary>
        /// Contains each <see cref="PlayerIdentity"/> and their corresponding <see cref="PlayerIdentity"/>
        /// instances with resources that need to be transferred.
        /// </summary>
        public Dictionary<PlayerIdentity, List<PlayerIdentity>> DependencyMap { get; private set; }

        private StateMachine uiStateMachine;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            uiStateMachine = GameObject.Find("UserInterface").GetComponent<StateMachine>();
            DependencyMap = new Dictionary<PlayerIdentity, List<PlayerIdentity>>();
        }

        /// <summary>
        /// Generates a dependency map from all <see cref="PlayerIdentity"/> instances.
        /// </summary>
        [Server]
        public void GenerateDependencyMap()
        {
            DependencyMap.Clear();

            List<PlayerIdentity> playerIdentities = FindObjectsOfType<PlayerIdentity>().ToList();

            foreach (PlayerIdentity currentIdentity in playerIdentities)
                foreach (PlayerIdentity otherIdentity in playerIdentities.Where(p => !p.Equals(currentIdentity)))
                    currentIdentity.TargetCheckDependency(currentIdentity.connectionToClient, otherIdentity.netId);
        }

        /// <summary>
        /// Adds the given dependency to the dependency map.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="dependantId"></param>
        [Command]
        public void CmdAddDependency(NetworkInstanceId ownerId, NetworkInstanceId dependantId)
        {
            PlayerIdentity owner = NetworkServer.FindLocalObject(ownerId).GetComponent<PlayerIdentity>();
            PlayerIdentity dependant = NetworkServer.FindLocalObject(dependantId).GetComponent<PlayerIdentity>();

            if (!DependencyMap.ContainsKey(owner))
                DependencyMap[owner] = new List<PlayerIdentity>();

            DependencyMap[owner].Add(dependant);
        }

        /// <summary>
        /// Pushes the given state for all clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Server]
        public void PushState<T>() where T : new()
        {
            RpcPushState(typeof(T).ToString());
        }

        /// <summary>
        /// Changes to the given state for all clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hardReset"></param>
        [Server]
        public void ChangeState<T>(bool hardReset = true)
        {
            RpcChangeState(typeof(T).ToString(), hardReset);
        }

        /// <summary>
        /// Pops the current state on all clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Server]
        public void PopState()
        {
            RpcPopState();
        }

        /// <summary>
        /// Cancels the synchronization process.
        /// </summary>
        [Command]
        public void CmdCancelSync()
        {
            if (!syncing)
                return;

            syncing = false;
            PopState();
            RpcCancelSync();
        }

        /// <summary>
        /// Displays an error message on all clients.
        /// </summary>
        [ClientRpc]
        public void RpcCancelSync()
        {
            UserMessageManager.Dispatch("Synchronization failed!", 8f);
        }

        /// <summary>
        /// Pushes the given state for all clients.
        /// </summary>
        /// <param name="stateType"></param>
        [ClientRpc]
        private void RpcPushState(string stateType)
        {
            State state = StringToState(stateType);

            if (state != null)
                uiStateMachine.PushState(state);
        }

        /// <summary>
        /// Changes to the given state for all clients.
        /// </summary>
        /// <param name="stateType"></param>
        /// <param name="hardReset"></param>
        [ClientRpc]
        private void RpcChangeState(string stateType, bool hardReset)
        {
            State state = StringToState(stateType);

            if (state != null)
                uiStateMachine.ChangeState(state, hardReset);
        }

        /// <summary>
        /// Pops the current state on all clients.
        /// </summary>
        [ClientRpc]
        private void RpcPopState()
        {
            uiStateMachine.PopState();
        }

        /// <summary>
        /// Returns a <see cref="State"/> from the given string.
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        private State StringToState(string stateType)
        {
            Type type = Type.GetType(stateType);

            if (type == null)
            {
                Debug.LogError("Could not create state from type \"" + stateType + "\"!");
                return null;
            }

            return Activator.CreateInstance(type) as State;
        }
    }
}
