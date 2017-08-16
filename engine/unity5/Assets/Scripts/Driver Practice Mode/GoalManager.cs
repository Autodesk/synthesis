using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalManager : MonoBehaviour
{
    public VerticalLayoutGroup goalDisplay;
    public GameObject goalElementPrefab;

    private DriverPracticeMode dpMode;
    private List<GameObject> goalElements;

    /// <summary>
    /// Prepares the goal display to show descriptions and buttons for a set of goals.
    /// </summary>
    /// <param name="descriptions">Descriptions of the goals.</param>
    /// <param name="points">Point value of the goals.</param>
    public void InitializeDisplay(string[] descriptions, int[] points)
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
            GameObject newGoalElement = Instantiate(goalElementPrefab);
            newGoalElement.transform.parent = goalDisplay.gameObject.transform;
            newGoalElement.name = "Goal" + i.ToString();

            Text descText = AuxFunctions.FindObject(newGoalElement, "DescriptionText").GetComponent<Text>();
            descText.text = descriptions[i] + " (" + points[i].ToString() + " points)";

            int id = i;
            Button moveButton = AuxFunctions.FindObject(newGoalElement, "MoveButton").GetComponent<Button>();
            moveButton.onClick.AddListener(() => { dpMode.SetGamepieceGoal(id); });

            Button deleteButton = AuxFunctions.FindObject(newGoalElement, "DeleteButton").GetComponent<Button>();
            deleteButton.onClick.AddListener(() => { dpMode.DeleteGamepieceGoal(id); });
            
            goalElements.Add(newGoalElement);
        }
    }
}
