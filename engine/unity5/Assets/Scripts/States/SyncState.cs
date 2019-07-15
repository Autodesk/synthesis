//using Synthesis.FSM;
//using Synthesis.GUI;
//using Synthesis.Network;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Synthesis.States
//{
//    public abstract class SyncState : State
//    {
//        /// <summary>
//        /// If true, this state is running on the host machine.
//        /// </summary>
//        protected bool Host { get; private set; }

//        /// <summary>
//        /// Initializes a new <see cref="SyncState"/> instance.
//        /// </summary>
//        public SyncState()
//        {
//            Host = PlayerIdentity.LocalInstance.isServer;
//        }

//        /// <summary>
//        /// Returns to the lobby if the back button is pressed on the host instance.
//        /// </summary>
//        public void OnBackButtonClicked()
//        {
//            if (Host)
//                MatchManager.Instance.CancelSync();
//            else
//                UserMessageManager.Dispatch("Only the host can cancel synchronization!", 8f);
//        }

//        /// <summary>
//        /// Indicates to the server that this state is ready for any awaiting
//        /// state changes.
//        /// </summary>
//        public void SendReadySignal()
//        {
//            PlayerIdentity.LocalInstance.CmdSetReady(true);
//        }
//    }
//}
