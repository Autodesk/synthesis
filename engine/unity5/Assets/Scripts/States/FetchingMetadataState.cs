//using Synthesis.FSM;
//using Synthesis.Network;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Synthesis.States
//{
//    public class FetchingMetadataState : SyncState
//    {
//        private bool guidsFetched;

//        /// <summary>
//        /// Initailizes all fields for this instance.
//        /// </summary>
//        public override void Awake()
//        {
//            guidsFetched = false;
//        }

//        /// <summary>
//        /// Loads the selected robot skeleton and retrieves its GUID.
//        /// </summary>
//        public override void Start()
//        {
//            if (Host)
//            {
//                MatchManager.Instance.AwaitChangeState<AnalyzingResourcesState>(false);
//                MatchManager.Instance.UpdateFieldGuid();
//            }

//            PlayerIdentity.LocalInstance.UpdateRobotGuid();
//        }

//        /// <summary>
//        /// When all the GUIDs have been selected, the host moves on to the next stage in synchronization.
//        /// </summary>
//        public override void Update()
//        {
//            if (!guidsFetched &&
//                MatchManager.Instance.FieldGuid.Length > 0 &&
//                UnityEngine.Object.FindObjectsOfType<PlayerIdentity>().All(p => p.robotGuid.Length > 0))
//            {
//                guidsFetched = true;
//                PlayerIdentity.LocalInstance.CmdSetReady(true);
//            }
//        }
//    }
//}
