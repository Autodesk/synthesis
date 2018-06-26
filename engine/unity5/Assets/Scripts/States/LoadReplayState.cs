using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LoadReplayState : State
{
    private GameObject splashScreen;

    public override void Start()
    {
        splashScreen = AuxFunctions.FindGameObject("LoadSplash");
    }

    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    public void OnDeleteButtonPressed()
    {
        GameObject replayList = GameObject.Find("SimLoadReplayList");
        string entry = replayList.GetComponent<ScrollableList>().selectedEntry;

        if (entry != null)
        {
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Synthesis\\Replays\\" + entry + ".replay");
            replayList.SetActive(false);
            replayList.SetActive(true);
        }
    }

    public void OnLaunchButtonPressed()
    {
        GameObject replayList = GameObject.Find("SimLoadReplayList");
        string entry = replayList.GetComponent<ScrollableList>().selectedEntry;

        if (entry != null)
        {
            splashScreen.SetActive(true);
            PlayerPrefs.SetString("simSelectedReplay", entry);
            PlayerPrefs.Save();
            Application.LoadLevel("Scene");
        }
    }
}
