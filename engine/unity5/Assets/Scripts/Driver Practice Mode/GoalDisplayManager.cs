using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalDisplayManager : MonoBehaviour
{
    public GameObject goalElementPrefab;

    GameObject canvas;

    GameObject goalWindow;

    Transform goalDisplay;

    private DriverPracticeMode dpMode;
    private List<GameObject> goalElements;

    private void Update()
    {
        if (goalDisplay == null)
            FindElements();
    }

    void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        goalWindow = AuxFunctions.FindObject(canvas, "GoalConfigPanel");

        goalDisplay = AuxFunctions.FindObject(goalWindow, "GoalDisplay").transform;

        if (goalDisplay != null)
            Debug.Log("Found goal display elements!");
        else
            Debug.Log("Goal display search failed!");
    }

    /// <summary>
    /// Prepares the goal display to show descriptions and buttons for a set of goals.
    /// </summary>
    /// <param name="descriptions">Descriptions of the goals.</param>
    /// <param name="points">Point value of the goals.</param>
    public void InitializeDisplay(string[] descriptions, int[] points)
    {
        if (goalDisplay != null)
        {
            if (goalElements == null)
                goalElements = new List<GameObject>();

            if (dpMode == null)
                dpMode = AuxFunctions.FindObject("StateMachine").GetComponent<DriverPracticeMode>();

            while (goalElements.Count > 0)
            {
                Destroy(goalElements[0]);
                goalElements.RemoveAt(0);
            }

            for (int i = 0; i < descriptions.Length; i++)
            {
                int id = i;

                GameObject newGoalElement = Instantiate(goalElementPrefab);
                newGoalElement.transform.parent = goalDisplay;
                newGoalElement.name = "Goal" + i.ToString();

                InputField descText = AuxFunctions.FindObject(newGoalElement, "DescriptionText").GetComponent<InputField>();
                descText.text = descriptions[i];
                descText.onValueChanged.AddListener(delegate { dpMode.SetGamepieceGoalDescription(id, descText.text); });

                InputField pointValue = AuxFunctions.FindObject(newGoalElement, "PointValue").GetComponent<InputField>();
                pointValue.text = points[i].ToString();
                pointValue.onValueChanged.AddListener(delegate { dpMode.SetGamepieceGoalPoints(id, int.Parse(pointValue.text)); });

                Button moveButton = AuxFunctions.FindObject(newGoalElement, "MoveButton").GetComponent<Button>();
                moveButton.onClick.AddListener(delegate { dpMode.SetGamepieceGoal(id); });

                Button deleteButton = AuxFunctions.FindObject(newGoalElement, "DeleteButton").GetComponent<Button>();
                deleteButton.onClick.AddListener(delegate { dpMode.DeleteGamepieceGoal(id); });

                goalElements.Add(newGoalElement);
            }
        }
        else Debug.Log("Could not initialize goal display, display not found!");
    }
}
