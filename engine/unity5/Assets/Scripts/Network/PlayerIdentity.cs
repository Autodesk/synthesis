//using Synthesis.FSM;
//using Synthesis.States;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using Synthesis.Utils;
//namespace Synthesis.Network
//{
//    [NetworkSettings(channel = 0, sendInterval = 0f)]
//    public class PlayerIdentity : NetworkBehaviour
//    {
//        public const int FileTransferBasePort = 1234;

//        #region ClientFields

//        /// <summary>
//        /// The local <see cref="PlayerIdentity"/> instance.
//        /// </summary>
//        public static PlayerIdentity LocalInstance { get; private set; }

//        /// <summary>
//        /// The player tag associated with new local <see cref="PlayerIdentity"/> instances.
//        /// </summary>
//        public static string DefaultLocalPlayerTag { get; set; }

//        /// <summary>
//        /// The local directory of the robot to be loaded.
//        /// </summary>
//        public string RobotFolder { get; set; }

//        #endregion

//        #region SyncVars

//        /// <summary>
//        /// The ID associatd with this <see cref="PlayerIdentity"/> instance.
//        /// </summary>
//        [SyncVar]
//        public int id;

//        /// <summary>
//        /// The player tag associated with this <see cref="PlayerIdentity"/> instance.
//        /// </summary>
//        [SyncVar]
//        public string playerTag;

//        /// <summary>
//        /// The robot name selected by this <see cref="PlayerIdentity"/>.
//        /// </summary>
//        [SyncVar]
//        public string robotName;

//        /// <summary>
//        /// The robot GUID of the selected robot.
//        /// </summary>
//        [SyncVar]
//        public string robotGuid;

//        /// <summary>
//        /// If true, this instance is ready to move to the next state.
//        /// </summary>
//        [SyncVar]
//        public bool ready;

//        /// <summary>
//        /// A percentage representing how much progress has been made gathering or distributing resources.
//        /// </summary>
//        [SyncVar]
//        public float transferProgress;

//        /// <summary>
//        /// The number of files to receive to this instance from the server.
//        /// </summary>
//        [SyncVar]
//        public int dependencyCount;

//        /// <summary>
//        /// True if the scene has been generated on this local instance.
//        /// </summary>
//        [SyncVar]
//        public bool sceneGenerated;

//        #endregion

//        #region ServerFields

//        /// <summary>
//        /// Represents the next available <see cref="PlayerIdentity"/> ID.
//        /// </summary>
//        private static int nextId = 0;

//        /// <summary>
//        /// The number of files the server is expecting to receive from the client,
//        /// or vice versa.
//        /// </summary>
//        public int LocalRobotFileCount { get; set; }

//        #endregion

//        /// <summary>
//        /// A hash set containing the names of files received on the server from the client,
//        /// or vice versa.
//        /// </summary>
//        public HashSet<string> ReceivedFiles { get; private set; }

//        /// <summary>
//        /// Returns the <see cref="PlayerIdentity"/> with the given ID.
//        /// </summary>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        public static PlayerIdentity FindById(int id)
//        {
//            IEnumerable<PlayerIdentity> results = FindObjectsOfType<PlayerIdentity>().Where(p => p.id == id);
//            return results.Any() ? results.First() : null;
//        }

//        /// <summary>
//        /// Initializes the properties of this <see cref="PlayerIdentity"/> and adds
//        /// itself to the player list.
//        /// </summary>
//        private void Start()
//        {
//            if (isServer)
//                id = nextId++;

//            if (isLocalPlayer)
//            {
//                LocalInstance = this;
//                CmdSetPlayerTag(DefaultLocalPlayerTag);
//                CmdSetRobotName(PlayerPrefs.GetString("simSelectedRobotName"));
//            }

//            RobotFolder = string.Empty;
//            ReceivedFiles = new HashSet<string>();
//            LocalRobotFileCount = 0;
            
//            PlayerList.Instance.AddPlayerEntry(this);
//        }

//        /// <summary>
//        /// Removes this instance from the player list.
//        /// </summary>
//        private void OnDestroy()
//        {
//            PlayerList.Instance.RemovePlayerEntry(this);
//        }

//        /// <summary>
//        /// Loads the selected robot file and reads its GUID.
//        /// </summary>
//        public void UpdateRobotGuid()
//        {
//            if (robotGuid.Length > 0)
//                return;

//            string robotFile = PlayerPrefs.GetString("simSelectedRobot") + Path.DirectorySeparatorChar + "skeleton";

//            if (!File.Exists(robotFile))
//            {
//                CmdCancelSync();
//                return;
//            }

