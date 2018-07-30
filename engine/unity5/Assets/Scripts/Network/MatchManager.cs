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

// TODO: Make it so they can see the lobby while in simulation.

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
        /// A reference to the <see cref="NetworkMultiplayerUI"/> <see cref="StateMachine"/>.
        /// </summary>
        private StateMachine uiStateMachine;

        #region SyncVars

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
        /// The GUID of the selected field.
        /// </summary>
        public string FieldGuid => fieldGuid;

        /// <summary>
        /// True if synchronization is occurring.
        /// </summary>
        [SyncVar]
        public bool syncing;

        /// <summary>
        /// A percentage value representing distribution progress.
        /// </summary>
        [SyncVar]
        public float distributionProgress;

        /// <summary>
        /// The name of the selected field.
        /// </summary>
        [SyncVar]
        private string fieldName;

        /// <summary>
        /// The selected field's GUID.
        /// </summary>
        [SyncVar]
        private string fieldGuid;

        /// <summary>
        /// Returns true if the server has finished generating the scene.
        /// </summary>
        [SyncVar]
        public bool serverSceneGenerated;

        #endregion

        #region ServerFields

        /// <summary>
        /// Describes which resources are required to be transferred by which clients.
        /// </summary>
        private Dictionary<int, HashSet<int>> dependencyMap;

        /// <summary>
        /// Indicates which dependencies have been resolved without transferring.
        /// </summary>
        private Dictionary<int, bool> resolvedDependencies;

        /// <summary>
        /// Represents pending file transfer information for each active player.
        /// </summary>
        private Dictionary<int, Dictionary<string, List<string>>> pendingTransfers;

        /// <summary>
        /// The number of pending file transfers.
        /// </summary>
        private int numTotalTransfers;

        /// <summary>
        /// The number of completed file tansfers.
        /// </summary>
        private int numCompletedTransfers;

        /// <summary>
        /// Called when dependency map generation is complete.
        /// </summary>
        private Action dependencyGenerationComplete;

        /// <summary>
        /// The <see cref="ServerToClientFileTransferer"/> associated with this instance.
        /// </summary>
        public ServerToClientFileTransferer FileTransferer { get; private set; }

        #endregion

        #region ClientFields

        /// <summary>
        /// The local directory of the field to be loaded.
        /// </summary>
        public string FieldFolder { get; set; }

        /// <summary>
        /// The separator used for splitting the transfer ID into the header and file name.
        /// </summary>
        private readonly char[] headerSeparator = { '.' };

        /// <summary>
        /// File data transferred from the server to the client.
        /// </summary>
        private Dictionary<string, List<byte>> fileData;

        #endregion

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            dependencyMap = new Dictionary<int, HashSet<int>>();
            resolvedDependencies = new Dictionary<int, bool>();
            pendingTransfers = new Dictionary<int, Dictionary<string, List<string>>>();
            uiStateMachine = GameObject.Find("UserInterface").GetComponent<StateMachine>();
            fileData = new Dictionary<string, List<byte>>();

            FileTransferer = GetComponent<ServerToClientFileTransferer>();

            if (!isServer)
            {
                FileTransferer.OnDataFragmentReceived += DataFragmentReceived;
                FileTransferer.OnReceivingComplete += ReceivingComplete;
            }
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
            dependencyGenerationComplete = onGenerationComplete;

            dependencyMap.Clear();
            resolvedDependencies.Clear();

            foreach (PlayerIdentity p in FindObjectsOfType<PlayerIdentity>())
                resolvedDependencies.Add(p.id, false);

            RpcCheckDependencies();
        }

        /// <summary>
        /// Gathers resources from all <see cref="PlayerIdentity"/> instances.
        /// </summary>
        [Server]
        public void GatherResources()
        {
            HashSet<PlayerIdentity> remainingIdentities = new HashSet<PlayerIdentity>(FindObjectsOfType<PlayerIdentity>());

            foreach (KeyValuePair<int, HashSet<int>> entry in dependencyMap.Where(e => e.Key >= 0))
            {
                PlayerIdentity identity = PlayerIdentity.FindById(entry.Key);
                remainingIdentities.Remove(identity);
                identity.TransferResources();
            }

            foreach (PlayerIdentity identity in remainingIdentities)
            {
                identity.ready = true;
                identity.gatheringProgress = 1f;
            }
        }

        /// <summary>
        /// Distributes resources to the required <see cref="PlayerIdentity"/> instances.
        /// </summary>
        [Server]
        public void DistributeResources()
        {
            PlayerIdentity.LocalInstance.FileTransferer.StopAllCoroutines();
            distributionProgress = 0f;
            numTotalTransfers = 0;
            numCompletedTransfers = 0;

            pendingTransfers.Clear();

            foreach (KeyValuePair<int, HashSet<int>> dependency in dependencyMap.Where(kvp => kvp.Key >= 0))
            {
                PlayerIdentity currentIdentity = PlayerIdentity.FindById(dependency.Key);
                List<string> files = currentIdentity.ReceivedFiles.ToList();

                foreach (int dependant in dependency.Value)
                {
                    numTotalTransfers += files.Count;

                    if (!pendingTransfers.ContainsKey(dependant))
                        pendingTransfers[dependant] = new Dictionary<string, List<string>>();

                    pendingTransfers[dependant].Add(currentIdentity.robotName, files);
                }
            }

            foreach (PlayerIdentity p in FindObjectsOfType<PlayerIdentity>())
                if (!pendingTransfers.ContainsKey(p.id))
                    p.ready = true;

            Dictionary<string, List<byte>> fieldData;

            if (dependencyMap.ContainsKey(-1))
            {
                fieldData = LoadFieldData();
                List<string> files = fieldData.Keys.ToList();

                foreach (int dependant in dependencyMap[-1])
                {
                    numTotalTransfers += fieldData.Count;
                    pendingTransfers[dependant].Add(fieldName, files);
                }
            }
            else
            {
                fieldData = null;
            }

            foreach (KeyValuePair<int, HashSet<int>> entry in dependencyMap)
                foreach (KeyValuePair<string, List<byte>> file in entry.Key >= 0 ? PlayerIdentity.FindById(entry.Key).FileData : fieldData)
                    FileTransferer.SendFile(entry.Key.ToString() + "." + file.Key, file.Value.ToArray());
        }

        /// <summary>
        /// Loads the field and returns a dictionary of all file names and the data
        /// associated with each file.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<byte>> LoadFieldData()
        {
            Dictionary<string, List<byte>> data = new Dictionary<string, List<byte>>();

            foreach (string file in Directory.GetFiles(PlayerPrefs.GetString("simSelectedField")))
                data.Add(new FileInfo(file).Name, File.ReadAllBytes(file).ToList());

            return data;
        }

        /// <summary>
        /// Called when a fragment of data is received from the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        private void DataFragmentReceived(string transferId, byte[] data)
        {
            int senderId;
            string fileName;

            if (!GetTransferInfo(transferId, out senderId, out fileName))
                return;

            if (!fileData.ContainsKey(transferId))
                fileData[transferId] = new List<byte>();

            fileData[transferId].AddRange(data);
        }

        /// <summary>
        /// Called when a file has been received completely by the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        private void ReceivingComplete(string transferId, byte[] data)
        {
            int senderId;
            string fileName;

            if (!GetTransferInfo(transferId, out senderId, out fileName))
                return;

            string folderName = senderId >= 0 ? PlayerIdentity.FindById(senderId).robotName : fieldName;
            string directory = PlayerPrefs.GetString(senderId >= 0 ? "RobotDirectory" : "FieldDirectory") +
                "\\" + folderName;

            Task task = new Task(() =>
            {
                Directory.CreateDirectory(directory);

                File.WriteAllBytes(directory + "\\" + fileName,
                    fileData[transferId].ToArray());
            });

            task.ContinueWith(t =>
            {
                PlayerIdentity.LocalInstance.CmdConfirmFileTransferred(folderName, fileName);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            task.Start();
        }

        /// <summary>
        /// Tells the server that the given dependency has been resolved.
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="receiverId"></param>
        public void ConfirmFileTransferred(int playerId, string folderName, string fileName)
        {
            if (pendingTransfers[playerId][folderName].Remove(fileName))
            {
                numCompletedTransfers++;
                distributionProgress = (float)numCompletedTransfers / numTotalTransfers;

                if (pendingTransfers[playerId][folderName].Count == 0)
                    pendingTransfers[playerId].Remove(folderName);
            }

            if (pendingTransfers[playerId].Count == 0)
                PlayerIdentity.FindById(playerId).ready = true;
        }

        /// <summary>
        /// Returns the file name stored in the given transfer id, or <see cref="string.Empty"/>
        /// if the id does not match that of this client.
        /// </summary>
        /// <param name="transferId"></param>
        /// <returns></returns>
        private bool GetTransferInfo(string transferId, out int senderId, out string fileName)
        {
            string[] splitId = transferId.Split(headerSeparator, 2);

            senderId = int.Parse(splitId[0]);
            fileName = splitId[1];

            return PlayerIdentity.LocalInstance.UnresolvedDependencies.Contains(senderId);
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
                if (!dependencyMap.ContainsKey(ownerId))
                    dependencyMap[ownerId] = new HashSet<int>();

                dependencyMap[ownerId].Add(dependantId);
            }

            if (!resolvedDependencies.ContainsValue(false))
                dependencyGenerationComplete();
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
        private void PopState(string msg = "")
        {
            StopAllCoroutines();
            FileTransferer.ResetTransferData();
            RpcPopState(msg);
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
            PlayerIdentity[] identities = FindObjectsOfType<PlayerIdentity>();

            yield return new WaitUntil(() => identities.All(p => p.ready));

            foreach (PlayerIdentity p in identities)
                p.ready = false;

            whenReady();
        }

        /// <summary>
        /// Cancels the synchronization process.
        /// </summary>
        [Server]
        public void CancelSync(bool displayMessage = true)
        {
            if (!syncing)
                return;

            syncing = false;
            PopState(displayMessage ? "Synchronization cancelled!" : string.Empty);
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
        private void RpcPopState(string msg)
        {
            PlayerIdentity.LocalInstance.FileTransferer.ResetTransferData();
            PlayerIdentity.LocalInstance.CmdSetReady(false);

            FileTransferer.ResetTransferData();

            uiStateMachine.PopState();

            if (!msg.Equals(string.Empty))
                UserMessageManager.Dispatch(msg, 8f);
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
