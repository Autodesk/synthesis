using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Assets.Scripts.FSM;
using System.IO;

public class MaMSimUI : MonoBehaviour {
    MainState main;
    GameObject canvas;

    GameObject mixAndMatchPanel;
    GameObject multiplayerPanel;

    private SimUI simUI;

    void Start()
    {
        FindElements();
    }

    private void OnGUI()
    {
        UserMessageManager.Render();
    }

    /// <summary>
    /// Finds all the necessary UI elements that need to be updated/modified
    /// </summary>
    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        mixAndMatchPanel = AuxFunctions.FindObject(canvas, "MixAndMatchPanel");
        multiplayerPanel = AuxFunctions.FindObject(canvas, "MultiplayerPanel");

        simUI = StateMachine.Instance.gameObject.GetComponent<SimUI>();
    }

    public void ToggleMaMPanel()
    {
        if (mixAndMatchPanel.activeSelf)
        {
            mixAndMatchPanel.SetActive(false);
        }
        else
        {
            simUI.EndOtherProcesses();
            mixAndMatchPanel.SetActive(true);
        }
    }

    public void ToggleMaMInMultiplayer()
    {
        if (mixAndMatchPanel.activeSelf)
        {
            mixAndMatchPanel.SetActive(false);
            multiplayerPanel.SetActive(true);
        }
        else
        {
            simUI.EndOtherProcesses();
            mixAndMatchPanel.SetActive(true);
            multiplayerPanel.SetActive(true);
        }
    }
}
