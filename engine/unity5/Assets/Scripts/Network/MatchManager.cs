//using Synthesis.FSM;
//using Synthesis.GUI;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace Synthesis.Network
//{
//    [NetworkSettings(channel = 0, sendInterval = 0f)]
//    public class MatchManager : NetworkBehaviour
//    {
//        /// <summary>
//        /// The global <see cref="MatchManager"/> instance.
//        /// </summary>
//        public static MatchManager Instance { get; private set; }

//        /// <summary>
//        /// A reference to the <see cref="NetworkMultiplayerUI"/> <see cref="StateMachine"/>.
//        /// </summary>
//        private StateMachine uiStateMachine;

//        #region SyncVars

//        /// <summary>
//        /// The name of the selected field.
//        /// </summary>
//        public string FieldName
//        {
//            get
//            {
//                return fieldName;
//            }
//            set
//            {
//                if (!isServer)
//                    return;

//                fieldName = value;
//                fieldGuid = string.Empty;
//            }
//        }

//        /// <summary>
//        /// The GUID of the selected field.
//        /// </summary>
//        public string FieldGuid => fieldGuid;

//        /// <summary>
//        /// True if synchronization is occurring.
//        /// </summary>
//        [SyncVar]
//        public bool syncing;

//        /// <summary>
//        /// The name of the selected field.
//        /// </summary>
//        [SyncVar]
//        private string fieldName;

//        /// <summary>
//        /// The selected field's GUID.
//        /// </summary>
//        [SyncVar]
//        private string fieldGuid;

//        /// <summary>
//        /// Returns true if the server has finished generating the scene.
//        /// </summary>
//        [SyncVar]
//        public bool serverSceneGenerated;

//        #endregion

//        #region ServerFields

//        /// <summary>
//        /// Describes which resources are required to be transferred by which clients.
//        /// </summary>
//        private Dictionary<int, HashSet<int>> dependencyMap;

//        /// <summary>
//        /// A set of IDs of all dependant <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        private HashSet<int> dependants;

//        #endregion

//        #region ClientFields

//        /// <summary>
//        /// The local directory of the field to be loaded.
//        /// </summary>
//        public string FieldFolder { get; set; }

//        #endregion

//        /// <summary>
//        /// Initializes this instance.
//        /// </summary>
//        private void Awake()
//        {
//            Instance = this;
//            dependencyMap = new Dictionary<int, HashSet<int>>();
//            dependants = new HashSet<int>();
//            uiStateMachine = GameObject.Find("UserInterface").GetComponent<StateMachine>();
//        }

//        /// <summary>
//        /// Loads the selected field file and reads its GUID.
//        /// </summary>
//        [Server]
//        public void UpdateFieldGuid()
//        {
//            if (fieldGuid.Length > 0)
//                return;

//            string fieldFile = PlayerPrefs.GetString("simSelectedField") + Path.DirectorySeparatorChar + "definition.bxdf";

//            if (!File.Exists(fieldFile))
//            {
//                CancelSync();
//                return;
//            }

//            Task<FieldDefinition> loadingTask = new Task<FieldDefinition>(() => BXDFProperties.ReadProperties(fieldFile));

//            loadingTask.ContinueWith(t =>
//            {
//                if (t.Result == null)
//                {
//                    CancelSync();
//                    return;
//                }

//                fieldGuid = t.Result.GUID.ToString();
//            });

//            loadingTask.Start();
//        }

//        /// <summary>
//        /// Generates a dependency map from all <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        [Server]
//        public void GenerateDependencyMap()
//        {
//            dependencyMap.Clear();
//            dependants.Clear();

//            RpcCheckDependencies();
//        }

//        /// <summary>
//        /// Gathers resources from all <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        [Server]
//        public void GatherResources()
//        {
//            HashSet<PlayerIdentity> remainingIdentities = new HashSet<PlayerIdentity>(FindObjectsOfType<PlayerIdentity>());

//            foreach (KeyValuePair<int, HashSet<int>> entry in dependencyMap.Where(e => e.Key >= 0 && e.Key != PlayerIdentity.LocalInstance.id))
//            {
//                PlayerIdentity identity = PlayerIdentity.FindById(entry.Key);
//                remainingIdentities.Remove(identity);
//                identity.TransferResources();
//            }

//            foreach (PlayerIdentity identity in remainingIdentities)
//            {
//                identity.ready = true;
//                identity.transferProgress = 1f;
//                identity.LocalRobotFileCount = Directory.GetFiles(identity.RobotFolder).Length;
//            }
//        }

//        /// <summary>
//        /// Distributes resources to the required <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        [Server]
//        public void DistributeResources()
//        {
//            foreach (PlayerIdentity identity in FindObjectsOfType<PlayerIdentity>())
//            {
//                if (dependants.Contains(identity.id))
//                {
//                    identity.transferProgress = 0f;
//                }
//                else
//                {
//                    identity.transferProgress = 1f;
//                    identity.ready = true;
//                }
//            }

//            foreach (KeyValuePair<int, HashSet<int>> entry in dependencyMap.Where(e => e.Key >= 0))
//            {
//                PlayerIdentity hostIdentity = PlayerIdentity.FindById(entry.Key);

//                foreach (PlayerIdentity dependant in FindObjectsOfType<PlayerIdentity>()
//                    .Where(p => entry.Value.Contains(p.id)))
//                    dependant.dependencyCount += hostIdentity.LocalRobotFileCount;
//            }
            
//            if (dependencyMap.ContainsKey(-1))
//                DistributeField(dependencyMap[-1]);

