using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MatchMode : GameMode
{
    public TMP_Text timer;

    public float targetTime = 135;
    bool runTimer = false;

    // Start is called before the first frame update
    public override void Start()
    {
        //DynamicUIManager.CreateModal<MatchModeModal>();
        DynamicUIManager.CreatePanel<MatchmodeScoreboardPanel>();

    }

    // Update is called once per frame
    public override void Update()
    {


    }
    public override void End()
    {
    }
    
    
    private void StartMatch()
    {
        Debug.Log("Match Started");
        //start timer
        runTimer = true;
        //show scoreboard
    }
    private void EndMatch()
    {
        Debug.Log("Match Ended");
    }
}
