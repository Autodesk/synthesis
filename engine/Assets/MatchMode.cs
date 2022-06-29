using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class MatchMode : MonoBehaviour
{
    public GameObject board;
    public TMP_Text timer;

    public float targetTime = 135;
    bool runTimer = false;


    // Start is called before the first frame update
    void Start()
    {
        isMatchModalOpen = false;
        board.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (runTimer)
        {
            targetTime -= Time.deltaTime;
            if (targetTime >= 0) timer.text = Mathf.RoundToInt(targetTime).ToString();
            else
            {
                runTimer = false;
                EndMatch();
            }
        }


    }
    bool isMatchModalOpen;
    void OnGUI()
    {
        Event e = Event.current;
        if (e.isKey)
        {
            if(e.keyCode == KeyCode.M && !isMatchModalOpen)
            {
                isMatchModalOpen = true;
                ModalManager.CreateModal<MatchModeModal>();
            }
            else if(e.keyCode == KeyCode.Escape && isMatchModalOpen)
            {
                ModalManager.CloseModal();
                isMatchModalOpen = false;
            }
            else if(e.keyCode == KeyCode.S && isMatchModalOpen)
            {
                ModalManager.CloseModal();
                isMatchModalOpen = false;
                StartMatch();
            }
        }
    }
    private void StartMatch()
    {
        Debug.Log("Match Started");
        //start timer
        board.SetActive(true);
        runTimer = true;
        //show scoreboard
    }
    private void EndMatch()
    {
        Debug.Log("Match Ended");
    }
}
