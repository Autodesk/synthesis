using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class StateBehaviour<T> : MonoBehaviour where T : State
{
    /// <summary>
    /// The state associated with this StateBehaviour.
    /// </summary>
    protected T State { get; private set; }

    /// <summary>
    /// Links this instance to the given State type.
    /// </summary>
    protected virtual void Awake()
    {
        StateMachine.Instance.Link<T>(this);
    }

    /// <summary>
    /// Assigns the State property to the current State in the StateMachine
    /// </summary>
    protected virtual void OnEnable()
    {
        if ((State = StateMachine.Instance.CurrentState as T) == null)
            Debug.LogError("Component \"" + name +
                "\" could not establish a conection to the state " +
                typeof(T).ToString() + ".");
    }
}
