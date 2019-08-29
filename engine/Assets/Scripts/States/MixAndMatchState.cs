using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.States
{
    public class MixAndMatchState : State
    {
        /// <summary>
        /// Returns to the previous scene when the back button is pressed.
        /// </summary>
        public void OnBackButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MixAndMatchMenu,
                AnalyticsLedger.EventAction.BackedOut,
                "",
                AnalyticsLedger.getMilliseconds().ToString());

            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MixMatch,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MixAndMatchMenu);
            StateMachine.PopState();
        }

        /// <summary>
        /// Launches a new <see cref="LoadFieldState"/> when the next button is pressed.
        /// </summary>
        public void OnNextButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MixAndMatchMenu,
                AnalyticsLedger.EventAction.Next,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
            StateMachine.PushState(new LoadFieldState());
        }
    }
}
