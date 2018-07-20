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
        /// Finds and returns the host <see cref="PlayerIdentity"/>.
        /// </summary>
        public static PlayerIdentity HostInstance
        {
            get
            {
                IEnumerable<PlayerIdentity> players = FindObjectsOfType<PlayerIdentity>().Where(p => p.IsHost);
                return players.Any() ? players.First() : null;
            }
        }

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

        /// <summary>
        /// The robot name associatd with this <see cref="PlayerIdentity"/>.
        /// </summary>
        public string RobotName
        {
            get
            {
                return robotName;
            }
            set
            {
                CmdSetRobotName(value);
            }
        }

        /// <summary>
        /// The field selection associated with this <see cref="PlayerIdentity"/>.
        /// </summary>
        public string FieldName
        {
            get
            {
                return fieldName;
            }
            set
            {
                if (!isLocalPlayer || !isServer)
                {
                    Debug.LogError("Only the host instance may make a field selection!");
                    return;
                }

                fieldName = value;
            }
        }

        /// <summary>
        /// Determines if this <see cref="PlayerIdentity"/> is ready to start the match.
        /// </summary>
        public bool Ready
        {
            get
            {
                return ready;
            }
            set
            {
                CmdSetReady(value);
            }
        }

        /// <summary>
        /// Determines if this <see cref="PlayerIdentity"/> instance is the host.
        /// </summary>
        public bool IsHost
        {
            get
            {
                return isHost;
            }
            private set
            {
                CmdSetIsHost(value);
            }
        }

        [SyncVar]
        private string playerTag;

        [SyncVar]
        private string robotName;

        [SyncVar]
        private string fieldName;

        [SyncVar]
        private bool ready;

        [SyncVar]
        private bool isHost;

        /// <summary>
        /// Initializes the properties of this <see cref="PlayerIdentity"/> and adds
        /// itself to the player list.
        /// </summary>
        private void Start()
        {
            if (isLocalPlayer)
            {
                LocalInstance = this;
                PlayerTag = DefaultLocalPlayerTag;
                RobotName = PlayerPrefs.GetString("simSelectedRobotName");

                if (IsHost = isServer)
                    FieldName = PlayerPrefs.GetString("simSelectedFieldName");
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
        /// Sets the player tag of this instance accross all clients.
        /// </summary>
        /// <param name="tag"></param>
        [Command]
        private void CmdSetPlayerTag(string tag)
        {
            playerTag = tag;
        }

        /// <summary>
        /// Sets the robot name of this instance accross all clients.
        /// </summary>
        /// <param name="name"></param>
        [Command]
        private void CmdSetRobotName(string name)
        {
            robotName = name;
        }

        /// <summary>
        /// Sets the field name of this instance accross all clients.
        /// </summary>
        /// <param name="name"></param>
        [Command]
        private void CmdSetFieldName(string name)
        {
            fieldName = name;
        }

        /// <summary>
        /// Sets the ready status of this instance accross all clients.
        /// </summary>
        /// <param name="playerReady"></param>
        [Command]
        private void CmdSetReady(bool playerReady)
        {
            ready = playerReady;
        }

        /// <summary>
        /// Sets the ready status of this instance accross all clients.
        /// </summary>
        /// <param name="playerReady"></param>
        [Command]
        private void CmdSetIsHost(bool host)
        {
            isHost = host;
        }
    }
}
