using UnityEngine;
using System.Collections;
using System;

public class RoamSynthBehaviour: BaseSynthBehaviour
{

    protected override void AILogic()
    {
        //this.agent.SetDestination(player.position);
        
    }

    public override bool Initialize(MainState main)
    {
        this.driveNow = true;
        Debug.Log("Roam behaviour initialized");
        return true;
    }

    public override string ToString()
    {
        return "Roam";
    }
}
