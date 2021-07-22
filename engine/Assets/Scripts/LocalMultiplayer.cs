using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;

public class LocalMultiplayer : MonoBehaviour
{
    private GameObject canvas;

    private GameObject multiplayerWindow;
    private GameObject addRobotWindow;

    private GameObject mixAndMatchPanel;

    private GameObject[] robotButtons = new GameObject[6];
    private int activeIndex = 0;
    private GameObject highlight;

    private int robotCount = 0;

    private void Start()
    {
        canvas = GameObject.Find("Canvas");

    }

    /*
    public void ChangeActiveRobot(int index)
    {
        if (index < AnimatorState.SpawnedRobots.count)
        {
            AnimatorState.SwitchActiveRobot(index);
            activeIndex = index;
            UpdateUI();

            AnimatorState.SwitchActiveRobot(index);
        }
    }
    */

    public void ChangeActiveRobot(int index)
    {
        if (index < robotCount)
        {
            
        }
    }

    public void RemoveRobot()
    {
        robotCount--;
    }



}