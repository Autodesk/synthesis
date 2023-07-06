using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Utilities;
using SynthesisAPI.Simulation;
using UnityEngine;

public class ServerMonobehaviour : MonoBehaviour {
    void Start() {
        SynthesisAPI.Utilities.Logger.Log("Start method called", LogLevel.Debug);
        TcpServerManager.Start();
        UdpServerManager.SimulationObjectsTarget = SimulationManager._simObjects;
        UdpServerManager.Start();
    }

    void OnDestroy() {
        SynthesisAPI.Utilities.Logger.Log(TcpServerManager.IsRunning, LogLevel.Debug);
        TcpServerManager.Stop();
        UdpServerManager.Stop();
    }
}