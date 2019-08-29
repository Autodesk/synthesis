using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.States;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorMenu : MonoBehaviour
{
    public void Start()
    {
        UICallbackManager.RegisterButtonCallbacks(StateMachine.SceneGlobal, gameObject);
        UICallbackManager.RegisterDropdownCallbacks(StateMachine.SceneGlobal, gameObject);

        StateMachine.SceneGlobal.ChangeState(new ErrorScreenState());
        // StateMachine.SceneGlobal.ChangeState(new LoadRobotState());
    }
}
