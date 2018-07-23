using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    public class FileTransferer : NetworkBehaviour
    {
        // TODO: Implement client-to-server file transfer.
        // UPDATE: It should work to use the same transfer data and transfer IDs.
        // You would be able to distinguish on the receiving end if it was a send or receive by the transfer ID.
        // Just use some sort of mapping system on the server side.

        private const int BufferSize = 1024;

        /// <summary>
        /// Represents a fragment of data from a file.
        /// </summary>
        private struct DataFragment
        {
            public int dataIndex;
            public byte[] data;

            public DataFragment(byte[] data)
            {
                dataIndex = 0;
                this.data = data;
            }
        }

        /// <summary>
        /// The global <see cref="FileTransferer"/> instance.
        /// </summary>
        public static FileTransferer Instance { get; private set; }

        /// <summary>
        /// Called when the transfer is complete on the sender's end.
        /// </summary>
        public event Action<int, byte[]> OnSendingComplete;
        
        /// <summary>
        /// Called when a data fragment is sent on the senders' end.
        /// </summary>
        public event Action<int, byte[]> OnDataFragmentSent;

        /// <summary>
        /// Called when a data fragment is received on the receiver's end.
        /// </summary>
        public event Action<int, byte[]> OnDataFragmentReceived;
        
        /// <summary>
        /// Called when the transfer is complete on the receiver's end.
        /// </summary>
        public event Action<int, byte[]> OnReceivingComplete;

        private List<int> transferIds;
        private Dictionary<int, DataFragment> transferData;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            Reset();
        }

        /// <summary>
        /// Stops all coroutines and resets this instance.
        /// </summary>
        public void Reset()
        {
            StopAllCoroutines();

            transferIds = new List<int>();
            transferData = new Dictionary<int, DataFragment>();
        }

        /// <summary>
        /// Sends the given file with the given transfer ID to the client with the provided
        /// <see cref="NetworkConnection"/>.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        [Server]
        public void SendFileToClient(NetworkConnection target, int transferId, byte[] data)
        {
            Debug.Assert(!transferIds.Contains(transferId));
            StartCoroutine(SendBytesToClientRoutine(target, transferId, data));
        }

        /// <summary>
        /// Sends the given file with the given transfer ID to the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        [Client]
        public void SendFileToServer(int transferId, byte[] data)
        {
            Debug.Assert(!transferIds.Contains(transferId));
            StartCoroutine(SendBytesToServerRoutine(transferId, data));
        }

        /// <summary>
        /// The routine that sends the given data to the provided target with the specified transfer ID.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Server]
        private IEnumerator SendBytesToClientRoutine(NetworkConnection target, int transferId, byte[] data)
        {
            return SendBytes(transferId, data, (id, size) => TargetPrepareToReceiveBytes(target, id, size),
                (id, buffer) => TargetReceiveBytes(target, id, buffer));
        }

        /// <summary>
        /// The routine that sends the given data to the server from the current client instance.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Client]
        private IEnumerator SendBytesToServerRoutine(int transferId, byte[] data)
        {
            return SendBytes(transferId, data, (id, size) => CmdPrepareToReceiveBytes(id, size),
                (id, buffer) => CmdReceiveBytes(id, buffer));
        }

        /// <summary>
        /// Sends bytes the server or a client from the given transfer ID and data, and preparation
        /// and receiving actions.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        /// <param name="prepare"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        private IEnumerator SendBytes(int transferId, byte[] data, Action<int, int> prepare, Action<int, byte[]> receive)
        {
            prepare(transferId, data.Length);
            yield return null;

            transferIds.Add(transferId);
            DataFragment dataToTransfer = new DataFragment(data);
            int bufferSize = BufferSize;

            while (dataToTransfer.dataIndex < dataToTransfer.data.Length - 1)
            {
                int remaining = dataToTransfer.data.Length - dataToTransfer.dataIndex;

                if (remaining < bufferSize)
                    bufferSize = remaining;

                byte[] buffer = new byte[bufferSize];
                Array.Copy(dataToTransfer.data, dataToTransfer.dataIndex, buffer, 0, bufferSize);

                receive(transferId, buffer);
                dataToTransfer.dataIndex += bufferSize;

                yield return null;

                OnDataFragmentSent?.Invoke(transferId, buffer);
            }

            transferIds.Remove(transferId);

            OnSendingComplete?.Invoke(transferId, dataToTransfer.data);
        }

        /// <summary>
        /// Initializes the given target in preparation for receiving data from the server.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        [TargetRpc(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void TargetPrepareToReceiveBytes(NetworkConnection target, int transferId, int expectedSize)
        {
            PrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Initializes the server in preparation for receiving data from a client.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        [Command(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void CmdPrepareToReceiveBytes(int transferId, int expectedSize)
        {
            PrepareToReceiveBytes(transferId, expectedSize);
        }

        /// <summary>
        /// Prepares the client or server to receive data.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        private void PrepareToReceiveBytes(int transferId, int expectedSize)
        {
            if (transferData.ContainsKey(transferId))
                return;

            DataFragment receivingData = new DataFragment(new byte[expectedSize]);
            transferData.Add(transferId, receivingData);
        }

        /// <summary>
        /// Reads bytes from the given buffer on the target specified.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        [TargetRpc(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void TargetReceiveBytes(NetworkConnection target, int transferId, byte[] buffer)
        {
            ReceiveBytes(transferId, buffer);
        }

        /// <summary>
        /// Reads bytes from the given buffer on the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        [Command(channel = MultiplayerNetwork.ReliableSequencedChannel)]
        private void CmdReceiveBytes(int transferId, byte[] buffer)
        {
            ReceiveBytes(transferId, buffer);
        }

        /// <summary>
        /// Reads bytes from the given buffer.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        private void ReceiveBytes(int transferId, byte[] buffer)
        {
            if (!transferData.ContainsKey(transferId))
                return;

            DataFragment dataToReceive = transferData[transferId];
            Array.Copy(buffer, 0, dataToReceive.data, dataToReceive.dataIndex, buffer.Length);

            dataToReceive.dataIndex += buffer.Length;

            OnDataFragmentReceived?.Invoke(transferId, buffer);

            if (dataToReceive.dataIndex < dataToReceive.data.Length - 1)
                return;

            transferData.Remove(transferId);

            OnReceivingComplete?.Invoke(transferId, buffer);
        }
    }
}
