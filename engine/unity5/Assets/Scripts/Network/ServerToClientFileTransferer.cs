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
        /// <summary>
        /// Invokes the prepare receive bytes command on all clients.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        protected override void OnPrepareToReceiveBytes(string transferId, int expectedSize)
        {
            RpcPrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Invokes the receive bytes command on clients.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        protected override void OnReceiveBytes(string transferId, byte[] buffer)
        {
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
    }
}
