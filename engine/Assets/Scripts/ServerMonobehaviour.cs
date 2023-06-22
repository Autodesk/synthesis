using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerMonobehaviour : MonoBehaviour {
    private void Start() {
        SynthesisAPI.Utilities.Logger.Log("Start method called", LogLevel.Debug);
        TcpServerManager.Start();
        UdpServerManager.SimulationObjectsTarget = SimulationManager._simObjects;
        UdpServerManager.Start();
    }

    private void OnDestroy() {
        SynthesisAPI.Utilities.Logger.Log(TcpServerManager.IsRunning, LogLevel.Debug);
        TcpServerManager.Stop();
        UdpServerManager.Stop();
    }
}
