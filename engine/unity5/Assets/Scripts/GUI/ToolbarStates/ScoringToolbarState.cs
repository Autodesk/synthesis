using Synthesis.DriverPractice;
using Synthesis.FSM;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// The state that controls the scoring toolbar and related state functions. Because of the nature of the specific functions,
    /// the scoring toolbar buttons are tethered within Unity.
    /// </summary>
    public class ScoringToolbarState : State
    {
        GameObject canvas;

        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        GameObject tabs;
        Text helpBodyText;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");

            tabs = Auxiliary.FindObject(canvas, "Tabs");
            toolbar = Auxiliary.FindObject(canvas, "ScoringToolbar");
            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);
        }

        public void OnHelpButtonClicked()
        {
            helpMenu.SetActive(true);

            helpBodyText.GetComponent<Text>().text = "Open Setup Menu: Click SCORE ZONES to begin set up" +
                "\n\nSwitch Gamepieces: To change the gamepiece that will be connected to the score zone use the tabs at the top of the set up window" +
                "\n\nSwitch Alliances: To change the alliance that will be connected to the score zone use the tabs to the left of the set up window" +
                "\n\nChange Goal Name: Modify text entry labeled DESCRIPTION" +
                "\n\nChange Point Value: To change the point value associated with a goal modify text entry labeled POINTS" +
                "\n\nChange Goal Location: Click MOVE next to desired goal and use WASD keys or drag navigation arrows. Click and drag a face of the navigation cube to move goal freely, press ENTER to save" +
                "\n\nChange Goal Size: Click SCALE next to desired goal and drag arrows to scale.Click and drag a face of the orientation cube to scale goal freely, press ENTER to save" +
                "\n\nAdd Goal: Navigate to desired gamepiece and alliance tabs then click NEW RED GOAL/ NEW BLUE GOAL" +
                "\n\nView Live Score: Click SCOREBOARD in the toolbar";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "ScoringToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(300, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0));
                else t.gameObject.SetActive(false);
            }

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.Help,
                AnalyticsLedger.EventAction.Viewed,
                "Help - Scoring Toolbar",
                AnalyticsLedger.getMilliseconds().ToString());
        }
        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }

        public override void ToggleHidden()
        {
            toolbar.SetActive(!toolbar.activeSelf);
        }
    }
}