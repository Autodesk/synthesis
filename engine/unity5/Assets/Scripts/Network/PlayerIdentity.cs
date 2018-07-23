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
        /// The local <see cref="PlayerIdentity"/> instance.
        /// </summary>
        public static PlayerIdentity LocalInstance { get; private set; }

        /// <summary>
        /// The player tag associated with new local <see cref="PlayerIdentity"/> instances.
        /// </summary>
        public static string DefaultLocalPlayerTag { get; set; }

        [SyncVar]
        public string playerTag;

        [SyncVar]
        public string robotName;

        [SyncVar]
        public string robotGuid;

        [SyncVar]
        public bool ready;

        /// <summary>
        /// Initializes the properties of this <see cref="PlayerIdentity"/> and adds
        /// itself to the player list.
        /// </summary>
        private void Start()
        {
            if (isLocalPlayer)
            {
                LocalInstance = this;
                CmdSetPlayerTag(DefaultLocalPlayerTag);
                CmdSetRobotName(PlayerPrefs.GetString("simSelectedRobotName"));
            }

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
        /// Checks if the resources held by the other identity need to be transferred to
        /// this instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="otherIdentity"></param>
        [TargetRpc]
        public void TargetCheckDependency(NetworkConnection target, NetworkInstanceId otherInstanceId)
        {
            // TODO: Add logic to check if the dependency needs to be added.
            MatchManager.Instance.CmdAddDependency(otherInstanceId, netId);
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
        /// Sets the robot name of this instance accross all clients.
        /// </summary>
        /// <param name="name"></param>
        [Command]
        public void CmdSetRobotName(string name)
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
    }
}
