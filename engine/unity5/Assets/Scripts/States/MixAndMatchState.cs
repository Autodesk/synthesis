using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MixAndMatchState : State
{
    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    public void OnNextButtonPressed()
    {
        StateMachine.Instance.PushState(new LoadFieldState());
    }
}
