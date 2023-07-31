using System;
using SimObjects.MixAndMatch;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class SelectPartModal : ModalDynamic {
        private const float CONTENT_WIDTH  = 400;
        private const float CONTENT_HEIGHT = 110;

        private Action<MixAndMatchPartData> _callback;

        public SelectPartModal(Action<MixAndMatchPartData> callback)
            : base(new Vector2(CONTENT_WIDTH, CONTENT_HEIGHT)) {
            _callback = callback;
        }

        public override void Create() {
            Title.SetText($"Choose a Robot Part");

            string[] files = MixAndMatchSaveUtil.PartFiles;

            var dropdown =
                MainContent.CreateDropdown().ApplyTemplate(MixAndMatchModal.VerticalLayout).SetOptions(files);

            AcceptButton.AddOnClickedEvent(
                _ => {
                    if (files.Length == 0 || dropdown.Value < 0) // TODO: Disable select button
                        return;

                    _callback(MixAndMatchSaveUtil.LoadPartData(files[dropdown.Value]));
                    DynamicUIManager.CloseActiveModal();
                });
        }

        public override void Update() {}

        public override void Delete() {}
    }
}