//            foreach (KeyValuePair<int, HashSet<int>> entry in dependencyMap.Where(e => e.Key >= 0))
//                PlayerIdentity.FindById(entry.Key).DistributeResources(entry.Value);
//        }

//        /// <summary>
//        /// Distributes the selected field to all given destinations.
//        /// </summary>
//        /// <param name="destinationIds"></param>
//        private void DistributeField(HashSet<int> destinationIds)
//        {
//            List<string> networkAddresses = new List<string>();
//            int port = PlayerIdentity.FileTransferBasePort - 1;

//            string[] fieldFiles = Directory.GetFiles(PlayerPrefs.GetString("simSelectedField"));

//            foreach (PlayerIdentity identity in FindObjectsOfType<PlayerIdentity>()
//                .Where(p => destinationIds.Contains(p.id)))
//            {
//                identity.dependencyCount += fieldFiles.Length;

//                string ipv6 = identity.connectionToClient.address;
//                networkAddresses.Add(ipv6.Substring(ipv6.LastIndexOf(':') + 1));
//                identity.TargetReceiveFiles(identity.connectionToClient, port, "FieldDirectory", FieldName);
//            }

//            TcpFileTransfer.SendFiles(networkAddresses.ToArray(), port, fieldFiles);
//        }

//        /// <summary>
//        /// Checks if the resources held by the other identity need to be transferred to
//        /// this instance.
//        /// </summary>
//        /// <param name="target"></param>
//        /// <param name="otherIdentity"></param>
//        [ClientRpc]
//        private void RpcCheckDependencies()
//        {
//            PlayerIdentity.LocalInstance.CheckDependencies();
//        }

//        /// <summary>
//        /// Adds the given dependency to the dependency map.
//        /// </summary>
//        /// <param name="ownerId"></param>
//        /// <param name="dependantId"></param>
//        public void AddDependencies(int dependantId, int[] ownerIds)
//        {
//            foreach (int ownerId in ownerIds)
//            {
//                if (!dependencyMap.ContainsKey(ownerId))
//                    dependencyMap[ownerId] = new HashSet<int>();

//                dependencyMap[ownerId].Add(dependantId);
//            }

//            if (dependantId != PlayerIdentity.LocalInstance.id)
//                dependants.Add(dependantId);
//        }

//        /// <summary>
//        /// Pushes the given state for all clients when all
//        /// <see cref="PlayerIdentity"/> instances are ready.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        [Server]
//        public void AwaitPushState<T>() where T : new()
//        {
//            StopAllCoroutines();
//            StartCoroutine(StartWhenReady(() => RpcPushState(typeof(T).ToString())));
//        }

//        /// <summary>
//        /// Changes to the given state for all clients when all
//        /// <see cref="PlayerIdentity"/> instances are ready.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="hardReset"></param>
//        [Server]
//        public void AwaitChangeState<T>(bool hardReset = true) where T : new()
//        {
//            StopAllCoroutines();
//            StartCoroutine(StartWhenReady(() => RpcChangeState(typeof(T).ToString(), hardReset)));
//        }

//        /// <summary>
//        /// Pops the current state on all clients.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        [Server]
//        private void PopState(string msg = "")
//        {
//            StopAllCoroutines();
//            RpcPopState(msg);
//        }

//        /// <summary>
//        /// Waits for all <see cref="PlayerIdentity"/> instances to indicate
//        /// readiness before executing the provided <see cref="Action"/>.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <returns></returns>
//        [Server]
//        private IEnumerator StartWhenReady(Action whenReady)
//        {
//            PlayerIdentity[] identities = FindObjectsOfType<PlayerIdentity>();

//            yield return new WaitUntil(() => identities.All(p => p.ready));

//            foreach (PlayerIdentity p in identities)
//                p.ready = false;

//            whenReady();
//        }

//        /// <summary>
//        /// Cancels the synchronization process.
//        /// </summary>
//        [Server]
//        public void CancelSync(bool displayMessage = true)
//        {
//            if (!syncing)
//                return;

//            syncing = false;
//            PopState(displayMessage ? "Synchronization cancelled!" : string.Empty);
//        }

//        /// <summary>
//        /// Pushes the given state for all clients.
//        /// </summary>
//        /// <param name="stateType"></param>
//        [ClientRpc]
//        private void RpcPushState(string stateType)
//        {
//            State state = StringToState(stateType);

//            if (state != null)
//                uiStateMachine.PushState(state);
//        }

//        /// <summary>
//        /// Changes to the given state for all clients.
//        /// </summary>
//        /// <param name="stateType"></param>
//        /// <param name="hardReset"></param>
//        [ClientRpc]
//        private void RpcChangeState(string stateType, bool hardReset)
//        {
//            State state = StringToState(stateType);

//            if (state != null)
//                uiStateMachine.ChangeState(state, hardReset);
//        }

//        /// <summary>
//        /// Pops the current state on all clients.
//        /// </summary>
//        [ClientRpc]
//        private void RpcPopState(string msg)
//        {
//            PlayerIdentity.LocalInstance.CmdSetReady(false);

//            uiStateMachine.PopState();

//            if (!msg.Equals(string.Empty))
//                UserMessageManager.Dispatch(msg, 8f);
//        }

//        /// <summary>
//        /// Returns a <see cref="State"/> from the given string.
//        /// </summary>
//        /// <param name="stateType"></param>
//        /// <returns></returns>
//        private State StringToState(string stateType)
//        {
//            Type type = Type.GetType(stateType);

//            if (type == null)
//            {
//                Debug.LogError("Could not create state from type \"" + stateType + "\"!");
//                return null;
//            }

//            return Activator.CreateInstance(type) as State;
//        }
//    }
//}
