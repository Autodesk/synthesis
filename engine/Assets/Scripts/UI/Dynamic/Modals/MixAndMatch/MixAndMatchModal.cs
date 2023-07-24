using System;
using JetBrains.Annotations;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class MixAndMatchModal : ModalDynamic {
        private const float SPLIT_SPACING = 15;
        private const float CONTENT_WIDTH = 400;
        
        private const float CHOOSE_MODE_HEIGHT = 55;
        private const float SELECT_OBJECT_HEIGHT = 110;
        private const float CREATE_NEW_HEIGHT = 55;

        [CanBeNull] private string[] _robotFiles;
        [CanBeNull] private string[] _partFiles;

        public MixAndMatchModal() : base(new Vector2(CONTENT_WIDTH, CHOOSE_MODE_HEIGHT)) {}

        // TODO: after merge, remove this and use the one in dynamic UI components
        private Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public override void Create() {
            Description.RootGameObject.SetActive(false);
            CreateChoosePartOrRobotModal();
        }

        /// <summary>The screen where the user specifies if they will edit a robot or part</summary>
        private void CreateChoosePartOrRobotModal() {
            Title.SetText("Mix and Match Robot Editor");
            
            AcceptButton.RootGameObject.SetActive(false);

            var (left, right) = MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);
            
            var robotEditorButton = left.CreateButton("Robot Editor")
                .ApplyTemplate<Button>(VerticalLayout)
                .AddOnClickedEvent(_ => CreateChooseObjectModal(true));

            var partEditorButton = right.CreateButton("Part Editor")
                .ApplyTemplate<Button>(VerticalLayout)
                .AddOnClickedEvent(_ => CreateChooseObjectModal(false));
        }

        /// <summary>User either selects a part/robot file chooses to create a new one</summary>
        private void CreateChooseObjectModal(bool robot) {
            Title.SetText($"Choose a {(robot ? "Robot" : "Part")} to Edit");
            ClearAndResizeContent(new Vector2(MainContent.Size.x, SELECT_OBJECT_HEIGHT));

            if (robot) {
                //TODO: Load robot files
            }
            else {
                //TODO: Load part files
            }

            var dropdown = MainContent.CreateDropdown().ApplyTemplate(VerticalLayout);
            var (left, right) = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 50)).ApplyTemplate(VerticalLayout).SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);

            var loadSelected = left.CreateButton("Select").ApplyTemplate(VerticalLayout)
                .AddOnClickedEvent(_ => {
                    if (robot)
                        LoadRobot(_robotFiles[dropdown.Value]);
                    else LoadPart(_partFiles[dropdown.Value]);
                });
            var createNew = right.CreateButton("New").ApplyTemplate(VerticalLayout)
                .AddOnClickedEvent(_ => CreateNewObjectModal(robot));
        }

        /// <summary>User names a new part/robot</summary>
        private void CreateNewObjectModal(bool robot) {
            Title.SetText($"Create a New {(robot ? "Robot" : "Part")}");
            ClearAndResizeContent(new Vector2(MainContent.Size.x, CREATE_NEW_HEIGHT));

            var nameInputField = MainContent.CreateInputField().ApplyTemplate(VerticalLayout).StepIntoHint(h 
                => h.SetText("")).AddOnValueChangedEvent((i, v) => UpdateAcceptButton(v));
            
            AcceptButton.AddOnClickedEvent(_ => {
                if (robot)
                    LoadRobot(nameInputField.Value);
                else LoadPart(nameInputField.Value);
            }).RootGameObject.SetActive(true);

            void UpdateAcceptButton(string inputValue) {
                if (inputValue == "")
                    AcceptButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                        .DisableEvents<Button>();
                else
                    AcceptButton.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ACCEPT))
                        .EnableEvents<Button>();
            }
            UpdateAcceptButton("");
        }

        /// <summary>Either load a robot if it exists or create a new one if it doesn't</summary>
        private void LoadRobot(string fileName) {
            // TODO: create a static class to handle loading and saving robots and parts
            // TODO: after loading a robot, open the robot editor
            throw new NotImplementedException($"Load robot {fileName}");
        }

        /// <summary>Either load a part if it exists or create a new one if it doesn't</summary>
        private void LoadPart(string fileName) {
            // TODO: create a static class to handle loading and saving robots and parts
            // TODO: after loading a part, open the part editor
            throw new NotImplementedException($"Load part {fileName}");
        }

        public override void Delete() {}

        public override void Update() {}
    }
}