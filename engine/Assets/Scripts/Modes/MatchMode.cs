using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MatchMode : IMode
{
    //PracticeMode practiceMode = new PracticeMode();
    public static int currentFieldIndex = -1;
    public static int currentRobotIndex = -1;

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

    // Start is called before the first frame update
    public void Start()
    {
        DynamicUIManager.CreateModal<MatchModeModal>();
    }

    // Update is called once per frame
    public void Update()
    {


    }
    public void End()
    {
    }

    public void OpenMenu(){}

    public void CloseMenu(){}

}
