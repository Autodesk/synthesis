using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace Synthesis.UI.Dynamic
{
    public class SpawnLocationPanel : PanelDynamic
    {

        
        private static float width = 200f;
        private static float height = 200f;

        private const float VERTICAL_PADDING = 25f;

        public Func<UIComponent, UIComponent> VerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
            return u;
        };

        public SpawnLocationPanel() : base(new Vector2(width, height)) { }

        Label location;

        public override void Create()
        {
            Title.SetText("Set Spawn").SetFontSize(25f);
            //PanelImage.RootGameObject.SetActive(false);
            //Description.RootGameObject.SetActive(false);

            Content panel = new Content(null, UnityObject, null);
            panel.SetBottomStretch<Content>(Screen.width / 2 - width / 2 - 40f, Screen.width / 2 - width / 2 - 40f, 0);

            AcceptButton
                    .StepIntoLabel(label => label.SetText("Start"))
                    .AddOnClickedEvent(b =>
                    {
                        if (!matchStarted)
                        {
                            matchStarted = true;
                            StartMatch();

                        }
                    });
            CancelButton
                .StepIntoLabel(label => label.SetText("Cancel"))
                .AddOnClickedEvent(b =>
                {
                    //if (FieldSimObject.CurrentField != null) FieldSimObject.CurrentField.DeleteField();
                    //if (RobotSimObject.GetCurrentlyPossessedRobot() != null) RobotSimObject.GetCurrentlyPossessedRobot().Destroy();
                    DynamicUIManager.CreateModal<MatchModeModal>();
                });

            /*MainContent.CreateLabel(50f).ApplyTemplate(VerticalLayout).SetText("Spawn Positions");
            var spawnPosition = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
                .SetOptions(new string[] { "Left", "Middle", "Right" })
                .AddOnValueChangedEvent((d, i, data) => { }
                //ADD: CHANGE SPAWN POSITION
                ).ApplyTemplate(VerticalLayout);*/

            


            MainContent.CreateButton()
                .ApplyTemplate(VerticalLayout)
                .SetTopStretch<Button>()
                .SetHeight<Button>(30f)
                .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
                .StepIntoLabel(label => label.SetText("Set to Center").SetFontSize(20f))
                .AddOnClickedEvent(b => {

                    if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                    {
                        RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position = new Vector3(0f, 0f, 0f);
                        Camera.main.GetComponent<CameraController>().FocusPoint = () => RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position;
                    }
                });

            MainContent.CreateButton()
                .ApplyTemplate(VerticalLayout)
                .SetHeight<Button>(30f)
                .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
                .StepIntoLabel(label => label.SetText("Set to Previous").SetFontSize(20f))
                .AddOnClickedEvent(b => {
                    if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                    {
                        PreferenceManager.PreferenceManager.Load();
                        if (PreferenceManager.PreferenceManager.ContainsPreference(MatchMode.PREVIOUS_SPAWN_LOCATION))
                        {

                            RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position = 
                                PreferenceManager.PreferenceManager.GetPreference<Vector3>(MatchMode.PREVIOUS_SPAWN_LOCATION);
                            RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.rotation =
                                PreferenceManager.PreferenceManager.GetPreference<Quaternion>(MatchMode.PREVIOUS_SPAWN_ROTATION);
                        }
                        else
                        {
                            RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position = new Vector3(0f, 0f, 0f);
                        }
                        Camera.main.GetComponent<CameraController>().FocusPoint = () => RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position;
                    }
                });


            location = MainContent.CreateLabel(30f).ApplyTemplate(VerticalLayout).SetFontSize(30)
                .SetHorizontalAlignment(TMPro.HorizontalAlignmentOptions.Center).SetVerticalAlignment(TMPro.VerticalAlignmentOptions.Bottom)
                .SetTopStretch(leftPadding: 10f, anchoredY: 120f).SetText("(0.00, 0.00, 0.00)");
        }
        private void StartMatch()
        {
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            {
                PreferenceManager.PreferenceManager.SetPreference(MatchMode.PREVIOUS_SPAWN_LOCATION, RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position);
                PreferenceManager.PreferenceManager.SetPreference(MatchMode.PREVIOUS_SPAWN_ROTATION, RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.rotation);
                PreferenceManager.PreferenceManager.Save();
            }
            PracticeMode.SetInitialState(GizmoManager.currentGizmo.transform.parent.gameObject);
            
            Shooting.ConfigureGamepieces();
            DynamicUIManager.CloseActivePanel();
            DynamicUIManager.CreatePanel<Synthesis.UI.Dynamic.ScoreboardPanel>();

            GizmoManager.ExitGizmo();
        }

        private bool matchStarted = false; 
        public override void Update() {

            Vector3 robotPosition = new Vector3();
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            {
                robotPosition = RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position;
            }

            location.SetText(
                $"({String.Format("{0:0.00}", robotPosition.x)}, {String.Format("{0:0.00}", robotPosition.y)}, {String.Format("{0:0.00}", robotPosition.z)})");


            if (GizmoManager.currentGizmo == null && !matchStarted)
            {
                matchStarted = true;
                StartMatch();
            }
        }

        public override void Delete() { }
    }
}