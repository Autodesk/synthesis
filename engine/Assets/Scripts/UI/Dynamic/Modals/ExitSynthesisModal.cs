using Synthesis.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Synthesis.UI.Dynamic {
    public class ExitSynthesisModal : ModalDynamic {
        public ExitSynthesisModal() : base(new Vector2(350, 50)) {
        }

        public override void Create() {
            bool isOnMainMenu = SceneManager.GetActiveScene().name != "MainScene";

            Title.SetText("Exit Synthesis");
            Description.SetText("");
            AcceptButton
                .AddOnClickedEvent(x => {
                    if (isOnMainMenu)
                        Application.Quit();
                    else {
                        SimulationRunner.InSim = false;
                        DynamicUIManager.CloseAllPanels(true);
                        SceneManager.LoadScene("GridMenuScene", LoadSceneMode.Single);
                    }
                })
                .StepIntoLabel(l => l.SetText("Exit"));
            ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"));
            ModalImage.SetColor(ColorManager.SYNTHESIS_WHITE);

            MainContent.CreateLabel(40)
                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center)
                .SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Middle)
                .SetAnchoredPosition<Label>(new Vector2(0, 15))
                .SetText(
                    isOnMainMenu ? "Are you sure you wish to Exit?" : "Are you sure you wish to\nleave to main menu?")
                .SetFont(SynthesisAssetCollection.GetFont("Roboto-Regular SDF"))
                .SetFontSize(20);
        }

        public override void Update() {
        }

        public override void Delete() {
        }
    }
}
