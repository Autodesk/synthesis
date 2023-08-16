using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring.ThemeEditor {
    public class DeleteAllThemesModal : ModalDynamic {
        private const float MODAL_WIDTH  = 300;
        private const float MODAL_HEIGHT = 0;

        public DeleteAllThemesModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        private string _newThemeName = null;

        public override void Create() {
            Title.SetText($"Delete All Themes?").SetWrapping(false);

            AcceptButton
                .AddOnClickedEvent(
                    _ => {
                        ColorManager.DeleteAllThemes();
                        DynamicUIManager.CreateModal<EditThemeModal>();
                    })
                .ApplyTemplate(Button.EnableCancelButton)
                .StepIntoLabel(l => l.SetText("Delete"));

            CancelButton.AddOnClickedEvent(x => { DynamicUIManager.CreateModal<EditThemeModal>(); })
                .ApplyTemplate(Button.EnableAcceptButton)
                .StepIntoLabel(l => l.SetText("Back"));

            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"))
                .SetColor(ColorManager.SynthesisColor.MainText);
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
