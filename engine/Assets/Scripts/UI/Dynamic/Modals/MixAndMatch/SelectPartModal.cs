using System;
using SimObjects.MixAndMatch;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    /// <summary>The modal for selecting a part to add when editing a robot</summary>
    public class SelectPartModal : ModalDynamic {
        private const float CONTENT_WIDTH  = 400;
        private const float CONTENT_HEIGHT = 55;

        private readonly Action<GlobalPartData> _callback;

        public SelectPartModal(Action<GlobalPartData> callback) : base(new Vector2(CONTENT_WIDTH, CONTENT_HEIGHT)) {
            _callback = callback;
        }

        public override void Create() {
            Title.SetText($"Choose a Robot Part");

            string[] files = MixAndMatchSaveUtil.PartFiles;

            var dropdown = MainContent.CreateDropdown().ApplyTemplate(UIComponent.VerticalLayout).SetOptions(files);

            AcceptButton
                .AddOnClickedEvent(
                    _ => {
                        if (files.Length == 0 || dropdown.Value < 0)
                            return;

                        _callback(MixAndMatchSaveUtil.LoadPartData(files[dropdown.Value]));
                        DynamicUIManager.CloseActiveModal();
                    })
                .StepIntoLabel(l => l.SetText("Add"));

            dropdown.AddOnValueChangedEvent((_, _, _) => UpdateSelectButton());

            void UpdateSelectButton() {
                AcceptButton.ApplyTemplate(
                    files.Length == 0 || dropdown.Value < 0 ? Button.DisableButton : Button.EnableAcceptButton);
            }
            UpdateSelectButton();
        }

        public override void Update() {}

        public override void Delete() {}
    }
}