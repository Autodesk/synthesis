using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.ModelManager;
using Synthesis.ModelManager.Models;

public class MultiplayerManager : MonoBehaviour
{
    private GameObject[] robotButtons = new GameObject[6];
    private PTL p;

    private Color disabled = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    private Color enabled = new Color(0.5f, 0.5f, 0.5f, 1f);
    private Color selected = new Color(0.1294118f, 0.5882353f, 0.9529412f, 1f);

    public void Start()
    {
        p = GameObject.Find("Tester").GetComponent<PTL>();
        for (int i = 0; i < robotButtons.Length; i++)
        {
            robotButtons[i] = GameObject.Find("RobotButton" + (i + 1));
            robotButtons[i].GetComponent<Button>().interactable = false;
            robotButtons[i].GetComponent<Image>().color = disabled;
        }
        SetButtonColors();
    }
    public void SetButtonColors()
    {
        for(int i = 0; i < ModelManager.Models.Count; i++)
        {
            robotButtons[i].GetComponent<Button>().interactable = true;
            robotButtons[i].GetComponent<Image>().color = enabled;
        }
        robotButtons[p.robotIndex].GetComponent<Image>().color = selected;
    }

    public void SwitchRobot(int index)
    {
        p.SetCameraTransform(index);
    }

    private void Update()
    {
        SetButtonColors();
    }
}
