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
    [NetworkSettings(channel = 0, sendInterval = 0.5f)]
    public abstract class FileTransferer : NetworkBehaviour
    {
        private const int BufferSize = 2048;

        /// <summary>
        /// Represents a fragment of data from a file.
        /// </summary>
        private class DataFragment
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
        /// Called when the transfer is complete on the sender's end.
        /// </summary>
        public event Action<string, byte[]> OnSendingComplete;

        /// <summary>
        /// Called when a data fragment is sent on the senders' end.
        /// </summary>
        public event Action<string, byte[]> OnDataFragmentSent;

        /// <summary>
        /// Called when a data fragment is received on the receiver's end.
        /// </summary>
        public event Action<string, byte[]> OnDataFragmentReceived;

        /// <summary>
        /// Called when the transfer is complete on the receiver's end.
        /// </summary>
        public event Action<string, byte[]> OnReceivingComplete;

        private List<string> transferIds;
        private Dictionary<string, DataFragment> transferData;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Awake()
        {
            ResetTransferData();
        }

        /// <summary>
        /// Stops all coroutines and resets this instance.
        /// </summary>
        public void ResetTransferData()
        {
            StopAllCoroutines();

            transferIds = new List<string>();
            transferData = new Dictionary<string, DataFragment>();
        }

        /// <summary>
        /// Sends the given file with the given transfer ID to the server.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="data"></param>
        public void SendFile(string transferId, byte[] data)
        {
            Debug.Assert(!transferIds.Contains(transferId));
            StartCoroutine(SendBytes(transferId, data));
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
        protected IEnumerator SendBytes(string transferId, byte[] data)
        {
            OnPrepareToReceiveBytes(transferId, data.Length);

            transferIds.Add(transferId);

            yield return new WaitUntil(() => transferIds[0].Equals(transferId));

            DataFragment dataToTransfer = new DataFragment(data);
            int bufferSize = BufferSize;

            while (dataToTransfer.dataIndex < dataToTransfer.data.Length - 1)
            {
                int remaining = dataToTransfer.data.Length - dataToTransfer.dataIndex;

                if (remaining < bufferSize)
                    bufferSize = remaining;

                byte[] buffer = new byte[bufferSize];
                Array.Copy(dataToTransfer.data, dataToTransfer.dataIndex, buffer, 0, bufferSize);

                OnReceiveBytes(transferId, buffer);
                dataToTransfer.dataIndex += bufferSize;

                yield return null;

                OnDataFragmentSent?.Invoke(transferId, buffer);
            }

            transferIds.Remove(transferId);

            OnSendingComplete?.Invoke(transferId, dataToTransfer.data);
        }

        /// <summary>
        /// Prepares the client or server to receive data.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        protected void PrepareToReceiveBytes(string transferId, int expectedSize)
        {
            if (transferData.ContainsKey(transferId))
                return;

            DataFragment receivingData = new DataFragment(new byte[expectedSize]);
            transferData.Add(transferId, receivingData);
        }

        /// <summary>
        /// Reads bytes from the given buffer.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        protected void ReceiveBytes(string transferId, byte[] buffer)
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

        /// <summary>
        /// Called when a stream of bytes are about to be received.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="expectedSize"></param>
        protected abstract void OnPrepareToReceiveBytes(string transferId, int expectedSize);

        /// <summary>
        /// Called when bytes are about to be received.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="buffer"></param>
        protected abstract void OnReceiveBytes(string transferId, byte[] buffer);
    }
}
