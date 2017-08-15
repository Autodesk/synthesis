using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalManager : MonoBehaviour
{
    public List<GameObject> goalElements;

    private GameObject DPMode;

    int currentGamepiece;

    public void InitGamepiece(int index)
    {
        currentGamepiece = index;

        for (int i = 0; i < goalElements.Count; i++)
            goalElements[i].SetActive(false);

        //DPRobot = AuxFunctions.FindObject(goalElements[nextGoal], "DescriptionText")
    }

    public void NewGoal()
    {
        // Update UI elements
        int nextGoal = 0;
        while (nextGoal < goalElements.Count && goalElements[nextGoal].activeSelf) // Iterate through goal elements until finding one that's disabled or until out of elements
            nextGoal++;

        if (nextGoal < goalElements.Count)
        {
            goalElements[nextGoal].SetActive(true);

            AuxFunctions.FindObject(goalElements[nextGoal], "DescriptionText").GetComponent<Text>().text = "New Goal";
        }
        else
        {
            UserMessageManager.Dispatch("You cannot have more than " + goalElements.Count.ToString() + " goals per gamepiece", 5);
        }

        // Update goal list

    }

    public void RemoveGoal(int index)
    {
        // Update UI elements
        int i = index;

        for (; i < goalElements.Count - 1 && goalElements[i + 1].activeSelf; i++)
        {
            Text textA = AuxFunctions.FindObject(goalElements[i], "DescriptionText").GetComponent<Text>();
            Text textB = AuxFunctions.FindObject(goalElements[i + 1], "DescriptionText").GetComponent<Text>();

            textA.text = textB.text;
        }

        goalElements[i].SetActive(false);
    }
}
