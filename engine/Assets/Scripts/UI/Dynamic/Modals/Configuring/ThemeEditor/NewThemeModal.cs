using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using UI.Dynamic.Modals.Configuring.ThemeEditor;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring
{
    public class NewThemeModal : ModalDynamic
    {
        private const float MODAL_WIDTH = 300;
        private const float MODAL_HEIGHT = 50;
        
        public NewThemeModal()
            : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        private string _newThemeName = null;

        public override void Create() {
            Title.SetText("New Theme");
            Description.SetText("Create a Custom Theme");

            var inputField = MainContent
                .CreateInputField()
                .StepIntoHint(h => h.SetText("Theme Name"))
                .AddOnValueChangedEvent((fieldRef, value) =>
                {
                    _newThemeName = value;
                })
                .SetTopStretch<Dropdown>();

            AcceptButton.AddOnClickedEvent(x =>
            {
                PreferenceManager.SetPreference(ColorManager.SELECTED_THEME_PREF, _newThemeName);
                ColorManager.SelectedTheme = _newThemeName;
                
                DynamicUIManager.CreateModal<EditThemeModal>();
            });

            CancelButton.AddOnClickedEvent(x => { DynamicUIManager.CreateModal<EditThemeModal>(); });
        }

        public override void Update() { }

        public override void Delete() { }
    }
}
