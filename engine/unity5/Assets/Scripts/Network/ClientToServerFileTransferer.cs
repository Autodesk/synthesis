using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    public class ClientToServerFileTransferer : FileTransferer
    {
        /// <summary>
        /// Invokes the command to prepare to receive bytes on the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        protected override void OnPrepareToReceiveBytes(string transferId, int expectedSize)
        {
            CmdPrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Invokes the command to receive bytes on the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        protected override void OnReceiveBytes(string transferId, byte[] buffer)
        {
            CmdReceiveBytes(transferId, buffer);
        }

        /// <summary>
        /// Initializes the server in preparation for receiving data from a client.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        [Command(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void CmdPrepareToReceiveBytes(string transferId, int expectedSize)
        {
            PrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Reads bytes from the given buffer on the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        [Command(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void CmdReceiveBytes(string transferId, byte[] buffer)
        {
            ReceiveBytes(transferId, buffer);
        }
    }
}
