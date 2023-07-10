using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring
{
    public class DeleteThemeModal : ModalDynamic
    {
        private const float MODAL_WIDTH = 500;
        private const float MODAL_HEIGHT = 0;
        
        public DeleteThemeModal()
            : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) { }

        private string _newThemeName = null;

        public override void Create() {
            Title.SetText($"Delete Theme {ColorManager.SelectedTheme}?");
            Description.RootGameObject.SetActive(false);

            AcceptButton.AddOnClickedEvent(x =>
            {
                ColorManager.DeleteSelectedTheme();
                DynamicUIManager.CreateModal<EditThemeModal>();
            });

            CancelButton.AddOnClickedEvent(x => { DynamicUIManager.CreateModal<EditThemeModal>(); });
        }

        public override void Update() { }

        public override void Delete() { }
    }
}
