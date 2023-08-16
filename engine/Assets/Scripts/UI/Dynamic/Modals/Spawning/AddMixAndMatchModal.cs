using System.IO;
using System.Linq;
using SimObjects.MixAndMatch;
using SynthesisAPI.Utilities;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Utilities.ColorManager;
using Logger = SynthesisAPI.Utilities.Logger;

namespace UI.Dynamic.Modals.Spawning {
    public class AddMixAndMatchModal : ModalDynamic {
        private int _selectedIndex = -1;
        private string[] _files;

        public AddMixAndMatchModal() : base(new Vector2(400, 55)) {}

        public override void Create() {
            _files = MixAndMatchSaveUtil.RobotFiles;

            Title.SetText("Mix And Match Selection");

            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("plus"))
                .SetColor(ColorManager.SynthesisColor.MainText);

            CancelButton.StepIntoLabel(l => l.SetText("Back"))
                .AddOnClickedEvent(
                    _ => DynamicUIManager.CreateModal<ChooseRobotTypeModal>());

            AcceptButton.StepIntoLabel(label => label.SetText("Load")).AddOnClickedEvent(_ => {
                if (_selectedIndex != -1) {
                    RobotSimObject.SpawnRobot(MixAndMatchSaveUtil.LoadRobotData(_files[_selectedIndex]));

                    DynamicUIManager.CloseActiveModal();
                    RobotSimObject.GetCurrentlyPossessedRobot()?.CreateDrivetrainTooltip();
                }
            });
            var chooseRobotDropdown = MainContent.CreateDropdown()
                                          .SetOptions(_files.Select(Path.GetFileNameWithoutExtension).ToArray())
                                          .AddOnValueChangedEvent((_, i, _) => _selectedIndex = i)
                                          .SetTopStretch<Dropdown>();

            if (_files.Length == 0) {
                chooseRobotDropdown.ApplyTemplate(Dropdown.DisableDropdown);
                AcceptButton.RootGameObject.SetActive(false);
            }

            _selectedIndex = _files.Length > 0 ? 0 : -1;
        }

        public override void Update() {
            if (RobotSimObject.SpawnedRobots.Count >= RobotSimObject.MAX_ROBOTS) {
                Logger.Log("Maximum number of bots reached", LogLevel.Info);
                DynamicUIManager.CloseActiveModal();
            }
        }

        public override void Delete() {}
    }
}