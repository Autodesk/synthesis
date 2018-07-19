using Synthesis.FSM;
using Synthesis.States;
using System;
using System.Collections.Generic;
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
        /// The player tag associated with new local <see cref="PlayerIdentity"/> instances.
        /// </summary>
        public static string DefaultLocalPlayerTag { get; set; }

        /// <summary>
        /// The player tag associated with this <see cref="PlayerIdentity"/>.
        /// </summary>
        public string PlayerTag
        {
            get
            {
                return playerTag;
            }
            set
            {
                CmdSetPlayerTag(value);
            }
        }

        [SyncVar]
        private string playerTag;

        /// <summary>
        /// Initializes the player tag and adds itself to the player list.
        /// </summary>
        private void Start()
        {
            if (isLocalPlayer)
                PlayerTag = DefaultLocalPlayerTag;

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
        /// Sets the player tag of this instance accross all clients.
        /// </summary>
        /// <param name="tag"></param>
        [Command]
        private void CmdSetPlayerTag(string tag)
        {
            playerTag = tag;
        }
    }
}
