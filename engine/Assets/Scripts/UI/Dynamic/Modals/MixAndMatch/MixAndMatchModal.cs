using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimObjects.MixAndMatch;
using Synthesis.UI.Dynamic;
using TMPro;
using UI.Dynamic.Panels.MixAndMatch;
using UnityEngine;
using UnityEngine.UI;
using Button = Synthesis.UI.Dynamic.Button;

namespace UI.Dynamic.Modals.MixAndMatch {
    public class MixAndMatchModal : ModalDynamic {
        private const float SPLIT_SPACING = 15;
        private const float CONTENT_WIDTH = 400;

        private const float CHOOSE_TYPE_HEIGHT   = 55;
        private const float SELECT_OBJECT_HEIGHT = 110;
        private const float CREATE_NEW_HEIGHT    = 55;
        private const float DELETE_HEIGHT        = 0;

        public MixAndMatchModal() : base(new Vector2(CONTENT_WIDTH, CHOOSE_TYPE_HEIGHT)) {}

        public override void Create() {
            Description.RootGameObject.SetActive(false);
            CreateChooseTypeModal();
        }

        /// <summary>The screen where the user specifies if they will edit a robot or part</summary>
        private void CreateChooseTypeModal() {
            ClearAndResizeContent(new Vector2(CONTENT_WIDTH, CHOOSE_TYPE_HEIGHT));
            Title.SetText("Mix and Match Robot Editor");

            AcceptButton.RootGameObject.SetActive(false);
            CancelButton.StepIntoLabel(l => l.SetText("Close"))
                .AddOnClickedEvent(
                    _ => DynamicUIManager.CloseActiveModal())
                .RootGameObject.SetActive(true);

            var (left, right) =
                MainContent.SplitLeftRight((CONTENT_WIDTH / 2f) - (SPLIT_SPACING / 2f), SPLIT_SPACING);

            left.CreateButton("Robot Editor") // Robot editor button
                .ApplyTemplate(UIComponent.VerticalLayout)
                .AddOnClickedEvent(
                    _ => CreateChooseObjectModal(true));

            right
                .CreateButton("Part Editor") // Part editor button
                .ApplyTemplate(UIComponent.VerticalLayout)
                .AddOnClickedEvent(
                    _ => CreateChooseObjectModal(false));
        }

        /// <summary>User either selects a part/robot file chooses to create a new one</summary>
        private void CreateChooseObjectModal(bool robot) {
            Title.SetText($"Choose a {(robot ? "Robot" : "Part")} to Edit");
            ClearAndResizeContent(new Vector2(CONTENT_WIDTH, SELECT_OBJECT_HEIGHT));

            AcceptButton.RootGameObject.SetActive(false);
            CancelButton.StepIntoLabel(l => l.SetText("Back"))
                .AddOnClickedEvent(
                    _ => CreateChooseTypeModal())
                .RootGameObject.SetActive(true);

            string[] files = robot ? MixAndMatchSaveUtil.RobotFiles : MixAndMatchSaveUtil.PartFiles;

            var dropdown = MainContent.CreateDropdown().ApplyTemplate(UIComponent.VerticalLayout).SetOptions(files);

            var (selectContent, right) =
                MainContent.CreateSubContent(new Vector2(CONTENT_WIDTH, 50))
                    .ApplyTemplate(UIComponent.VerticalLayout)
                    .SplitLeftRight((CONTENT_WIDTH / 3f) - (SPLIT_SPACING * 0.5f), SPLIT_SPACING);

            var (newContent, deleteContent) =
                right.SplitLeftRight((CONTENT_WIDTH / 3f) - (SPLIT_SPACING * 0.5f), SPLIT_SPACING);

            var selectButton =
                selectContent.CreateButton("Select")
                    .ApplyTemplate(UIComponent.VerticalLayout)
                    .AddOnClickedEvent(
                        _ => {
                            if (files.Length == 0 || dropdown.Value < 0)
                                return;

                            if (robot)
                                OpenRobotEditor(MixAndMatchSaveUtil.LoadRobotData(files[dropdown.Value]));
                            else
                                OpenPartEditor(MixAndMatchSaveUtil.LoadPartData(files[dropdown.Value]));
                        });

            newContent.CreateButton("New")
                .ApplyTemplate(UIComponent.VerticalLayout)
                .AddOnClickedEvent(
                    _ => CreateNewObjectModal(robot));

            var deleteButton = deleteContent.CreateButton("Delete")
                                   .ApplyTemplate(UIComponent.VerticalLayout)
                                   .AddOnClickedEvent(
                                       _ => CreateDeleteObjectModal(robot, files[dropdown.Value]));

            void UpdateButtons() {
                bool itemSelected = files.Length == 0 || dropdown.Value < 0;

                selectButton.ApplyTemplate(itemSelected ? Button.DisableButton : Button.EnableButton);
                deleteButton.ApplyTemplate(itemSelected ? Button.DisableButton : Button.EnableDeleteButton);
            }

            UpdateButtons();

            dropdown.AddOnValueChangedEvent((_, _, _) => UpdateButtons());
        }

