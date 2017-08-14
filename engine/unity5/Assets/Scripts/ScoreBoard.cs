using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour {

    [SerializeField]
    private Text score = null; //this allows us to set the text feild in the editor.

    // To change text do : score.text = "";
    int totalPoints = 0;
    void addPoints(int points)
    {
        totalPoints = totalPoints + points;
        string displayPoints = totalPoints.ToString();
        score.text = displayPoints;
    }
}
