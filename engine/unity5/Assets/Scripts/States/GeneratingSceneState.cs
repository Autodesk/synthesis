//using Synthesis.Network;
//using Synthesis.States;
//using Synthesis.FSM;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using Synthesis.GUI;
//using System.Collections;

//namespace Synthesis.States
//{
//    public class GeneratingSceneState : SyncState
//    {
//        /// <summary>
//        /// Starts scene generation.
//        /// </summary>
//        public override void Start()
//        {
//            MatchManager.Instance.StartCoroutine(Host ? ServerGenerateScene() : ClientGenerateScene());
//        }

//        /// <summary>
//        /// Generates the scene on the server.
//        /// </summary>
//        /// <returns></returns>
//        private IEnumerator ServerGenerateScene()
//        {
//            yield return new WaitForFixedUpdate();

//            GenerateScene();
//            MatchManager.Instance.serverSceneGenerated = true;

//            yield return new WaitUntil(() => UnityEngine.Object.FindObjectsOfType<PlayerIdentity>().All(p => p.ready));

//            StartSceneRendering();
//            MatchManager.Instance.syncing = false;
//        }

//        /// <summary>
//        /// Generates the scene on the client.
//        /// </summary>
//        /// <returns></returns>
//        private IEnumerator ClientGenerateScene()
//        {
//            yield return new WaitUntil(() => MatchManager.Instance.serverSceneGenerated);

//            GenerateScene();
//            StartSceneRendering();
//        }

//        /// <summary>
//        /// Generates the scene on this instance.
//        /// </summary>
//        private void GenerateScene()
//        {
//            StateMachine.SceneGlobal.FindState<MultiplayerState>().LoadField(MatchManager.Instance.FieldFolder, Host);

//            foreach (PlayerIdentity p in UnityEngine.Object.FindObjectsOfType<PlayerIdentity>())
//                p.GetComponent<NetworkRobot>().enabled = true;

//            PlayerIdentity.LocalInstance.CmdSetReady(true);
//        }

//        /// <summary>
//        /// Starts rendering the scene on this instance.
//        /// </summary>
//        private void StartSceneRendering()
//        {
//            NetworkMultiplayerUI.Instance.Visible = false;
//            StateMachine.PopState();
//        }
//    }
//}
