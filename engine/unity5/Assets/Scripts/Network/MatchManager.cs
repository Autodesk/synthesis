using Synthesis.FSM;
using Synthesis.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public string FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                if (!isServer)
                    return;

                fieldName = value;
                fieldGuid = string.Empty;
            }
        }

        /// <summary>
        /// Teh GUID of the selected field.
        /// </summary>
        public string FieldGuid => fieldGuid;

        /// <summary>
        /// True if synchronization is occurring.
        /// </summary>
        [SyncVar]
        public bool syncing;

        [SyncVar]
        private string fieldName;

        [SyncVar]
        private string fieldGuid;

        /// <summary>
        /// Contains each <see cref="PlayerIdentity"/> id and their corresponding <see cref="PlayerIdentity"/>
        /// ids with resources that need to be transferred.
        /// </summary>
        public Dictionary<int, List<int>> DependencyMap { get; private set; }

        /// <summary>
        /// Contains each <see cref="PlayerIdentity"/> id and their corresponding file transfer ids.
        /// </summary>
        public Dictionary<int, List<int>> TransferMap { get; private set; }

        private Dictionary<int, bool> resolvedDependencies;

        private Action generationComplete;

        private StateMachine uiStateMachine;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            DependencyMap = new Dictionary<int, List<int>>();
            TransferMap = new Dictionary<int, List<int>>();
            resolvedDependencies = new Dictionary<int, bool>();
            uiStateMachine = GameObject.Find("UserInterface").GetComponent<StateMachine>();
        }

        /// <summary>
        /// Loads the selected field file and reads its GUID.
        /// </summary>
        [Server]
        public void UpdateFieldGuid()
        {
            if (fieldGuid.Length > 0)
                return;

            string fieldFile = PlayerPrefs.GetString("simSelectedField") + "\\definition.bxdf";

            if (!File.Exists(fieldFile))
            {
                CancelSync();
                return;
            }

            Task<FieldDefinition> loadingTask = new Task<FieldDefinition>(() => BXDFProperties.ReadProperties(fieldFile));

            loadingTask.ContinueWith(t =>
            {
                if (t.Result == null)
                {
                    CancelSync();
                    return;
                }

                fieldGuid = t.Result.GUID.ToString();
            });

            loadingTask.Start();
        }

        /// <summary>
        /// Generates a dependency map from all <see cref="PlayerIdentity"/> instances.
        /// </summary>
        [Server]
        public void GenerateDependencyMap(Action onGenerationComplete)
        {
            generationComplete = onGenerationComplete;

            DependencyMap.Clear();
            resolvedDependencies.Clear();

            foreach (PlayerIdentity p in FindObjectsOfType<PlayerIdentity>())
                resolvedDependencies.Add(p.id, false);

            RpcCheckDependencies();
        }

        [Server]
        public void TransferFiles()
        {
            // TODO: Attach the file transferer script to the player identity GameObject.
            // That way, we don't have issues with client/server priority and the transfer ids
            // can overlap since they influence different objects anyway.
        }

        /// <summary>
        /// Checks if the resources held by the other identity need to be transferred to
        /// this instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="otherIdentity"></param>
        [ClientRpc]
        private void RpcCheckDependencies()
        {
            PlayerIdentity.LocalInstance.CheckDependencies();
        }

        /// <summary>
        /// Adds the given dependency to the dependency map.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="dependantId"></param>
        public void AddDependencies(int dependantId, int[] ownerIds)
        {
            resolvedDependencies[dependantId] = true;

            foreach (int ownerId in ownerIds)
            {
                if (!DependencyMap.ContainsKey(ownerId))
                    DependencyMap[ownerId] = new List<int>();

                DependencyMap[ownerId].Add(dependantId);
            }

            if (!resolvedDependencies.ContainsValue(false))
                generationComplete();
        }

        /// <summary>
        /// Pushes the given state for all clients when all
        /// <see cref="PlayerIdentity"/> instances are ready.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Server]
        public void AwaitPushState<T>() where T : new()
        {
            StopAllCoroutines();
            StartCoroutine(StartWhenReady(() => RpcPushState(typeof(T).ToString())));
        }

        /// <summary>
        /// Changes to the given state for all clients when all
        /// <see cref="PlayerIdentity"/> instances are ready.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hardReset"></param>
        [Server]
        public void AwaitChangeState<T>(bool hardReset = true) where T : new()
        {
            StopAllCoroutines();
            StartCoroutine(StartWhenReady(() => RpcChangeState(typeof(T).ToString(), hardReset)));
        }

        /// <summary>
        /// Pops the current state on all clients.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Server]
        public void PopState()
        {
            StopAllCoroutines();
            RpcPopState();
        }

        /// <summary>
        /// Waits for all <see cref="PlayerIdentity"/> instances to indicate
        /// readiness before executing the provided <see cref="Action"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Server]
        private IEnumerator StartWhenReady(Action whenReady)
        {
            yield return new WaitUntil(() => FindObjectsOfType<PlayerIdentity>().All(p => p.ready));
            whenReady();
        }

        /// <summary>
        /// Cancels the synchronization process.
        /// </summary>
        public void CancelSync()
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
            {
                PlayerIdentity.LocalInstance.CmdSetReady(false);
                uiStateMachine.PushState(state);
            }
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
            {
                PlayerIdentity.LocalInstance.CmdSetReady(false);
                uiStateMachine.ChangeState(state, hardReset);
            }
        }

        /// <summary>
        /// Pops the current state on all clients.
        /// </summary>
        [ClientRpc]
        private void RpcPopState()
        {
            PlayerIdentity.LocalInstance.CmdSetReady(false);
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
