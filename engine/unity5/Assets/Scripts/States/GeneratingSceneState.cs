using Synthesis.Network;
using Synthesis.States;
using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Synthesis.GUI;

namespace Synthesis.States
{
    public class GeneratingSceneState : SyncState
    {
        private bool sceneGenerated;
        private bool renderingStarted;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Awake()
        {
            sceneGenerated = false;
            renderingStarted = false;
        }

        /// <summary>
        /// Starts scene generation on the host.
        /// </summary>
        public override void Start()
        {
            if (Host)
            {
                GenerateScene();
                PlayerIdentity.LocalInstance.sceneGenerated = true;
                PlayerIdentity.LocalInstance.ready = true;
                MatchManager.Instance.serverSceneGenerated = true;
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public override void Update()
        {
            if (Host)
            {
                if (!renderingStarted && UnityEngine.Object.FindObjectsOfType<PlayerIdentity>().All(p => p.ready))
                    StartSceneRendering();
            }
            else if (!sceneGenerated && MatchManager.Instance.serverSceneGenerated)
            {
                GenerateScene();
            }
        }

        /// <summary>
        /// Generates the scene on this instance.
        /// </summary>
        private void GenerateScene()
        {
            sceneGenerated = true;

            StateMachine.SceneGlobal.FindState<MultiplayerState>().LoadField(MatchManager.Instance.FieldFolder, Host);

            foreach (PlayerIdentity p in UnityEngine.Object.FindObjectsOfType<PlayerIdentity>())
                p.GetComponent<NetworkRobot>().enabled = true;

            if (!Host)
            {
                PlayerIdentity.LocalInstance.CmdSetSceneGenerated(true);
                PlayerIdentity.LocalInstance.CmdSetReady(true);
                StartSceneRendering();
            }
        }

        /// <summary>
        /// Starts rendering the scene on this instance.
        /// </summary>
        private void StartSceneRendering()
        {
            renderingStarted = true;

            NetworkMultiplayerUI.Instance.Visible = false;

            if (Host)
                MatchManager.Instance.syncing = false;

            StateMachine.PopState();
        }
    }
}
