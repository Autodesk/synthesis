using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.States
{
    public abstract class SyncState : State
    {
        /// <summary>
        /// If true, this state is running on the host machine.
        /// </summary>
        protected bool Host { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="SyncState"/> instance.
        /// </summary>
        public SyncState()
        {
            Host = PlayerIdentity.LocalInstance.isServer;
        }

        /// <summary>
        /// Returns to the lobby if the back button is pressed on the host instance.
        /// </summary>
        public void OnBackButtonPressed()
        {
            if (Host)
                MatchManager.Instance.PopState();
            else
                UserMessageManager.Dispatch("Only the host can cancel synchronization!", 8f);
        }
    }
}
