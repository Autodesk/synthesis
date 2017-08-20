using UnityEngine;
using System.Collections;
using System;

public class ChaseSynthBehaviour : BaseSynthBehaviour
{
    private Transform player;

    protected override void AILogic()
    {
        this.agent.SetDestination(player.position);
        
    }

    public override bool Initialize(MainState main)
    {
        this.player = main.activeRobot.transform.GetChild(0).GetComponentInChildren<Transform>();
        this.driveNow = true;
        Debug.Log("Chase behaviour initialized");
        return true;
    }

    public override string ToString()
    {
        return "Chase";
    }
}
