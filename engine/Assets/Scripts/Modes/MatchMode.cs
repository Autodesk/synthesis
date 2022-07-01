using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MatchMode : GameMode
{
    public GameObject board;
    public TMP_Text timer;

    public float targetTime = 135;
    bool runTimer = false;

    private static MatchMode instance;
    public static MatchMode GetInstance()
    {
        if (instance != null) instance = new MatchMode();
        return instance;
    }


    // Start is called before the first frame update
    public override void Start()
    {
        instance = this;
        board.SetActive(false);
    }

    // Update is called once per frame
    public override void Update()
    {
        if (runTimer)
        {
            targetTime -= Time.deltaTime;
            if (targetTime >= 0) timer.text = Mathf.RoundToInt(targetTime).ToString();
            else
            {
                runTimer = false;
                EndMatch();
            }
        }


    }
    public override void End()
    {
    }
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if (e.keyCode == KeyCode.M && DynamicUIManager.ActiveModal == null)
            {
                DynamicUIManager.CreateModal<MatchModeModal>();
            }
            if (robotLoaded && GizmoManager.currentGizmo == null)
            {
                robotLoaded = false;
                StartMatch();
            }
        }
    }
    private bool robotLoaded = false;
    public void SpawnedIn()
    {
        robotLoaded = true;
        board.SetActive(true);
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
