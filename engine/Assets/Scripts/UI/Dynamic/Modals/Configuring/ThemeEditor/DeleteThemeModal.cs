using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring.ThemeEditor {
    public class DeleteThemeModal : ModalDynamic {
        private const float MODAL_WIDTH  = 300;
        private const float MODAL_HEIGHT = 0;

        public DeleteThemeModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        private string _newThemeName = null;

        public override void Create() {
            ModalIcon.UnityImage.sprite = SynthesisAssetCollection.GetSpriteByName("trash-icon");
            Title.SetText($"Delete {ColorManager.SelectedTheme}?").SetWrapping(false);

            AcceptButton
                .AddOnClickedEvent(x => {
                    ColorManager.DeleteSelectedTheme();
                    DynamicUIManager.CreateModal<EditThemeModal>();
                })
                .ApplyTemplate(Button.EnableCancelButton)
                .StepIntoLabel(l => l.SetText("Delete"));

            CancelButton.AddOnClickedEvent(x => { DynamicUIManager.CreateModal<EditThemeModal>(); })
                .ApplyTemplate(Button.EnableAcceptButton)
                .StepIntoLabel(l => l.SetText("Back"));
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
