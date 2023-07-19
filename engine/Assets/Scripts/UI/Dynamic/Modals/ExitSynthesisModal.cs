using System.Collections;
using System.Collections.Generic;
using Analytics;
using Synthesis.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.ColorManager;

namespace Synthesis.UI.Dynamic {
    public class ExitSynthesisModal : ModalDynamic {
        public ExitSynthesisModal() : base(new Vector2(350, 50)) {}

        public override void Create() {
            bool isOnMainMenu = SceneManager.GetActiveScene().name != "MainScene";

            Title.SetText("Exit Synthesis");

            AcceptButton
                .AddOnClickedEvent(x => {
                    if (isOnMainMenu) {
                        Application.Quit();
                    } else {
                        SimulationRunner.InSim = false;
                        DynamicUIManager.CloseAllPanels(true);
                        ModeManager.CurrentMode = null;
                        SceneManager.LoadScene("GridMenuScene", LoadSceneMode.Single);

                        AnalyticsManager.LogCustomEvent(AnalyticsEvent.ExitedToMenu);
                    }
                })
                .StepIntoLabel(l => l.SetText("Exit"));

            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"))
                .SetColor(ColorManager.SynthesisColor.MainText);

            MainContent.CreateLabel(40)
                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                .SetAnchoredPosition<Label>(new Vector2(0, 15))
                .SetText(
                    isOnMainMenu ? "Are you sure you wish to Exit?" : "Are you sure you wish to\nleave to main menu?")
                .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"))
                .SetFontSize(20);
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
