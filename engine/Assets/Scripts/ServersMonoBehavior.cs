using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;

public class ServersMonoBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TcpServerManager.Start();
        UdpServerManager.Start();
    }


    void OnDestroy()
    {
        TcpServerManager.Stop();
        UdpServerManager.Stop();
    }
}