//            Task<RigidNode_Base> loadingTask = new Task<RigidNode_Base>(() =>  BXDExtensions.ReadSkeletonSafe(robotFile));

//            loadingTask.ContinueWith(t =>
//            {
//                if (t.Result == null)
//                {
//                    CmdCancelSync();
//                    return;
//                }

//                CmdSetRobotGuid(t.Result.GUID.ToString());
//            }, TaskScheduler.FromCurrentSynchronizationContext());

//            loadingTask.Start();
//        }

//        /// <summary>
//        /// Cancels file synchronization.
//        /// </summary>
//        [Command]
//        private void CmdCancelSync()
//        {
//            MatchManager.Instance.CancelSync();
//        }

//        /// <summary>
//        /// Checks if the resources held by the other identity need to be transferred to
//        /// this instance.
//        /// </summary>
//        public void CheckDependencies()
//        {
//            ReceivedFiles.Clear();
//            dependencyCount = 0;

//            List<int> unresolvedDependencies = new List<int>();

//            RobotFolder = PlayerPrefs.GetString("simSelectedRobot");

//            foreach (PlayerIdentity otherIdentity in FindObjectsOfType<PlayerIdentity>().Where(p => p.id != id))
//            {
//                string robotsDirectory = PlayerPrefs.GetString("RobotDirectory");
//                otherIdentity.RobotFolder = string.Empty;

//                foreach (string dir in Directory.GetDirectories(robotsDirectory, otherIdentity.robotName))
//                {
//                    RigidNode_Base root = BXDJSkeleton.ReadSkeleton(dir + Path.DirectorySeparatorChar + "skeleton");

//                    if (root.GUID.ToString().Equals(otherIdentity.robotGuid))
//                    {
//                        otherIdentity.RobotFolder = dir;
//                        break;
//                    }
//                }

//                if (otherIdentity.RobotFolder.Length == 0)
//                {
//                    unresolvedDependencies.Add(otherIdentity.id);
//                    otherIdentity.RobotFolder = robotsDirectory + Path.DirectorySeparatorChar + otherIdentity.robotName;
//                }
//            }

//            string fieldsDirectory = PlayerPrefs.GetString("FieldDirectory");
//            MatchManager.Instance.FieldFolder = string.Empty;
            
//            foreach (string dir in Directory.GetDirectories(fieldsDirectory, MatchManager.Instance.FieldName))
//            {
//                FieldDefinition definition = BXDFProperties.ReadProperties(dir + Path.DirectorySeparatorChar + "definition.bxdf");

//                if (definition.GUID.ToString().Equals(MatchManager.Instance.FieldGuid))
//                {
//                    MatchManager.Instance.FieldFolder = dir;
//                    break;
//                }
//            }

//            if (MatchManager.Instance.FieldFolder.Length == 0)
//            {
//                unresolvedDependencies.Add(-1);
//                MatchManager.Instance.FieldFolder = fieldsDirectory + Path.DirectorySeparatorChar + MatchManager.Instance.FieldName;
//            }

//            CmdAddDependencies(unresolvedDependencies.ToArray());
//        }

//        /// <summary>
//        /// Adds the list of unresolved dependencies to the <see cref="MatchManager"/>'s dependency
//        /// map.
//        /// </summary>
//        /// <param name="unresolvedDependencies"></param>
//        [Command]
//        private void CmdAddDependencies(int[] unresolvedDependencies)
//        {
//            if (unresolvedDependencies.Length > 0)
//                MatchManager.Instance.AddDependencies(id, unresolvedDependencies);

//            ready = true;
//        }

//        /// <summary>
//        /// Transfers the resources owned by the client instance to the server (this instance)
//        /// </summary>
//        [Server]
//        public void TransferResources()
//        {
//            ReceivedFiles.Clear();
//            LocalRobotFileCount = -1;
//            transferProgress = 0f;

//            int port = FileTransferBasePort + id;

//            TcpFileTransfer.ReceiveFiles(port, PlayerPrefs.GetString("RobotDirectory") + Path.DirectorySeparatorChar + robotName,
//                OnServerFileReceived);

//            TargetTransferResources(connectionToClient, port);
//        }

//        /// <summary>
//        /// Transfers the resources owned by this instance to the server.
//        /// </summary>
//        [TargetRpc]
//        private void TargetTransferResources(NetworkConnection target, int port)
//        {
//            string[] files = Directory.GetFiles(PlayerPrefs.GetString("simSelectedRobot"));

//            CmdSetNumRobotFiles(files.Length);

