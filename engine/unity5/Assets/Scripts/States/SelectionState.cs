using Assets.Scripts.FSM;

public class SelectionState : State
{
    /// <summary>
    /// Opens the default simulator tab when the main simulator button is pressed.
    /// </summary>
    public void OnMainSimulatorButtonPressed()
    {
        StateMachine.Instance.PushState(new DefaultSimulatorState());
    }

    /// <summary>
    /// Opens the mix and match tab when the mix and match button is pressed.
    /// </summary>
    public void OnMixAndMatchButtonPressed()
    {
        StateMachine.Instance.PushState(new MixAndMatchState());
    }
}
