﻿using Assets.Scripts.FSM;
using System;
using UnityEngine;

public class LoadFieldState : State
{
    private string fieldDirectory;
    private GameObject mixAndMatchModeScript;
    private GameObject splashScreen;
    private SelectFieldScrollable fieldList;

    /// <summary>
    /// Initializes required <see cref="GameObject"/> references.
    /// </summary>
    public override void Start()
    {
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Fields"));
        mixAndMatchModeScript = AuxFunctions.FindGameObject("MixAndMatchModeScript");
        splashScreen = AuxFunctions.FindGameObject("LoadSplash");
        fieldList = GameObject.Find("SimLoadFieldList").GetComponent<SelectFieldScrollable>();
    }

    /// <summary>
    /// Updates the field list when this state is activated.
    /// </summary>
    public override void Resume()
    {
        fieldList.Refresh(PlayerPrefs.GetString("FieldDirectory"));
    }

    /// <summary>
    /// Pops this state when the back button is pressed.
    /// </summary>
    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    /// <summary>
    /// When the select field button is pressed, the selected field is saved and
    /// the current state is popped.
    /// </summary>
    public void OnSelectFieldButtonPressed()
    {
        GameObject fieldList = GameObject.Find("SimLoadFieldList");
        string entry = (fieldList.GetComponent<SelectFieldScrollable>().selectedEntry);
        if (entry != null)
        {
            string simSelectedFieldName = fieldList.GetComponent<SelectFieldScrollable>().selectedEntry;
            string simSelectedField = fieldDirectory + "\\" + simSelectedFieldName + "\\";

            if (StateMachine.Instance.FindState<MixAndMatchState>() != null) //Starts the MixAndMatch scene
            {
                PlayerPrefs.SetString("simSelectedField", simSelectedField);
                PlayerPrefs.SetString("simSelectedFieldName", simSelectedFieldName);
                fieldList.SetActive(false);
                splashScreen.SetActive(true);
                mixAndMatchModeScript.GetComponent<MixAndMatchMode>().StartMaMSim();
            }
            else
            {
                PlayerPrefs.SetString("simSelectedField", simSelectedField);
                PlayerPrefs.SetString("simSelectedFieldName", simSelectedFieldName);
                StateMachine.Instance.PopState();
            }
        }
        else
        {
            UserMessageManager.Dispatch("No Field Selected!", 2);
        }
    }

    /// <summary>
    /// Launches the browser and opens the field tutorials webpage when the field exporter
    /// tutorial button is pressed.
    /// </summary>
    public void OnFieldExportButtonPressed()
    {
        Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-field.html");
    }

    /// <summary>
    /// Pushes a new <see cref="BrowseFieldState"/> when the change field directory
    /// button is pressed.
    /// </summary>
    public void OnChangeFieldButtonPressed()
    {
        StateMachine.Instance.PushState(new BrowseFieldState());
    }
}
