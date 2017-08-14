using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour {

    [SerializeField]

    private Text score = null; //this allows us to set the text feild in the editor.

    private void Update()
    {   
        
    }
   
    // To change text do : score.text = "";

    int totalPoints = 0;

    public void addPoints(int points)
    {
        totalPoints = totalPoints + points;
        string displayPoints = totalPoints.ToString();
        score.text = displayPoints;
    }

    int oldHighscore = PlayerPrefs.GetInt("highscore", 0);

    public void StoreHighscore(int newHighscore)
    {
            PlayerPrefs.SetInt("highscore", newHighscore);
    }
}
