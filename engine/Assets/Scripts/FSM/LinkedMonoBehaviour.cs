using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.FSM
{
    /// <summary>
    /// This class can be used to automatically attach a <see cref="MonoBehaviour"/>
    /// to a state run by <see cref="StateMachine.SceneGlobal"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LinkedMonoBehaviour<T> : MonoBehaviour where T : State
    {
        /// <summary>
        /// The state associated with this StateBehaviour.
        /// </summary>
        protected T State { get; private set; }

        /// <summary>
        /// Links this instance to the global <see cref="StateMachine"/>.
        /// </summary>
        protected virtual void Awake()
        {
            StateMachine.SceneGlobal.Link<T>(this);
        }

        /// <summary>
        /// Assigns the State property to the current State in the StateMachine
        /// </summary>
        protected virtual void OnEnable()
        {
            State = StateMachine.SceneGlobal.CurrentState as T;
        }
    }
}
