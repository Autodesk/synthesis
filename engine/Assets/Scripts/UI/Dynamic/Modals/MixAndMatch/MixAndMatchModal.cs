using System;
using System.IO;
using System.Linq;
using SimObjects.MixAndMatch;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UI.Dynamic.Panels.MixAndMatch;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class MixAndMatchModal : ModalDynamic {
        private const float SPLIT_SPACING = 15;
        private const float CONTENT_WIDTH = 400;

        private const float CHOOSE_MODE_HEIGHT   = 55;
        private const float SELECT_OBJECT_HEIGHT = 110;
        private const float CREATE_NEW_HEIGHT    = 55;
        private const float DELETE_HEIGHT = 0;

        public MixAndMatchModal() : base(new Vector2(CONTENT_WIDTH, CHOOSE_MODE_HEIGHT)) {}

        // TODO: after merge, remove this and use the one in dynamic UI components
        public static Func<UIComponent, UIComponent> VerticalLayout = (u) => {
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
            CancelButton.StepIntoLabel(l => l.SetText("Close")).RootGameObject.SetActive(true);

            var (left, right) =
                MainContent.SplitLeftRight((MainContent.Size.x / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);

            left.CreateButton("Robot Editor") // Robot editor button
                .ApplyTemplate(VerticalLayout)
                .AddOnClickedEvent(
                    _ => CreateChooseObjectModal(true));

            right
                .CreateButton("Part Editor") // Part editor button
                .ApplyTemplate(VerticalLayout)
                .AddOnClickedEvent(
                    _ => CreateChooseObjectModal(false));
        }

        /// <summary>User either selects a part/robot file chooses to create a new one</summary>
        private void CreateChooseObjectModal(bool robot) {
            Title.SetText($"Choose a {(robot ? "Robot" : "Part")} to Edit");
            ClearAndResizeContent(new Vector2(MainContent.Size.x, SELECT_OBJECT_HEIGHT));
            
            AcceptButton.RootGameObject.SetActive(false);
            CancelButton.StepIntoLabel(l => l.SetText("Back")).AddOnClickedEvent(_ => CreateChoosePartOrRobotModal()).RootGameObject.SetActive(true);

            string[] files = robot ? MixAndMatchSaveUtil.RobotFiles : MixAndMatchSaveUtil.PartFiles;

            var dropdown = MainContent.CreateDropdown().ApplyTemplate(VerticalLayout).SetOptions(files);

            var (selectContent, right) = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 50))
                                    .ApplyTemplate(VerticalLayout)
                                    .SplitLeftRight((MainContent.Size.x / 3f) - (SPLIT_SPACING * 0.5f), SPLIT_SPACING);
            
            var (newContent, deleteContent) = right.SplitLeftRight((MainContent.Size.x / 3f) - (SPLIT_SPACING * 0.5f), SPLIT_SPACING);

            selectContent.CreateButton("Select") 
                .ApplyTemplate(VerticalLayout) 
                .AddOnClickedEvent(
                    _ => {
                        if (files.Length == 0 || dropdown.Value < 0) // TODO: Disable select button
                                                                     // when this condition is true
                            return;

                        if (robot)
                            OpenRobotEditor(MixAndMatchSaveUtil.LoadRobotData(files[dropdown.Value]));
                        else
                            OpenPartEditor(MixAndMatchSaveUtil.LoadPartData(files[dropdown.Value]));
                    });
            
            newContent.CreateButton("New") 
                .ApplyTemplate(VerticalLayout) 
                .AddOnClickedEvent(
                    _ => CreateNewObjectModal(robot));
            
            deleteContent.CreateButton("Delete") 
                .ApplyTemplate(VerticalLayout) 
                .AddOnClickedEvent(
                    _ => CreateDeleteObject(robot, files[dropdown.Value]));
        }

        /// <summary>User names a new part/robot</summary>
        private void CreateNewObjectModal(bool robot) {
            Title.SetText($"Create a New {(robot ? "Robot" : "Part")}");
            ClearAndResizeContent(new Vector2(MainContent.Size.x, CREATE_NEW_HEIGHT));

            var nameInputField = MainContent.CreateInputField()
                                     .ApplyTemplate(VerticalLayout)
                                     .StepIntoHint(h => h.SetText(""))
                                     .AddOnValueChangedEvent((_, v) => UpdateAcceptButton(v));

            AcceptButton
                .AddOnClickedEvent(
                    _ => {
                        if (robot)
                            OpenRobotEditor(MixAndMatchSaveUtil.CreateNewRobot(nameInputField.Value));
                        else
                            CreateChoosePartMirabufFile(nameInputField.Value);
                    })
                .RootGameObject.SetActive(true);
            
            CancelButton.StepIntoLabel(l => l.SetText("Back")).AddOnClickedEvent(_ => CreateChooseObjectModal(robot)).RootGameObject.SetActive(true);

            void UpdateAcceptButton(string inputValue) {
                if (inputValue == "")
                    AcceptButton
                        .StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                                           ColorManager.SynthesisColor.InteractiveElementRight))
                        .DisableEvents<Button>();
                else
                    AcceptButton.StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.BackgroundSecondary))
                        .EnableEvents<Button>();
            }

            UpdateAcceptButton("");
        }

        /// <summary>Prompt the user to delete a part</summary>
        private void CreateDeleteObject(bool robot, string fileName) {
            Title.SetText($"Delete {fileName}?");
            ClearAndResizeContent(new Vector2(MainContent.Size.x, DELETE_HEIGHT));
            
            AcceptButton.StepIntoLabel(l => l.SetText("Back")).AddOnClickedEvent(_ => CreateChoosePartOrRobotModal()).RootGameObject.SetActive(true);

            CancelButton.AddOnClickedEvent(_ => {
                if (robot)
                    MixAndMatchSaveUtil.DeleteRobot(fileName);
                else MixAndMatchSaveUtil.DeletePart(fileName);
            }).StepIntoLabel(l => l.SetText("Delete")).RootGameObject.SetActive(true);
        }

        /// <summary>User selects a mirabuf file to use for the new part</summary>
        private void CreateChoosePartMirabufFile(string fileName) {
            Title.SetText("Select a Base Part File");
            ClearAndResizeContent(new Vector2(MainContent.Size.x, CREATE_NEW_HEIGHT));
            
            CancelButton.StepIntoLabel(l => l.SetText("Back")).AddOnClickedEvent(_ => CreateChoosePartOrRobotModal()).RootGameObject.SetActive(true);

            string[] files = MixAndMatchSaveUtil.PartMirabufFiles;

            var dropdown = MainContent.CreateDropdown()
                               .ApplyTemplate(VerticalLayout)
                               .SetOptions(files.Select(Path.GetFileName).ToArray());

            AcceptButton.AddOnClickedEvent(
                _ => OpenPartEditor(MixAndMatchSaveUtil.CreateNewPart(fileName, files[dropdown.Value]))).StepIntoLabel(l => l.SetText("Select")).RootGameObject.SetActive(true);
        }

        /// <summary>Either load a robot if it exists or create a new one if it doesn't</summary>
        private void OpenRobotEditor(MixAndMatchRobotData robot) {
            DynamicUIManager.CloseActiveModal();
            DynamicUIManager.CreatePanel<RobotEditorPanel>(persistent: true, args: robot);
        }

        /// <summary>Either load a part if it exists or create a new one if it doesn't</summary>
        private void OpenPartEditor(MixAndMatchPartData part) {
            DynamicUIManager.CloseActiveModal();
            DynamicUIManager.CreatePanel<PartEditorPanel>(persistent: true, args: part);
        }

        public override void Delete() {}

        public override void Update() {}
    }
}