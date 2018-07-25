using Synthesis.FSM;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.States
{
    public class DistributingResourcesState : SyncState
    {
        public override void Start()
        {
            if (Host)
                MatchManager.Instance.DistributeResources();
        }
    }
}
