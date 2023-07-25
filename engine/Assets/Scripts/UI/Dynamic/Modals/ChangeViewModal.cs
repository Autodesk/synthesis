using System.Linq;
using UI.Dynamic.Panels.Tooltip;
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

            AcceptButton.AddOnClickedEvent(
                _ => {
                    controller.CameraMode = CameraController.CameraModes[_selectedView];
                    DynamicUIManager.CloseActiveModal();

                    switch (_selectedView) {
                        case "Orbit":
                            TooltipManager.CreateTooltip(("LM", "Orbit Cam"), ("Scroll", "Zoom Cam"));
                            break;
                        case "Freecam":
                            TooltipManager.CreateTooltip(
                                ("RM", "Rotate Cam"), ("RM + WASD", "Move Cam"), ("Scroll", "Zoom Cam"));
                            break;
                        case "Overview":
                            break;
                        case "Driver Station":
                            TooltipManager.CreateTooltip(("RM + WASD", "Move Cam"), ("Scroll", "Zoom Cam"));
                            break;
                    }
                });
        }

        public override void Update() {}

        public override void Delete() {}
    }
}