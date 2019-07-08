//using Synthesis.Network;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Synthesis.States
//{
//    public class AnalyzingResourcesState : SyncState
//    {
//        /// <summary>
//        /// Generates a dependency map to identify how file data should flow from client to client.
//        /// </summary>
//        public override void Start()
//        {
//            if (Host)
//            {
//                MatchManager.Instance.AwaitChangeState<GatheringResourcesState>(false);
//                MatchManager.Instance.GenerateDependencyMap();
//            }
//        }
//    }
//}
