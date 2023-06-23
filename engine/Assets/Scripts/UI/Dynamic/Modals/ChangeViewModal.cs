using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Synthesis.UI.Dynamic {
    public class ChangeViewModal : ModalDynamic {
        private string _selectedView = "Orbit";
        private string[] _viewOptions;

        public ChangeViewModal() : base(new Vector2(400, 40)) {}

        public override void Create() {
            Title.SetText("Change View");
            Description.SetText("Change the current camera view");

            CameraController controller = Camera.main.GetComponent<CameraController>();

            if (_viewOptions == null) {
                _viewOptions = CameraController.CameraModes.Keys.ToArray();
            }

            int selectedIndex = 0;
            _selectedView     = CameraController.CameraModes
                                .FirstOrDefault(mode => {
                                    if (mode.Value == controller.CameraMode) {
                                        selectedIndex = _viewOptions.ToList().IndexOf(mode.Key);
                                        return true;
                                    }
                                    return false;
                                })
                                .Key;

            var viewDropdown = MainContent.CreateDropdown()
                                   .SetOptions(CameraController.CameraModes.Keys.ToArray())
                                   .SetValue(selectedIndex)
                                   .AddOnValueChangedEvent((d, i, option) => _selectedView = option.text)
                                   .SetTopStretch<Dropdown>();

            AcceptButton.AddOnClickedEvent(b => {
                controller.CameraMode = CameraController.CameraModes[_selectedView];
                DynamicUIManager.CloseActiveModal();
            });
        }

        public override void Update() {}

        public override void Delete() {}
    }
}