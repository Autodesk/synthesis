using Synthesis.FSM;
using Synthesis.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    [NetworkSettings(channel = 0, sendInterval = 0f)]
    public class PlayerIdentity : NetworkBehaviour
    {
        /// <summary>
        /// The local <see cref="PlayerIdentity"/> instance.
        /// </summary>
        public static PlayerIdentity LocalInstance { get; private set; }

        /// <summary>
        /// The player tag associated with new local <see cref="PlayerIdentity"/> instances.
        /// </summary>
        public static string DefaultLocalPlayerTag { get; set; }

        /// <summary>
        /// The <see cref="ClientToServerFileTransferer"/> associated with this instance.
        /// </summary>
        public ClientToServerFileTransferer FileTransferer { get; private set; }

        [SyncVar]
        public int id;

        [SyncVar]
        public string playerTag;

        [SyncVar]
        public string robotName;

        [SyncVar]
        public string robotGuid;

        [SyncVar]
        public bool ready;

        private static int nextId = 0;

        private Dictionary<string, List<byte>> fileData;
        private HashSet<string> receivedFiles;
        private int numFilesToReceive;

        /// <summary>
        /// Returns the <see cref="PlayerIdentity"/> with the given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static PlayerIdentity FindById(int id)
        {
            IEnumerable<PlayerIdentity> results = FindObjectsOfType<PlayerIdentity>().Where(p => p.id == id);
            return results.Any() ? results.First() : null;
        }

        /// <summary>
        /// Initializes the properties of this <see cref="PlayerIdentity"/> and adds
        /// itself to the player list.
        /// </summary>
        private void Start()
        {
            FileTransferer = GetComponent<ClientToServerFileTransferer>();

            if (isServer)
            {
                id = nextId++;
                FileTransferer.OnDataFragmentReceived += DataFragmentReceived;
                FileTransferer.OnReceivingComplete += ReceivingComplete;
            }

            if (isLocalPlayer)
            {
                LocalInstance = this;
                CmdSetPlayerTag(DefaultLocalPlayerTag);
                CmdSetRobotName(PlayerPrefs.GetString("simSelectedRobotName"));
            }

            fileData = new Dictionary<string, List<byte>>();
            receivedFiles = new HashSet<string>();
            numFilesToReceive = -1;

            PlayerList.Instance.AddPlayerEntry(this);
        }

        /// <summary>
        /// Removes this instance from the player list.
        /// </summary>
        private void OnDestroy()
        {
            PlayerList.Instance.RemovePlayerEntry(this);
        }

        /// <summary>
        /// Loads the selected robot file and reads its GUID.
        /// </summary>
        public void UpdateRobotGuid()
        {
            if (robotGuid.Length > 0)
                return;

            string robotFile = PlayerPrefs.GetString("simSelectedRobot") + "\\skeleton.bxdj";

            if (!File.Exists(robotFile))
            {
                CmdCancelSync();
                return;
            }

            Task<RigidNode_Base> loadingTask = new Task<RigidNode_Base>(() => BXDJSkeleton.ReadSkeleton(robotFile));

            loadingTask.ContinueWith(t =>
            {
                if (t.Result == null)
                {
                    CmdCancelSync();
                    return;
                }

                CmdSetRobotGuid(t.Result.GUID.ToString());
            }, TaskScheduler.FromCurrentSynchronizationContext());

            loadingTask.Start();
        }

        /// <summary>
        /// Cancels file synchronization.
        /// </summary>
        [Command]
        private void CmdCancelSync()
        {
            MatchManager.Instance.CancelSync();
        }

        /// <summary>
        /// Checks if the resources held by the other identity need to be transferred to
        /// this instance.
        /// </summary>
        public void CheckDependencies()
        {
            List<int> unresolvedDependencies = new List<int>();

            foreach (PlayerIdentity otherIdentity in FindObjectsOfType<PlayerIdentity>().Where(p => p.id != id))
            {
                string robotDirectory = PlayerPrefs.GetString("RobotDirectory");
                bool robotDependencyResolved = false;

                foreach (string dir in Directory.GetDirectories(robotDirectory, otherIdentity.robotName))
                {
                    RigidNode_Base root = BXDJSkeleton.ReadSkeleton(dir + "\\skeleton.bxdj");

                    if (root.GUID.ToString().Equals(otherIdentity.robotGuid))
                    {
                        robotDependencyResolved = true;
                        break;
                    }
                }

                if (!robotDependencyResolved)
                    unresolvedDependencies.Add(otherIdentity.id);
            }

            string fieldDirectory = PlayerPrefs.GetString("FieldDirectory");
            bool fieldDependencyResolved = false;
            
            foreach (string dir in Directory.GetDirectories(fieldDirectory, MatchManager.Instance.FieldName))
            {
                FieldDefinition definition = BXDFProperties.ReadProperties(dir + "\\definition.bxdf");

                if (definition.GUID.ToString().Equals(MatchManager.Instance.FieldGuid))
                {
                    fieldDependencyResolved = true;
                    break;
                }
            }

            if (!fieldDependencyResolved)
                unresolvedDependencies.Add(-1);

            CmdAddDependencies(unresolvedDependencies.ToArray());
        }

        /// <summary>
        /// Adds the list of unresolved dependencies to the <see cref="MatchManager"/>'s dependency
        /// map.
        /// </summary>
        /// <param name="unresolvedDependencies"></param>
        [Command]
        private void CmdAddDependencies(int[] unresolvedDependencies)
        {
            MatchManager.Instance.AddDependencies(id, unresolvedDependencies);
        }

        /// <summary>
        /// Transfers the resources owned by the client instance to the server (this instance)
        /// </summary>
        [Server]
        public void TransferResources()
        {
            fileData.Clear();
            receivedFiles.Clear();
            numFilesToReceive = -1;

            TargetTransferResources(connectionToClient);
        }

        /// <summary>
        /// Transfers the resources owned by this instance to the server.
        /// </summary>
        [TargetRpc]
        private void TargetTransferResources(NetworkConnection target)
        {
            FileTransferer.Reset();

            string[] fileList = Directory.GetFiles(PlayerPrefs.GetString("simSelectedRobot"));

            CmdSetNumFilesToReceive(fileList.Length);

            var task = new Task<List<Tuple<string, byte[]>>>(() =>
            {
                List<Tuple<string, byte[]>> files = new List<Tuple<string, byte[]>>();

                foreach (string file in fileList)
                    files.Add(new Tuple<string, byte[]>(new FileInfo(file).Name, File.ReadAllBytes(file)));

                return files;
            });

            task.ContinueWith(t =>
            {
                foreach (Tuple<string, byte[]> file in t.Result)
                    FileTransferer.SendFile(file.Item1, file.Item2);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            task.Start();
        }

        /// <summary>
        /// Called when a data fragment is received.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        [Server]
        private void DataFragmentReceived(string transferId, byte[] data)
        {
            if (!fileData.ContainsKey(transferId))
                fileData[transferId] = new List<byte>();

            fileData[transferId].AddRange(data);
        }

        /// <summary>
        /// Called when receiving file data has completed.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        [Server]
        private void ReceivingComplete(string transferId, byte[] data)
        {
            receivedFiles.Add(transferId);

            //Debug.Log(transferId + " from " + robotName);

            if (receivedFiles.Count == numFilesToReceive)
                ready = true;
        }

        /// <summary>
        /// Sets the player tag of this instance accross all clients.
        /// </summary>
        /// <param name="tag"></param>
        [Command]
        public void CmdSetPlayerTag(string tag)
        {
            playerTag = tag;
        }

        /// <summary>
        /// Sets the robot name on this instance accross all clients.
        /// </summary>
        /// <param name="name"></param>
        public void SetRobotName(string name)
        {
            if (isServer)
            {
                robotName = name;
                robotGuid = string.Empty;
            }
            else
            {
                CmdSetRobotName(name);
            }
        }

        /// <summary>
        /// Sets the robot name of this instance accross all clients.
        /// </summary>
        /// <param name="name"></param>
        [Command]
        private void CmdSetRobotName(string name)
        {
            robotName = name;
            CmdSetRobotGuid(string.Empty);
        }

        /// <summary>
        /// Sets the robot guid of this instance accross all clients.
        /// </summary>
        /// <param name="guid"></param>
        [Command]
        public void CmdSetRobotGuid(string guid)
        {
            robotGuid = guid;
        }

        /// <summary>
        /// Sets the ready status of this instance accross all clients.
        /// </summary>
        /// <param name="playerReady"></param>
        [Command]
        public void CmdSetReady(bool playerReady)
        {
            ready = playerReady;
        }

        /// <summary>
        /// Tells the server how many files to expect during the transfer.
        /// </summary>
        /// <param name="numFiles"></param>
        [Command]
        private void CmdSetNumFilesToReceive(int numFiles)
        {
            numFilesToReceive = numFiles;
        }
    }
}
