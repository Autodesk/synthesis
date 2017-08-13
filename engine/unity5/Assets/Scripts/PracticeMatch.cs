using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticeMatch : MonoBehaviour {

    private float keyDownTime;
    private bool released; //Makes sure the window won't just toggle every half second the key is held down

    //Match Management
    private GameObject canvas;
    private GameObject practiceMatchWindow;
    private GameObject confirmationWindow;
    private GameObject practiceMatchClose;
    
    private float matchTime;
    private float tempTime;

    private enum GameState {Auto, Tele, EndGame, Over}; //If gameState is in Over, then the match is not currently running.
    private GameState gameState;

    private AudioSource auto;
    private AudioSource tele;
    private AudioSource end;
    private AudioSource over;

    // Use this for initialization
    void Start () {
        canvas = GameObject.Find("Canvas");
        matchTime = 0;
        tempTime = 0;
        practiceMatchWindow = AuxFunctions.FindObject(canvas, "PracticeMatchPanel");
        practiceMatchClose = AuxFunctions.FindObject(canvas, "ClosePracticeMatch");
        confirmationWindow = AuxFunctions.FindObject(canvas, "ConfirmationPanel");

        Controls.ArcadeDrive(); //If I don't do this then the toggle key for the window doesn't get set to an instance, but it still feels wrong

        auto = GameObject.Find("Auto").GetComponent<AudioSource>();
        tele = GameObject.Find("Tele").GetComponent<AudioSource>();
        end = GameObject.Find("End").GetComponent<AudioSource>();
        over = GameObject.Find("Over").GetComponent<AudioSource>();

        gameState = GameState.Over;
    }
	
	// Update is called once per frame
	void Update () {
        if (gameState != GameState.Over) //Don't waste processing power if there isn't a match currently running
        {
            UpdateMatchTime();
            tempTime = GetMatchTime(); //Only call the function once
            GameObject.Find("Countdown").GetComponent<Text>().text = tempTime.ToString(); //Set the time text

            //Set game states and match timer color based on match time
            if (tempTime > 30 && tempTime < 120 && gameState != GameState.Tele)
            {
                gameState = GameState.Tele;
                tele.Play();
                GameObject.Find("Countdown").GetComponent<Text>().color = Color.green;
                GameObject.Find("PracticeHeader").GetComponent<Text>().text = "Teleop";
            }
            else if (tempTime < 30 && gameState != GameState.EndGame)
            {
                gameState = GameState.EndGame;
                end.Play();
                GameObject.Find("Countdown").GetComponent<Text>().color = Color.yellow;
                GameObject.Find("PracticeHeader").GetComponent<Text>().text = "End Game";
            }
            else if (tempTime <= 0 && gameState != GameState.Over)
            {
                gameState = GameState.Over;
                over.Play();
                GameObject.Find("Countdown").GetComponent<Text>().color = Color.red;
                GameObject.Find("PracticeHeader").GetComponent<Text>().text = "Practice Match";

                //Reset the match
                practiceMatchClose.SetActive(true); //Make it so they can close the panel again
                matchTime = 0;
                tempTime = 0;
            }
        }

        //Lovingly stolen from Robot
        if(InputControl.GetButtonDown(Controls.buttons[0].practiceMatch)) //When the button is first pushed down
        {
            keyDownTime = Time.time;
            released = false;
        }
        else if(InputControl.GetButton(Controls.buttons[0].practiceMatch) && !released && gameState == GameState.Over) //Don't allow window toggling during a match
        {
            if(Time.time - keyDownTime > 0.5f) //Make sure it wasn't a mis hit
            {
                TogglePracticeMatchWindow();
                released = true;
            }
        }
	}

    //Called when the Start Match button is pressed
    public void StartMatch()
    {
        GameObject.Find("Countdown").GetComponent<Text>().text = "135";
        GameObject.Find("Countdown").GetComponent<Text>().color = Color.green;
        GameObject.Find("PracticeHeader").GetComponent<Text>().text = "Auto";  //Visual heads up of which period the user is in
        practiceMatchClose.SetActive(false); //If the window closes during the match bad things happen
        ToggleConfirmationWindow();
        gameState = GameState.Auto;
        matchTime = 135;
        auto.Play();
    }

    //Called every frame to update the match time
    private void UpdateMatchTime()
    {
        matchTime -= Time.deltaTime; //Countdown timer, don't count up
    }

    //Round match time to seconds
    public int GetMatchTime()
    {
        return (int)Mathf.Round(matchTime * 100) / 100;
    }

    //Lovingly stolen from ToggleToolkitWindow
    public void TogglePracticeMatchWindow()
    {
        practiceMatchWindow.SetActive(!practiceMatchWindow.activeSelf);
    }

    public void ToggleConfirmationWindow()
    {
        confirmationWindow.SetActive(!confirmationWindow.activeSelf);
    }
}
