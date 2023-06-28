using System;
using UnityEngine;
using Synthesis.Runtime;
using UnityEngine.EventSystems;


namespace Synthesis.UI.Dynamic
{
    public class SpawnLocationPanel : PanelDynamic
    {
        private const float WIDTH = 400f;
        private const float HEIGHT = 150f;
        private const float VERTICAL_PADDING = 15f;

        private const float robotMoveSpeed = 7f;
        private const float robotTiltAmount = 0.32f;

        private const float rotateRobotSpeed = 450f;
        
        private const float spawnDistanceFromSurface = 0.1524f;
        
        private static readonly Color redBoxColor = new Color(1, 0, 0, 0.5f);
        private static readonly Color blueBoxColor = new Color(0, 0, 1, 0.5f);

        private static readonly Color redButtonColor = new Color(0.7f, 0, 0);
        private static readonly Color blueButtonColor = new Color(0, 0, 0.7f);

        private static readonly Material mat = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));
        private static readonly int shaderColorProperty = Shader.PropertyToID("Color_48545d7793c14f3d9e1dd2264f072068");

        private static readonly int fieldLayerMask = 1 << LayerMask.NameToLayer("FieldCollisionLayer");
        
        private readonly Button[] buttons = new Button[6];
        private readonly Transform[] _robotHighlights = new Transform[6];
        private readonly Vector3[] _robotOffsets = new Vector3[6];

        private readonly Func<Button, Button> DisabledTemplate = b =>
            b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));
        

        public readonly Func<UIComponent, UIComponent> VerticalLayout = (u) =>
        {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
            return u;
        };
        
        private int _selectedButton;
        private bool _renderBoxes = false;
        
        public SpawnLocationPanel() : base(new Vector2(WIDTH, HEIGHT)) { }
        
        public override bool Create()
        {
            Title.SetText("Set Spawn Locations").SetFontSize(25f);
            PanelImage.RootGameObject.SetActive(false);

            Content panel = new Content(null, UnityObject, null);
            
            float padding = 700;
            panel.SetBottomStretch<Content>(padding, padding, 0);
            
            AcceptButton
                    .StepIntoLabel(label => label.SetText("Accept"))
                    .AddOnClickedEvent(b => {
                        DynamicUIManager.ClosePanel<SpawnLocationPanel>();
                        MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.Auto);
                    });
            CancelButton
                .StepIntoLabel(label => label.SetText("Revert"))
                .AddOnClickedEvent(b => { });
            
            CreateRobotHighlights();
            CreateButtons();

            SelectButton(0);
            return true;
        }
        
        /*private void StartMatch() {
           if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
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

           GizmoManager.ExitGizmo();
       }*/

        public override void Update() {
            FindSpawnPosition();
            RotateRobot();
            MoveRobots();
        }
        
        public override void Delete()
        {
            _robotHighlights.ForEach(x => UnityEngine.Object.Destroy(x.gameObject));
        }

        /// <summary>
        /// Creates the robot highlight gameobjects and configures them
        /// </summary>
        private void CreateRobotHighlights()
        {
            for (int i = 0; i < 6; i++) 
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                
                if (!_renderBoxes)
                    obj.SetActive(false);
                
                var rend = obj.GetComponent<Renderer>();
                rend.material = mat;
                rend.material.SetColor(shaderColorProperty, (i < 3) ? redBoxColor : blueBoxColor);

                obj.GetComponent<Collider>().isTrigger = true;

                RobotSimObject simObject = MatchMode.Robots[i];
                if (simObject != null)
                {
                    obj.transform.localScale = MatchMode.Robots[i].RobotBounds.size;
                    _robotOffsets[i] =
                        simObject.RobotNode.transform.localToWorldMatrix.MultiplyPoint(simObject.RobotBounds
                            .center) + Vector3.down*0.496f;
                }

                _robotHighlights[i] = obj.transform;
            }
        }

        /// <summary>
        /// Creates the buttons to select which robot to move
        /// </summary>
        private void CreateButtons()
        {
            float spacing = 15f;
            var (left, rightSection) = MainContent.SplitLeftRight((MainContent.Size.x / 3f)
                                                                  - (spacing / 2f), spacing);
            
            var (center, right) = rightSection.SplitLeftRight((MainContent.Size.x / 3f) 
                                                              - (spacing / 2f), spacing);
            for (int i = 0; i < 6; i++)
            {
                int buttonIndex = i;
                buttons[i] =
                    ((i % 3 == 0) ? left : ((i % 3 == 1) ? center : right)).CreateButton()
                    .StepIntoLabel(l => l.SetText($"{((buttonIndex < 3) ? "Red" : "Blue")} {(buttonIndex % 3 + 1)}"))
                    .ApplyTemplate(VerticalLayout)
                    .ApplyTemplate(DisabledTemplate)
                    .AddOnClickedEvent(b =>
                    {
                        SelectButton(buttonIndex);
                    });
            }
        }

        /// <summary>
        /// Sets which button is currently selected and updates button colors
        /// </summary>
        /// <param name="index">the selected buttons index</param>
        private void SelectButton(int index)
        {
            buttons[_selectedButton].Image.Color = ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT);
            _selectedButton = index;

            buttons[index].Image.Color = (index < 3) ? redButtonColor : blueButtonColor;
        }

       

        /// <summary>
        /// Sets the selected robots spawn position based on where the mouse is pointing
        /// </summary>
        private void FindSpawnPosition()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0))
            {
                // Raycast out from camera to see where the mouse is pointing
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out var hit, 100, fieldLayerMask))
                {
                    Transform selectedPosition = _robotHighlights[_selectedButton];

                    Vector3 boxHalfSize =
                        selectedPosition.localScale / 2f;
                    
                    // Box cast down towards where the mouse is pointing to find the lowest suitable spawn position for the robot
                    if (UnityEngine.Physics.BoxCast(hit.point + Vector3.up * 20f, boxHalfSize, 
                            Vector3.down, out var boxHit,
                            MatchMode.RobotSpawnLocations[_selectedButton].rotation, 30f, fieldLayerMask))
                    {
                        MatchMode.RobotSpawnLocations[_selectedButton].position = new Vector3(hit.point.x,
                            boxHit.point.y + boxHalfSize.y + spawnDistanceFromSurface, hit.point.z);
                    }
                }
            }
        }

        private void RotateRobot()
        {
            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001f)
            {
                //var rotation = MatchMode.RobotSpawnLocations[_selectedButton].rotation;
                MatchMode.RobotSpawnLocations[_selectedButton].rotation.eulerAngles += Vector3.up 
                    * (Input.mouseScrollDelta.y * rotateRobotSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Smoothly lerps all robot objects towards their spawn location and tilts them in the direction they are moving
        /// </summary>
        private void MoveRobots()
        {
            for (int i = 0; i < 6; i++)
            {
                if (MatchMode.Robots[i] == null)
                    continue;
                
                var robot = MatchMode.Robots[i].RobotNode.transform;
                var box = _robotHighlights[i];

                Vector3 prevPos = box.position;
                Vector3 target = MatchMode.RobotSpawnLocations[i].position;
                box.position = Vector3.Lerp(prevPos, target, robotMoveSpeed * Time.deltaTime);

                Vector3 robotTilt = (target - prevPos) * (45f * robotTiltAmount);
                //box.rotation = Quaternion.Euler(robotTilt.z, MatchMode.RobotSpawnLocations[i].rotation.eulerAngles.y, -robotTilt.x);
                
                //Quaternion rotation 

                Quaternion robotYaw = MatchMode.RobotSpawnLocations[i].rotation;
                
                box.rotation = Quaternion.Euler(robotTilt.z, 0, -robotTilt.x) 
                               * (MatchMode.RobotSpawnLocations[i].rotation);

                RobotSimObject simObject = MatchMode.Robots[i];
                
                robot.rotation = Quaternion.identity;
                robot.position = Vector3.zero;
                
                robot.rotation = box.rotation * Quaternion.Inverse(simObject.GroundedNode.transform.rotation);
                robot.position = box.position - simObject.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(simObject.GroundedBounds.center);
            }
        }
    }
}