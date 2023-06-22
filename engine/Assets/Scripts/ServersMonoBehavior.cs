using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServersMonoBehavior : MonoBehaviour {
    private void Start() {
        TcpServerManager.Start();
        UdpServerManager.Start();
    }

    private void OnDestroy() {
        TcpServerManager.Stop();
        UdpServerManager.Stop();
    }
}
