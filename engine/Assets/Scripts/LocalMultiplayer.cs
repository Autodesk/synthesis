
using UnityEngine;


// the logic here should be to increment the Robot counter, and if the count that the gameObject is in becomes full then make the button active

public class LocalMultiplayer : MonoBehaviour
{
    private GameObject canvas;
    private GameObject button;

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