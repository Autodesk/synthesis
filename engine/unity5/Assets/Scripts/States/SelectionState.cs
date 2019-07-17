using Synthesis.FSM;
using UnityEngine.SceneManagement;

namespace Synthesis.States
{
    public class SelectionState : State
    {
        /// <summary>
        /// Opens the default simulator tab when the main simulator button is pressed.
        /// </summary>
        public void OnMainSimulatorButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MainSimMenu,
                AnalyticsLedger.EventAction.Start,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.MainSimMenu,
                AnalyticsLedger.TimingVarible.Customizing);

            StateMachine.PushState(new DefaultSimulatorState());
        }

        /// <summary>
        /// Opens the mix and match tab when the mix and match button is pressed.
        /// </summary>
        public void OnMixAndMatchButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MixAndMatchMenu,
                AnalyticsLedger.EventAction.Start,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.MixAndMatchMenu,
                AnalyticsLedger.TimingVarible.Customizing);

            StateMachine.PushState(new MixAndMatchState());
        }

        /// <summary>
        /// Launches the multiplayer scene when the network multiplayer button is pressed.
        /// </summary>
        public void OnMultiplayerButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MultiplayerMenu,
                AnalyticsLedger.EventAction.Start,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.MultiplayerLobbyMenu,
                    AnalyticsLedger.TimingVarible.Customizing);

            SceneManager.LoadScene("MultiplayerScene");
        }

        /// <summary>
        /// Returns back to the <see cref="HomeTabState"/>.
        /// </summary>
        public void OnBackButtonClicked()
        {
            StateMachine.ChangeState(new HomeTabState());
        }
    }
}
