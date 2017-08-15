using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTimer : MonoBehaviour {

    [SerializeField]

    private Text gameplayTimer = null;
    void Update()
    {
        runTimer();
    }
    // Use this for initialization
    void Start () {
		
	}
 
    public static bool timeRunning;

    public static double timeLeft = 135.0;

    public void runTimer()
    {
        if (timeRunning == true)
        {
            timeLeft -= Time.deltaTime;
            int intTimeLeft = (int)timeLeft;
            string timeLeftText = intTimeLeft.ToString();
            gameplayTimer.text = timeLeftText;
            if (timeLeft < 0)
            {
                GameBehaviour.GameEnd();
                //return;
            }
        }
    }

   
    }

