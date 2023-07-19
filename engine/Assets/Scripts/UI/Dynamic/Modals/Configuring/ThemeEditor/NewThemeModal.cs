using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using UI.Dynamic.Modals.Configuring.ThemeEditor;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring {
    public class NewThemeModal : ModalDynamic {
        private const float MODAL_WIDTH  = 300;
        private const float MODAL_HEIGHT = 55;

        public NewThemeModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        private string _newThemeName = null;

        public override void Create() {
            Title.SetText("New Theme");

            AcceptButton
                .AddOnClickedEvent(x => {
                    PreferenceManager.SetPreference(ColorManager.SELECTED_THEME_PREF, _newThemeName);
                    ColorManager.SelectedTheme = _newThemeName;
                    DynamicUIManager.CreateModal<EditThemeModal>();
                })
                .DisableEvents<Button>()
                .SetBackgroundColor<Button>(ColorManager.SynthesisColor.BackgroundSecondary);

            CancelButton.AddOnClickedEvent(x => { DynamicUIManager.CreateModal<EditThemeModal>(); });

            var inputField = MainContent.CreateInputField()
                                 .StepIntoHint(h => h.SetText("Theme Name"))
                                 .AddOnValueChangedEvent((fieldRef, value) => {
                                     if (value is "Default" or "")
                                         AcceptButton.DisableEvents<Button>().SetBackgroundColor<Button>(
                                             ColorManager.SynthesisColor.BackgroundSecondary);
                                     else
                                         AcceptButton.EnableEvents<Button>().SetBackgroundColor<Button>(
                                             ColorManager.SynthesisColor.AcceptButton);
                                     _newThemeName = value;
                                 })
                                 .SetCharacterLimit(20)
                                 .SetTopStretch<InputField>();
        }

        public override void Update() {}

        public override void Delete() {}
    }
}
