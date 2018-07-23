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
    public class FileTransferState : SyncState
    {
        public override void Start()
        {
            FileTransferer.Instance.OnSendingComplete += SendingComplete;
            FileTransferer.Instance.OnDataFragmentReceived += DataFragmentReceived;
            FileTransferer.Instance.OnReceivingComplete += ReceivingComplete;

            // FOR TESTING:
            if (Host)
            {
                byte[] bytes = File.ReadAllBytes(PlayerPrefs.GetString("simSelectedField") + "\\definition.bxdf");
                FileTransferer.Instance.SendFileToClient(PlayerIdentity.LocalInstance.connectionToClient, 0, bytes);
            }
        }

        public override void End()
        {
            FileTransferer.Instance.Reset();
            FileTransferer.Instance.OnSendingComplete -= SendingComplete;
            FileTransferer.Instance.OnDataFragmentReceived -= DataFragmentReceived;
            FileTransferer.Instance.OnReceivingComplete -= ReceivingComplete;
        }

        private void SendingComplete(int transferId, byte[] data)
        {
            Debug.Log("Complete!");
        }

        private void DataFragmentReceived(int transferId, byte[] data)
        {

        }

        private void ReceivingComplete(int transferId, byte[] data)
        {

        }
    }
}
