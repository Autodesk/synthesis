using Synthesis.FSM;

namespace Synthesis.States
{
    public class SimTabState : State
    {
        /// <summary>
        /// Immediately pushes a new <see cref="SelectionState"/> (the default substate).
        /// </summary>
        public override void Start()
        {
            StateMachine.PushState(new SelectionState());
        }
    }
}
