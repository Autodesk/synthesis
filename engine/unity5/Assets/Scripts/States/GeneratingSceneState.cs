using Synthesis.Network;
using Synthesis.States;
using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.States
{
    public class GeneratingSceneState : SyncState
    {
        private bool sceneGenerated;
        private bool renderingStarted;

        public override void Awake()
        {
            sceneGenerated = false;
            renderingStarted = false;
        }

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

        private void GenerateScene()
        {
            sceneGenerated = true;

            StateMachine.SceneGlobal.FindState<MultiplayerState>().LoadField(MatchManager.Instance.FieldFolder, Host);

            foreach (PlayerIdentity p in UnityEngine.Object.FindObjectsOfType<PlayerIdentity>())
                p.GetComponent<NetworkRobot>().enabled = true;

            if (!Host)
            {
                StartSceneRendering();
                PlayerIdentity.LocalInstance.CmdSetSceneGenerated(true);
                PlayerIdentity.LocalInstance.CmdSetReady(true);
            }
        }

        private void StartSceneRendering()
        {
            renderingStarted = true;

            StateMachine.GetComponent<Canvas>().enabled = false;
            UnityEngine.Object.FindObjectOfType<UnityEngine.Camera>().cullingMask |= (1 << LayerMask.NameToLayer("Default"));
        }
    }
}
