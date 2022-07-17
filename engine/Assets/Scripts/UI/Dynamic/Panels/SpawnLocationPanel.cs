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

        private const float VERTICAL_PADDING = 10f;

        public Func<UIComponent, UIComponent> VerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
            return u;
        };

        public SpawnLocationPanel() : base(new Vector2(width, height)) { }

        public override void Create()
        {
            Title.SetText("Set Spawn").SetFontSize(30f);
            //PanelImage.RootGameObject.SetActive(false);
            //Description.RootGameObject.SetActive(false);

            Content panel = new Content(null, UnityObject, null);
            panel.SetBottomStretch<Content>(Screen.width / 2 - width / 2 - 40f, Screen.width / 2 - width / 2 - 40f, 0);

            AcceptButton
                    .StepIntoLabel(label => label.SetText("Start"))
                    .AddOnClickedEvent(b =>
                    {
                        StartMatch();
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
                .SetTopStretch<Button>()
                .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
                .StepIntoLabel(label => label.SetText("Set to Center"))
                .AddOnClickedEvent(b => { });

            MainContent.CreateButton()
                .ApplyTemplate(VerticalLayout)
                .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
                .StepIntoLabel(label => label.SetText("Set to Previous"))
                .AddOnClickedEvent(b => { });
        }
        private void StartMatch()
        {
            PracticeMode.SetInitialState(GizmoManager.currentGizmo.transform.parent.gameObject);
            Shooting.ConfigureGamepieces();

            DynamicUIManager.CreatePanel<Synthesis.UI.Dynamic.ScoreboardPanel>();
            GizmoManager.ExitGizmo();
        }

        public override void Update() { 
            if(GizmoManager.currentGizmo == null)
            {
                StartMatch();
            }
        }

        public override void Delete() { }
    }
}