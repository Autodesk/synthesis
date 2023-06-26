using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using Synthesis.PreferenceManager;
using UnityEngine;
using Synthesis.Gizmo;
using Synthesis.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Synthesis.UI.Dynamic
{
    public class SpawnLocationPanel : PanelDynamic
    {
        private const float width = 400f;
        private const float height = 150f;
        private const float spawnDistanceFromSurface = 1f;

        private static readonly Color redColor = new Color(1, 0, 0, 0.5f);
        private static readonly Color blueColor = new Color(0, 0, 1, 0.5f);
        private static readonly Material mat = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));

        public Func<Button, Button> DisabledTemplate = b =>
            b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
        
        private const float VERTICAL_PADDING = 15f;

        private Button[] buttons = new Button[6];
        private Transform[] _positions = new Transform[6];
        private int _selectedButton;

        public Func<UIComponent, UIComponent> VerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
            return u;
        };

        public SpawnLocationPanel() : base(new Vector2(width, height)) { }
        
        public override bool Create()
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                var rend = obj.GetComponent<Renderer>();
                rend.material = mat;
                rend.material.SetColor("Color_48545d7793c14f3d9e1dd2264f072068", (i < 3) ? redColor : blueColor);
                //rend.material.color = (i < 3) ? redColor : blueColor;

                obj.GetComponent<Collider>().isTrigger = true;
                _positions[i] = obj.transform;
            }

            Title.SetText("Set Spawn").SetFontSize(25f);
            PanelImage.RootGameObject.SetActive(false);

            Content panel = new Content(null, UnityObject, null);
            panel.SetBottomStretch<Content>(0, 0, 0);
            
            //float padding = Screen.width / 2f - width/2f;
            float padding = 700;
            panel.SetBottomStretch<Content>(padding, padding, 0);
            
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
                    DynamicUIManager.CreateModal<MatchModeModal>();
                });
            
            float spacing = 15f;
            var (left, rightSection) = MainContent.SplitLeftRight((MainContent.Size.x / 3f) - (spacing / 2f), spacing);
            var (center, right) = rightSection.SplitLeftRight((MainContent.Size.x / 3f) - (spacing / 2f), spacing);
            
            buttons[0] = left.CreateButton()
                .StepIntoLabel(l => l.SetText("Red 1"))
                .ApplyTemplate(VerticalLayout)
                .ApplyTemplate(DisabledTemplate)
                .AddOnClickedEvent(b =>
                {
                    selectButton(0);
                });
            buttons[1] = center.CreateButton()
                .StepIntoLabel(l => l.SetText("Red 2"))
                .ApplyTemplate(VerticalLayout)
                .ApplyTemplate(DisabledTemplate)
                .AddOnClickedEvent(b =>
                {
                    selectButton(1);
                });
            buttons[2] = right.CreateButton()
                .StepIntoLabel(l => l.SetText("Red 3"))
                .ApplyTemplate(VerticalLayout)
                .ApplyTemplate(DisabledTemplate)
                .AddOnClickedEvent(b =>
                {
                    selectButton(2);
                });
            buttons[3] = left.CreateButton()
                .StepIntoLabel(l => l.SetText("Blue 1"))
                .ApplyTemplate(VerticalLayout)
                .ApplyTemplate(DisabledTemplate)
                .AddOnClickedEvent(b =>
                {
                    selectButton(3);

                });
            buttons[4] = center.CreateButton()
                .StepIntoLabel(l => l.SetText("Blue 2"))
                .ApplyTemplate(VerticalLayout)
                .ApplyTemplate(DisabledTemplate)
                .AddOnClickedEvent(b =>
                {
                    selectButton(4);
                });
            buttons[5] = right.CreateButton()
                .StepIntoLabel(l => l.SetText("Blue 3"))
                .ApplyTemplate(VerticalLayout)
                .ApplyTemplate(DisabledTemplate)
                .AddOnClickedEvent(b =>
                {
                    selectButton(5);
                });
            
            selectButton(0);
            return true;
        }

        private void selectButton(int index)
        {
            Debug.Log(index);
            buttons[_selectedButton].Image.Color = ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT);
            _selectedButton = index;

            if (index < 3)
            {
                buttons[index].Image.Color = new Color(0.7f, 0, 0);
            }
            else
                buttons[index].Image.Color = new Color(0, 0, 0.7f);
        }

        private void deselectAllButtons()
        {
            buttons.ForEach(x =>
            {
                x.ApplyTemplate(DisabledTemplate);
            });
        }
        
        private void StartMatch() {
            /*if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
            {
                Vector3 p = RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position;
                PreferenceManager.PreferenceManager.SetPreference(MatchMode.PREVIOUS_SPAWN_LOCATION, new float[] { p.x, p.y, p.z});
                Quaternion q = RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.rotation;
                PreferenceManager.PreferenceManager.SetPreference(MatchMode.PREVIOUS_SPAWN_ROTATION, new float[] { q.x, q.y, q.z, q.w });
                PreferenceManager.PreferenceManager.Save();
            }
            
            // Shooting.ConfigureGamepieces();
            
            //TEMPORARY: FOR POWERUP ONLY
            
            Scoring.CreatePowerupScoreZones();
            DynamicUIManager.CloseAllPanels(true);
            DynamicUIManager.CreatePanel<Synthesis.UI.Dynamic.ScoreboardPanel>(true);

            GizmoManager.ExitGizmo();*/
        }

        private bool matchStarted = false;
        public override void Update() {
            
            if ((SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT) || SimulationRunner.HasContext(SimulationRunner.PAUSED_SIM_CONTEXT))
                    && !matchStarted) {
                matchStarted = true;
                StartMatch();
            }

            if (Input.GetMouseButton(1))
            { 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out var hit, 100,
                        1 << LayerMask.NameToLayer("FieldCollisionLayer")))
                {
                    Debug.Log(hit.point); 
                    //RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position = hit.point;
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    //Debug.DrawRay(Camera.main.transform.position, hit.point);

                    Vector3 position = hit.point + hit.normal * spawnDistanceFromSurface;
                    _positions[_selectedButton].position = position;
                }
            }
        }

        public override void Delete() { }
    }
}