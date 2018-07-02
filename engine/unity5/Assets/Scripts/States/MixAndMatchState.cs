using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MixAndMatchState : State
{
    /// <summary>
    /// Returns to the previous scene when the back button is pressed.
    /// </summary>
    public void OnBackButtonPressed()
    {
        StateMachine.PopState();
    }

    /// <summary>
    /// Launches a new <see cref="LoadFieldState"/> when the next button is pressed.
    /// </summary>
    public void OnNextButtonPressed()
    {
        StateMachine.PushState(new LoadFieldState());
    }
}