//            TcpFileTransfer.SendFiles(MultiplayerNetwork.Instance.networkAddress, port, files);
//        }

//        /// <summary>
//        /// Updates the set of received files and the progress value.
//        /// </summary>
//        /// <param name="fileName"></param>
//        private void OnServerFileReceived(string fileName)
//        {
//            ReceivedFiles.Add(fileName);
            
//            transferProgress = LocalRobotFileCount > 0 ? ReceivedFiles.Count / (float)LocalRobotFileCount : 0f;

//            if (ReceivedFiles.Count == LocalRobotFileCount)
//                ready = true;
//        }

//        /// <summary>
//        /// Distributes resources associated with this instance to all dependent clients.
//        /// </summary>
//        /// <param name="destinationIds"></param>
//        [Server]
//        public void DistributeResources(HashSet<int> destinationIds)
//        {
//            List<string> networkAddresses = new List<string>();
//            int port = FileTransferBasePort + id;

//            foreach (PlayerIdentity identity in FindObjectsOfType<PlayerIdentity>()
//                .Where(p => p.id != LocalInstance.id && destinationIds.Contains(p.id)))
//            {
//                string ipv6 = identity.connectionToClient.address;
//                networkAddresses.Add(ipv6.Substring(ipv6.LastIndexOf(':') + 1));
//                identity.TargetReceiveFiles(identity.connectionToClient, port, "RobotDirectory", robotName);
//            }

//            if (networkAddresses.Any())
//                TcpFileTransfer.SendFiles(networkAddresses.ToArray(), port, Directory.GetFiles(RobotFolder));
//        }

//        /// <summary>
//        /// Receives files from the given port and saves them to a folder of the provided name
//        /// in the robot directory.
//        /// </summary>
//        /// <param name="target"></param>
//        /// <param name="port"></param>
//        /// <param name="folderName"></param>
//        [TargetRpc]
//        public void TargetReceiveFiles(NetworkConnection target, int port, string playerPrefsDirectory, string folderName)
//        {
//            TcpFileTransfer.ReceiveFiles(port, PlayerPrefs.GetString(playerPrefsDirectory) + Path.DirectorySeparatorChar + folderName,
//                OnClientFileReceived);
//        }

//        /// <summary>
//        /// Updates the set of received files and the progress value.
//        /// </summary>
//        /// <param name="fileName"></param>
//        private void OnClientFileReceived(string fileName)
//        {
//            ReceivedFiles.Add(fileName);

//            CmdSetTransferProgress(dependencyCount > 0 ? ReceivedFiles.Count / (float)dependencyCount : 0f);

//            if (ReceivedFiles.Count == dependencyCount)
//                CmdSetReady(true);
//        }

//        /// <summary>
//        /// Sets the player tag of this instance accross all clients.
//        /// </summary>
//        /// <param name="tag"></param>
//        [Command]
//        public void CmdSetPlayerTag(string tag)
//        {
//            playerTag = tag;
//        }

//        /// <summary>
//        /// Sets the robot name of this instance accross all clients.
//        /// </summary>
//        /// <param name="name"></param>
//        [Command]
//        public void CmdSetRobotName(string name)
//        {
//            robotName = name;
//            CmdSetRobotGuid(string.Empty);
//        }

//        /// <summary>
//        /// Sets the robot guid of this instance accross all clients.
//        /// </summary>
//        /// <param name="guid"></param>
//        [Command]
//        public void CmdSetRobotGuid(string guid)
//        {
//            robotGuid = guid;
//        }

//        /// <summary>
//        /// Sets the ready status of this instance accross all clients.
//        /// </summary>
//        /// <param name="playerReady"></param>
//        [Command]
//        public void CmdSetReady(bool playerReady)
//        {
//            ready = playerReady;
//        }

//        /// <summary>
//        /// Sets the progress value of this instance accross all clients.
//        /// </summary>
//        /// <param name="progress"></param>
//        [Command]
//        private void CmdSetTransferProgress(float progress)
//        {
//            transferProgress = progress;
//        }

//        /// <summary>
//        /// Sets the value of the scene generated <see cref="SyncVarAttribute"/>.
//        /// </summary>
//        /// <param name="generated"></param>
//        [Command]
//        public void CmdSetSceneGenerated(bool generated)
//        {
//            sceneGenerated = generated;
//        }

//        /// <summary>
//        /// Tells the server how many files to expect during the transfer.
//        /// </summary>
//        /// <param name="numFiles"></param>
//        [Command]
//        private void CmdSetNumRobotFiles(int numFiles)
//        {
//            LocalRobotFileCount = numFiles;
//        }
//    }
//}
