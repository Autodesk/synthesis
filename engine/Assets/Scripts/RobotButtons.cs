using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RobotButtons : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject[] robotButtons = new GameObject[6];
    public GameObject p;

    public void Start()
    {
        p = GameObject.Find("Tester");
        for (int i = 0; i < robotButtons.Length; i++)
        {
            robotButtons[i] = GameObject.Find("RobotButton" + (i + 1));
            robotButtons[i].GetComponent<Button>().interactable = false;
        }
    }

    // Update is called once per frame
    public void Update()
    {
        for (int i = 0; i < robotButtons.Length; i++)
        {
            if (p.GetComponent<PTL>().hasRobotAtPosition(i) == true)
            {
                robotButtons[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    private void TaskOnClickMain(int index)
    {    
        p.GetComponent<PTL>().fixTransformPosition(index);
     }

    public void TaskOnClick1()
    {
        TaskOnClickMain(0);
    }

    public void TaskOnClick2()
    {
        TaskOnClickMain(1);
    }

    public void TaskOnClick3()
    {
        TaskOnClickMain(2);
    }

    public void TaskOnClick4()
    {
        TaskOnClickMain(3);
    }

    public void TaskOnClick5()
    {
        TaskOnClickMain(4);
    }

    public void TaskOnClick6()
    {
        TaskOnClickMain(5);
    }
}
