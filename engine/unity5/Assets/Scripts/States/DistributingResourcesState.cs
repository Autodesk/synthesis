using Synthesis.FSM;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class DistributingResourcesState : SyncState
    {
        private Text percentageText;

        /// <summary>
        /// Starts the resource distribution process.
        /// </summary>
        public override void Start()
        {
            percentageText = UnityEngine.GameObject.Find("DistributingPercentageText").GetComponent<Text>();

            if (Host)
                MatchManager.Instance.DistributeResources();
        }

        /// <summary>
        /// Updates the percentage text.
        /// </summary>
        public override void OnGUI()
        {
            percentageText.text = ((int)(MatchManager.Instance.distributionProgress * 100f)).ToString() + "%";
        }
    }
}
