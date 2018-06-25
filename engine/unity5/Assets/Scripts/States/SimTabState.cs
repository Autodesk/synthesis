using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SimTabState : State
{
    public override void Start()
    {
        StateMachine.Instance.PushState(new SelectionState());
    }
}
