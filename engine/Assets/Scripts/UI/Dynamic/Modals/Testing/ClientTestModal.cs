using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using TMPro;
using UnityEngine;

namespace Synthesis.UI.Dynamic {

    public class ClientTestModal: ModalDynamic {

        private static ClientTestModal _self;

        private Label _statusLabel;

        public ClientTestModal(Vector2 mainContentSize): base(mainContentSize) { }

        public override void Create() {
            _statusLabel = MainContent.CreateLabel(30).SetTopStretch<Label>();
            _statusLabel.SetText("Initializing...");

            _self = this;
        }

        public override void Update() { }

        public override void Delete() {
            _statusLabel = null;
        }

        public static void TrySetStatus(string txt) {
            if (_self != null) {
                _self._statusLabel.SetText(txt);
            }
        }

    }

}
