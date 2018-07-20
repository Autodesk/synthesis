using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    [NetworkSettings(channel = 0, sendInterval = 0f)]
    public class LobbySettings : NetworkBehaviour
    {
        [SyncVar]
        public string fieldName;
    }
}
