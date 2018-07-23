using Synthesis.FSM;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.States
{
    public class FetchingMetadataState : SyncState
    {
        /// <summary>
        /// Loads the selected robot skeleton and retrieves its GUID.
        /// </summary>
        public override void Start()
        {
            string robotFile = PlayerPrefs.GetString("simSelectedRobot") + "\\skeleton.bxdj";

            if (!File.Exists(robotFile))
            {
                MatchManager.Instance.CmdCancelSync();
                return;
            }

            RigidNode_Base root = BXDJSkeleton.ReadSkeleton(robotFile);

            if (root == null)
            {
                MatchManager.Instance.CmdCancelSync();
                return;
            }

            PlayerIdentity.LocalInstance.CmdSetRobotGuid(root.GUID.ToString());
        }

        /// <summary>
        /// When all the GUIDs have been selected, the host moves on to the next stage in synchronization.
        /// </summary>
        public override void Update()
        {
            if (!Host)
                return;

            if (UnityEngine.Object.FindObjectsOfType<PlayerIdentity>().All(p => p.robotGuid.Length > 0))
                MatchManager.Instance.ChangeState<AnalyzingResourcesState>(false);
        }
    }
}
