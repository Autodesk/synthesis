using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{

    [SerializeField]

    private Text score = null; 
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

    public void GameStart()
    {
        GameBehaviour.GameStart();
    }

}
public static class GameBehaviour : object
{
    public static void GameStart()
    {
        Image img = GameObject.Find("ScoreTextPanel").GetComponent<Image>();
        img.color = UnityEngine.Color.green;
        GameplayTimer.timeRunning = true;
    }

    public static void GameEnd()
    {
        GameplayTimer.timeLeft = 135.0;
        GameplayTimer.timeRunning = false;
        Image img = GameObject.Find("ScoreTextPanel").GetComponent<Image>();
        img.color = UnityEngine.Color.red;
    }
}