        /// <summary>User names a new part/robot</summary>
        private void CreateNewObjectModal(bool robot) {
            Title.SetText($"Create a New {(robot ? "Robot" : "Part")}");
            ClearAndResizeContent(new Vector2(CONTENT_WIDTH, CREATE_NEW_HEIGHT));

            string[] files = robot ? MixAndMatchSaveUtil.RobotFiles : MixAndMatchSaveUtil.PartFiles;

            var nameInputField = MainContent.CreateInputField()
                                     .ApplyTemplate(UIComponent.VerticalLayout)
                                     .StepIntoHint(h => h.SetText(""))
                                     .AddOnValueChangedEvent((_, v) => UpdateAcceptButton(v));

            AcceptButton
                .AddOnClickedEvent(
                    _ => {
                        if (robot)
                            OpenRobotEditor(MixAndMatchSaveUtil.CreateNewRobot(nameInputField.Value));
                        else
                            CreateChoosePartFileModal(nameInputField.Value);
                    }).StepIntoLabel(l => l.SetText("Create")).RootGameObject.SetActive(true);

            CancelButton.StepIntoLabel(l => l.SetText("Back"))
                .AddOnClickedEvent(
                    _ => CreateChooseObjectModal(robot))
                .RootGameObject.SetActive(true);

            void UpdateAcceptButton(string inputValue) {
                AcceptButton.ApplyTemplate(
                    inputValue == "" || files.Contains(inputValue) ? Button.DisableButton : Button.EnableAcceptButton);
            }
            UpdateAcceptButton("");
        }

        /// <summary>Prompt the user to delete something</summary>
        private void CreateDeleteObjectModal(bool robot, string fileName) {
            Title.SetText($"Delete {fileName}?");

            // A list of robots that depend on the part
            List<string> dependencies = new();
            if (!robot) {
                foreach (var robotName in MixAndMatchSaveUtil.RobotFiles) {
                    foreach (var part in MixAndMatchSaveUtil.LoadRobotData(robotName).PartData) {
                        if (part.fileName == fileName) {
                            dependencies.Add(robotName);
                            break;
                        }
                    }
                }
            }

            ClearAndResizeContent(new Vector2(CONTENT_WIDTH,
                DELETE_HEIGHT + (dependencies.Count > 0 ? 150 : 0)));

            if (dependencies.Count > 0) {

                var scrollView = MainContent.CreateScrollView().SetHeight<ScrollView>(150);
                var textContent = scrollView.Content.CreateSubContent(new Vector2(CONTENT_WIDTH, 150))
                    .SetTopStretch<Content>(leftPadding: 15);
                
                textContent.CreateLabel().SetFontSize(20).SetText($"Unable to delete! Dependencies:").SetTopStretch<Label>(anchoredY: 15).SetFontStyle(FontStyles.Bold);
                dependencies.ForEach(d => textContent.CreateLabel().SetFontSize(17).SetText(d).ApplyTemplate(UIComponent.VerticalLayoutBigSpacing));
                    
                CancelButton.RootGameObject.SetActive(false);
            }
            else {
                CancelButton.StepIntoLabel(l => l.SetText("Delete"))
                    .AddOnClickedEvent(
                        _ => {
                            if (robot)
                                MixAndMatchSaveUtil.DeleteRobot(fileName);
                            else
                                MixAndMatchSaveUtil.DeletePart(fileName);
                            CreateChooseObjectModal(robot);
                        })
                    .RootGameObject.SetActive(true);
            }

            AcceptButton
                .AddOnClickedEvent(
                    _ => CreateChooseObjectModal(robot))
                .StepIntoLabel(l => l.SetText("Back"))
                .RootGameObject.SetActive(true);
        }

        /// <summary>User selects a mirabuf file to use for the new part</summary>
        private void CreateChoosePartFileModal(string fileName) {
            Title.SetText("Select a Base Part File");
            ClearAndResizeContent(new Vector2(CONTENT_WIDTH, CREATE_NEW_HEIGHT));

            CancelButton.StepIntoLabel(l => l.SetText("Back"))
                .AddOnClickedEvent(
                    _ => CreateNewObjectModal(false))
                .RootGameObject.SetActive(true);

            string[] files = MixAndMatchSaveUtil.PartMirabufFiles;

            var dropdown = MainContent.CreateDropdown()
                               .ApplyTemplate(UIComponent.VerticalLayout)
                               .SetOptions(files.Select(Path.GetFileName).ToArray());

            AcceptButton
                .AddOnClickedEvent(
                    _ => OpenPartEditor(MixAndMatchSaveUtil.CreateNewPart(fileName, files[dropdown.Value])))
                .StepIntoLabel(l => l.SetText("Select"))
                .RootGameObject.SetActive(true);
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