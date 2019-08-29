using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Synthesis.GUI
{

    public class Wizard : ScriptableObject
    {

        GameObject mainPanel;
        GameObject back;
        GameObject next;

        GameObject[] screens;
        private int screenOn;

        public Wizard(GameObject mainPanel, GameObject backButton, GameObject nextButton, GameObject[] screens)
        {
            this.mainPanel = mainPanel;
            this.screens = screens;
            screenOn = -1;
            back = backButton;
            next = nextButton;
        }

        public void LoadNewScreen()
        {
            screens[screenOn].SetActive(true);
            back.SetActive(screenOn > 0);
            next.GetComponentInChildren<Text>().text = (screenOn < screens.Length - 1) ? "Next" : "Done";
        }

        public void Next()
        {
            if (screenOn + 1 == screens.Length)
            {
                Close();
            }
            else
            {
                screens[screenOn].SetActive(false);
                screenOn += 1;
                LoadNewScreen();
            }
        }

        public void Back()
        {
            screens[screenOn].SetActive(false);
            screenOn -= 1;
            LoadNewScreen();
        }

        public void ToggleOpen(int screen)
        {
            if (mainPanel.activeSelf && screenOn == screen)
            {
                Close();
            }
            else
            {
                mainPanel.SetActive(true);
                screenOn = screen;
                LoadNewScreen();
            }
        }

        public void Close()
        {
            screens[screenOn].SetActive(false);
            mainPanel.SetActive(false);
            screenOn = -1;
        }

    }

}