//using Synthesis.FSM;
//using Synthesis.GUI;
//using Synthesis.Network;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//namespace Synthesis.States
//{
//    public class GatheringResourcesState : SyncState
//    {
//        private Text percentageText;
//        private PlayerIdentity[] identities;

//        /// <summary>
//        /// Starts gathering resources from any necessary clients, and continues to the next state when
//        /// the process is complete.
//        /// </summary>
//        public override void Start()
//        {
//            percentageText = GameObject.Find("GatheringPercentageText").GetComponent<Text>();

//            if (Host)
//            {
//                MatchManager.Instance.AwaitChangeState<DistributingResourcesState>(false);
//                MatchManager.Instance.GatherResources();
//            }
//        }

//        /// <summary>
//        /// Updates the percentage label.
//        /// </summary>
//        public override void OnGUI()
//        {
//            identities = UnityEngine.Object.FindObjectsOfType<PlayerIdentity>();
//            percentageText.text = ((int)(identities.Average(p => p.transferProgress) * 100f)).ToString() + "%";
//        }
//    }
//}
