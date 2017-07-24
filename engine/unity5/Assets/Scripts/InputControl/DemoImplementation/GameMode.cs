using UnityEngine;


public class GameMode : MonoBehaviour
{
    public GameObject settingsMode;
    //public InputControlDemo_PlayerScript playerScript;

    // Update is called once per frame
    void Update()
    {
        if (InputControl.GetKeyDown(KeyCode.Escape))
        {
            settingsMode.SetActive(true);
            gameObject.SetActive(false);

            //playerScript.enabled = false;
        }
    }
}
