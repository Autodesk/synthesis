using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MatchMode : IMode
{

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
