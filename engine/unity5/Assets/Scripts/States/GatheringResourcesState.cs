using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Synthesis.States
{
    public class GatheringResourcesState : SyncState
    {
        private Dictionary<int, List<int>> transferMap;

        public override void Awake()
        {
            transferMap = new Dictionary<int, List<int>>();
        }

        public override void Start()
        {
            if (Host)
            {
                FileTransferer.Instance.OnDataFragmentReceived += DataFragmentReceived;
                FileTransferer.Instance.OnReceivingComplete += ReceivingComplete;

                MatchManager.Instance.TransferFiles();

                //foreach (KeyValuePair<int, List<int>> entry in MatchManager.Instance.DependencyMap.Where(e => e.Key >= 0))
                //{
                //    PlayerIdentity identity = PlayerIdentity.FindById(entry.Key);
                //    //FileTransferer.Instance.GetComponent<NetworkIdentity>().AssignClientAuthority(identity.connectionToClient);
                //    identity.TargetTransferDependencies(identity.connectionToClient);
                //}
            }
            else
            {
                SendReadySignal();
            }

            // FOR TESTING:
            //if (Host)
            //{
            //    foreach (KeyValuePair<int, List<int>> entry in MatchManager.Instance.DependencyMap)
            //    {
            //        Debug.Log("Player " + entry.Key + " has assets needed by:");

            //        foreach (int id in entry.Value)
            //            Debug.Log("Player " + id);
            //    }
            //}

            //if (Host)
            //{
            //    byte[] bytes = File.ReadAllBytes(PlayerPrefs.GetString("simSelectedField") + "\\definition.bxdf");
            //    FileTransferer.Instance.SendFileToClient(PlayerIdentity.LocalInstance.connectionToClient, 0, bytes);
            //}
        }

        public override void End()
        {
            FileTransferer.Instance.Reset();
            FileTransferer.Instance.OnDataFragmentReceived -= DataFragmentReceived;
            FileTransferer.Instance.OnReceivingComplete -= ReceivingComplete;
        }

        private void SendingComplete(int transferId, byte[] data)
        {
            //Debug.Log("Complete!");
        }

        private void DataFragmentReceived(int transferId, byte[] data)
        {
            //Debug.Log("Fragment from " + transferId);
        }

        private void ReceivingComplete(int transferId, byte[] data)
        {
            Debug.Log(transferId + " complete!");
        }
    }
}
