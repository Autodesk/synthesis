using Synthesis.FSM;

namespace Synthesis.States
{
    public class SelectionState : State
    {
        /// <summary>
        /// Opens the default simulator tab when the main simulator button is pressed.
        /// </summary>
        public void OnMainSimulatorButtonPressed()
        {
            StateMachine.PushState(new DefaultSimulatorState());
        }

        /// <summary>
        /// Opens the mix and match tab when the mix and match button is pressed.
        /// </summary>
        public void OnMixAndMatchButtonPressed()
        {
            StateMachine.PushState(new MixAndMatchState());
        }
        public void OnBackButtonPressed()
        {
            StateMachine.ChangeState(new HomeTabState());
        }
    }
}
