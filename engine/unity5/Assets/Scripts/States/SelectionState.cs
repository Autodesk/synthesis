using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SelectionState : State
{
    public void OnMainSimulatorButtonPressed()
    {
        StateMachine.Instance.PushState(new DefaultSimulatorState());
    }

    public void OnMixAndMatchButtonPressed()
    {
        StateMachine.Instance.PushState(new MixAndMatchState());
    }
}
