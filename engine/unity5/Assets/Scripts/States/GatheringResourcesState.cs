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
    public class GatheringResourcesState : SyncState
    {
        /// <summary>
        /// Starts gathering resources from any necessary clients, and continues to the next state when
        /// the process is complete.
        /// </summary>
        public override void Start()
        {
            if (!Host)
                return;

            MatchManager.Instance.AwaitChangeState<DistributingResourcesState>(false);
            MatchManager.Instance.GatherResources();
        }
    }
}
