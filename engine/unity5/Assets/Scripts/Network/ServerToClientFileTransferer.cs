using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    public class ServerToClientFileTransferer : FileTransferer
    {
        private Dictionary<string, NetworkConnection> targetConnections;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Awake()
        {
            targetConnections = new Dictionary<string, NetworkConnection>();
            ResetTransferData();
        }

        /// <summary>
        /// Sends a file to the specified target <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        /// <param name="target"></param>
        public void SendFile(string transferId, byte[] data, NetworkConnection target)
        {
            targetConnections[transferId] = target;
            SendFile(transferId, data);
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void ResetTransferData()
        {
            base.ResetTransferData();

            targetConnections.Clear();
        }

        /// <summary>
        /// Invokes the prepare receive bytes command on all clients.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        protected override void OnPrepareToReceiveBytes(string transferId, int expectedSize)
        {
            if (targetConnections.ContainsKey(transferId))
                TargetPrepareToReceiveBytes(targetConnections[transferId], transferId, expectedSize);
            else
                RpcPrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Invokes the receive bytes command on clients.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        protected override void OnReceiveBytes(string transferId, byte[] buffer)
        {
            if (targetConnections.ContainsKey(transferId))
                TargetReceiveBytes(targetConnections[transferId], transferId, buffer);
            else
                RpcReceiveBytes(transferId, buffer);
        }

        /// <summary>
        /// Initializes all clients in preparation for receiving data from the server.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        [ClientRpc(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void RpcPrepareToReceiveBytes(string transferId, int expectedSize)
        {
            PrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Initializes the given target in preparation for receiving data from the server.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        [TargetRpc(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void TargetPrepareToReceiveBytes(NetworkConnection target, string transferId, int expectedSize)
        {
            PrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Reads bytes from the given buffer on the target specified.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        [ClientRpc(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void RpcReceiveBytes(string transferId, byte[] buffer)
        {
            ReceiveBytes(transferId, buffer);
        }

        /// <summary>
        /// Reads bytes from the given buffer on the target specified.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        [TargetRpc(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void TargetReceiveBytes(NetworkConnection target, string transferId, byte[] buffer)
        {
            ReceiveBytes(transferId, buffer);
        }
    }
}
