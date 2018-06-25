using Assets.Scripts.FSM;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SimStart : MonoBehaviour
{
    /// <summary>
    /// Starts the simulation by adding a <see cref="MainState"/> to the scene's <see cref="StateMachine"/>.
    /// </summary>
    private void Start()
    {
        StateMachine.Instance.PushState(new MainState());
    }
}
