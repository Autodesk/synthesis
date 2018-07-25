using Assets.Scripts.Network;
using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

public class ServerNetworkDiscovery : NetworkDiscovery
{
    private void Awake()
    {
        showGUI = false;
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        MultiplayerState state = StateMachine.SceneGlobal.CurrentState as MultiplayerState;

        if (state == null)
            return;

        ServerStatus status = ServerStatus.deserialize(data);

        state.Network.networkAddress = fromAddress;
        state.Network.networkPort = 3674;
        state.Network.StartClient();
    }
}
