using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LoadFieldState : State
{
    private string fieldDirectory;
    private GameObject mixAndMatchModeScript;
    private GameObject splashScreen;

    public override void Start()
    {
        fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Fields"));
        mixAndMatchModeScript = AuxFunctions.FindGameObject("MixAndMatchModeScript");
        splashScreen = AuxFunctions.FindGameObject("LoadSplash");
    }

    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

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

    public void OnFieldExportButtonPressed()
    {
        Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-field.html");
    }

    public void OnChangeFieldButtonPressed()
    {

    }
}
