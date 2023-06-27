using System;
using UnityEngine;
using Synthesis.Runtime;


namespace Synthesis.UI.Dynamic
{
    public class SpawnLocationPanel : PanelDynamic
    {
        private const float robotMoveSpeed = 5f;
        private const float robotTiltAmount = 0.2f;
        
        private const float width = 400f;
        private const float height = 150f;
        private const float spawnDistanceFromSurface = 0.05f;
        
        private int fieldLayerMask = 1 << LayerMask.NameToLayer("FieldCollisionLayer");

        private static readonly Color redColor = new Color(1, 0, 0, 0.5f);
        private static readonly Color blueColor = new Color(0, 0, 1, 0.5f);
        private static readonly Material mat = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));

        private Transform[] _spawnPositionDisplays = new Transform[6];

        public Func<Button, Button> DisabledTemplate = b =>
            b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
        
        private const float VERTICAL_PADDING = 15f;

        private Button[] buttons = new Button[6];
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
                _spawnPositionDisplays[i] = obj.transform;
            }

            Title.SetText("Set Spawn").SetFontSize(25f);
            PanelImage.RootGameObject.SetActive(false);

            Content panel = new Content(null, UnityObject, null);
            panel.SetBottomStretch<Content>(0, 0, 0);
            
            //float padding = Screen.width / 2f - width/2f;
            float padding = 700;
            panel.SetBottomStretch<Content>(padding, padding, 0);
            
            AcceptButton
                    .StepIntoLabel(label => label.SetText("Accept"))
                    .AddOnClickedEvent(b =>
                    {
                        DynamicUIManager.CreateModal<MatchModeModal>();
                    });
            CancelButton
                .StepIntoLabel(label => label.SetText("Revert"))
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

        public override void Update() {
            
            /*if ((SimulationRunner.HasContext(SimulationRunner.GIZMO_SIM_CONTEXT) || SimulationRunner.HasContext(SimulationRunner.PAUSED_SIM_CONTEXT))
                    && !matchStarted) {
                matchStarted = true;
                StartMatch();
            }*/

            // Find robot spawn position
            if (Input.GetMouseButton(1))
            {
                // Raycast out from camera to see where the mouse is pointing
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out var hit, 100, fieldLayerMask))
                {
                    Transform selectedPosition = _spawnPositionDisplays[_selectedButton];

                    Vector3 boxHalfSize =
                        selectedPosition.localScale / 2f;
                    
                    // Box cast down towards where the mouse is pointing to find the lowest suitable spawn position for the robot
                    if (UnityEngine.Physics.BoxCast(hit.point + Vector3.up * 20f, boxHalfSize, 
                            Vector3.down, out var boxHit, Quaternion.identity, 30f, fieldLayerMask))
                    {
                        MatchMode.RobotSpawnLocations[_selectedButton] = new Vector3(hit.point.x, 
                            boxHit.point.y + boxHalfSize.y + spawnDistanceFromSurface, hit.point.z);
                    }
                }
            }

            // Move robot objects toward thier spawn positions
            for (int i = 0; i < MatchMode.Robots.Count; i++)
            {
                //Transform trf = _spawnPositionDisplays[i];
                Transform trf = MatchMode.Robots[i].RobotNode.transform;

                Vector3 prevPos = trf.position;
                Vector3 target = MatchMode.RobotSpawnLocations[i];
                trf.position = Vector3.Lerp(prevPos, target, robotMoveSpeed * Time.deltaTime);

                Vector3 movementDirection = (target - prevPos);
                Vector3 robotTilt = (target - prevPos) * 45f * robotTiltAmount;
                trf.rotation = Quaternion.Euler(robotTilt.z, 0, -robotTilt.x);
            }
        }

        public override void Delete()
        {
            _spawnPositionDisplays.ForEach(x => UnityEngine.Object.Destroy(x.gameObject));
        }
    }
}