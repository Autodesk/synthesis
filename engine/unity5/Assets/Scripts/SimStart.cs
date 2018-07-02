using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Synthesis.States;

public class SimStart : MonoBehaviour
{
    /// <summary>
    /// Starts the simulation by adding a <see cref="MainState"/> to the scene's <see cref="StateMachine"/>.
    /// </summary>
    private void Start()
    {
        StateMachine stateMachine = GetComponent<StateMachine>();

        if (stateMachine == null)
            Debug.LogError("Could not find the required StateMachine component!");
        else
            stateMachine.PushState(new MainState());
    }
}
