using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.FSM;
using BulletUnity;


/// <summary>
/// Meant to be used for selected dummies within the simulator
/// </summary>
public class DummyScrollable : ScrollablePanel
{
    private MainState mainState;

    // Use this for initialization
    protected override void Start()
    {

        base.Start();
        listStyle.fontSize = 12;
        highlightStyle.fontSize = 12;
        toScale = false;
        errorMessage = "No dummies in field.";
    }
   

    void OnEnable()
    {
        items = new List<string>();
        items.Clear();
    }

    private void Update()
    {
        if (mainState == null) mainState = StateMachine.Instance.FindState<MainState>();
    }
    /*
    // Update is called once per frame
    protected override void OnGUI()
    {
        if ( items.Count == 0)
        {
            for (int i = 0; i < mainState.dummyRootNodes.Count; i++)
            {
                items.Add("Dummy Robot " + i);
            }

            if (items.Count > 0) selectedEntry = items[0];
        }

        position = GetComponent<RectTransform>().position;

        base.OnGUI();

    }

    public void AddDummy()
    {
        mainState.SpawnDummyRobot(PlayerPrefs.GetString("simSelectedRobot"));
        items.Add("Dummy Robot " + items.Count);
    }

    public void DeleteDummy()
    {
        if (selectedEntry != null)
        {
            GameObject.Destroy(((RigidNode)mainState.activeRobot).MainObject.transform.parent);
            items.Remove(selectedEntry);
            selectedEntry = null;
        }
    }

    public void ControlDummy()
    {
        if (selectedEntry != null)
        {
            Debug.Log(mainState.dummyRootNodes);    
            mainState.activeRobot = mainState.dummyRootNodes[items.IndexOf(selectedEntry)];

            //This slightly alters the velocity of the newly controlled dummy object, which fixes a bug where switching controls will sometimes not work.
            //The cause of the bug is unknown, but control is regained when the dummy object makes any sort of collision, which is why changing the velocity helps.
            ((RigidNode)mainState.activeRobot).MainObject.GetComponent<BRigidBody>().GetComponentInChildren<BRigidBody>().velocity = Vector3.up*5;
        }
    }*/
}
