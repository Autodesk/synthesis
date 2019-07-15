//using Synthesis.FSM;
//using Synthesis.Network;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Synthesis.States
//{
//    public class DistributingResourcesState : SyncState
//    {
//        private Text percentageText;
//        //private PlayerIdentity[] identities;

//        /// <summary>
//        /// Starts the resource distribution process.
//        /// </summary>
//        public override void Start()
//        {
//            percentageText = GameObject.Find("DistributingPercentageText").GetComponent<Text>();

//            //if (Host)
//            //{
//            //    MatchManager.Instance.AwaitChangeState<GeneratingSceneState>(false);
//            //    MatchManager.Instance.DistributeResources();
//            //}
//        }

//        /// <summary>
//        /// Updates the percentage text.
//        /// </summary>
//        public override void OnGUI()
//        {
//            identities = UnityEngine.Object.FindObjectsOfType<PlayerIdentity>();
//            percentageText.text = ((int)(identities.Average(p => p.transferProgress) * 100f)).ToString() + "%";
//        }
//    }
//}
