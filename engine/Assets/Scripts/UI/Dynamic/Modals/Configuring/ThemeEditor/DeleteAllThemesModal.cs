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

            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("CloseIcon"));

            AcceptButton.AddOnClickedEvent(x => {
                ColorManager.DeleteAllThemes();
                DynamicUIManager.CreateModal<EditThemeModal>();
            });

            CancelButton.AddOnClickedEvent(x => { DynamicUIManager.CreateModal<EditThemeModal>(); });
        }

        public override void Update() {}

        public override void Delete() {}
    }
}