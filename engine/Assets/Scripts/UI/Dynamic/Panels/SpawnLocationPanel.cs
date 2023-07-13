using System;
using Modes.MatchMode;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using UnityEngine.EventSystems;

using Input  = UnityEngine.Input;
using Object = UnityEngine.Object;

namespace Synthesis.UI.Dynamic {
    public class SpawnLocationPanel : PanelDynamic {
        private const string SNAP_MODE_KEY = "ROBOT_PLACEMENT_SNAPPING";

        private const float WIDTH            = 400f;
        private const float HEIGHT           = 150f;
        private const float VERTICAL_PADDING = 15f;

        private const float ROBOT_MOVE_SPEED  = 7f;
        private const float ROBOT_TILT_AMOUNT = 0.32f;
        private const float MAX_TILT_DEGREES  = 10f;

        private const float MOVEMENT_SNAPPING_METERS  = 0.5f;
        private const float ROTATION_SNAPPING_DEGREES = 22.5f;

        private const float ROTATE_ROBOT_SPEED = 17;

        private const float SPAWN_HEIGHT_FROM_GROUND = 0.1524f;

        private static readonly Color redBoxColor  = new Color(1, 0, 0, 0.5f);
        private static readonly Color blueBoxColor = new Color(0, 0, 1, 0.5f);

        private static readonly Color redButtonColor  = new Color(0.7f, 0, 0);
        private static readonly Color blueButtonColor = new Color(0, 0, 0.7f);

        private static readonly Material mat =
            new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));
        private static readonly int shaderColorProperty = Shader.PropertyToID("Color_48545d7793c14f3d9e1dd2264f072068");

        private static readonly int fieldLayerMask = 1 << LayerMask.NameToLayer("FieldCollisionLayer");

        private readonly Button[] buttons             = new Button[6];
        private readonly Transform[] _robotHighlights = new Transform[6];

        private readonly Func<Button, Button> DisabledTemplate = b =>
            b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                .StepIntoLabel(l => l.SetColor(ColorManager.SYNTHESIS_WHITE));

        public readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
            return u;
        };

        private int _selectedButton;
        private bool _renderBoxes = false;

        public SpawnLocationPanel() : base(new Vector2(WIDTH, HEIGHT)) {}

        public override bool Create() {
            if (!InputManager.MappedDigitalInputs.ContainsKey(SNAP_MODE_KEY))
                InputManager.AssignDigitalInput(
                    SNAP_MODE_KEY, (Digital) new Digital("LeftShift").WithModifier((int) ModKey.LeftShift));

            Title.SetText("Set Spawn Locations").SetFontSize(25f);
            PanelImage.RootGameObject.SetActive(false);

            Content panel = new Content(null, UnityObject, null);

            float padding = 700;
            panel.SetBottomStretch<Content>(padding, padding, 0);

            AcceptButton.StepIntoLabel(label => label.SetText("Accept")).AddOnClickedEvent(b => {
                DynamicUIManager.ClosePanel<SpawnLocationPanel>();
                MatchStateMachine.Instance.SetState(MatchStateMachine.StateName.FieldConfig);
            });
            CancelButton.RootGameObject.SetActive(false);
            CreateRobotHighlights();
            CreateButtons();
            SelectButton(0);
            return true;
        }

        private DigitalState prevInput = DigitalState.None;

        public override void Update() {
            if (!MainHUD.isConfig) {
                var currentInput                              = InputManager.MappedDigitalInputs[SNAP_MODE_KEY][0].State;
                MatchMode.RoundSpawnLocation[_selectedButton] = currentInput == DigitalState.Held;

                // True the frame the input is pressed or released
                if (prevInput != currentInput) {
                    MatchMode.RawSpawnLocations[_selectedButton] =
                        RoundSpawnLocation(MatchMode.RawSpawnLocations[_selectedButton]);
                }

                prevInput = currentInput;

                FindSpawnPosition();
                RotateRobot();
                MoveRobots();
            }
        }

        public override void Delete() {
            _robotHighlights.ForEach(x => Object.Destroy(x.gameObject));
        }

        /// <summary>
        /// Creates the robot highlight gameobjects and configures them
        /// </summary>
        private void CreateRobotHighlights() {
            for (int i = 0; i < 6; i++) {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                if (!_renderBoxes)
                    obj.SetActive(false);

                var rend      = obj.GetComponent<Renderer>();
                rend.material = mat;
                rend.material.SetColor(shaderColorProperty, (i < 3) ? redBoxColor : blueBoxColor);

                obj.GetComponent<Collider>().isTrigger = true;

                RobotSimObject simObject = MatchMode.Robots[i];
                if (simObject != null) {
                    obj.transform.localScale = MatchMode.Robots[i].RobotBounds.size;
                }

                _robotHighlights[i] = obj.transform;
            }
        }

        /// <summary>
        /// Creates the buttons to select which robot to move
        /// </summary>
        private void CreateButtons() {
            float spacing            = 15f;
            var (left, rightSection) = MainContent.SplitLeftRight((MainContent.Size.x / 3f) - (spacing / 2f), spacing);

            var (center, right) = rightSection.SplitLeftRight((MainContent.Size.x / 3f) - (spacing / 2f), spacing);
            for (int i = 0; i < 6; i++) {
                int buttonIndex = i;
                buttons[i]      = ((i % 3 == 0) ? left : ((i % 3 == 1) ? center : right))
                                 .CreateButton()
                                 .StepIntoLabel(
                                     l => l.SetText($"{((buttonIndex < 3) ? "Red" : "Blue")} {(buttonIndex % 3 + 1)}"))
                                 .ApplyTemplate(VerticalLayout)
                                 .ApplyTemplate(DisabledTemplate)
                                 .AddOnClickedEvent(b => { SelectButton(buttonIndex); });
            }
        }

        /// <summary>
        /// Sets which button is currently selected and updates button colors
        /// </summary>
        /// <param name="index">the selected buttons index</param>
        private void SelectButton(int index) {
            buttons[_selectedButton].Image.Color = ColorManager.TryGetColor(ColorManager.SYNTHESIS_BLACK_ACCENT);
            _selectedButton                      = index;

            buttons[index].Image.Color = (index < 3) ? redButtonColor : blueButtonColor;
        }

        /// <summary>
        /// Sets the selected robots spawn position based on where the mouse is pointing
        /// </summary>
        private void FindSpawnPosition() {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0)) {
                // Raycast out from camera to see where the mouse is pointing
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (UnityEngine.Physics.Raycast(ray, out var hit, 100, fieldLayerMask)) {
                    Transform selectedPosition = _robotHighlights[_selectedButton];

                    Vector3 boxHalfSize = selectedPosition.localScale / 2f;

                    Vector3 rayOrigin =
                        (MatchMode.RoundSpawnLocation[_selectedButton])
                            ? RoundSpawnLocation((hit.point + Vector3.up * 20f, Quaternion.identity)).position
                            : hit.point + Vector3.up * 20f;

                    // Box cast down towards where the mouse is pointing to find the lowest suitable spawn position for
                    // the robot
                    if (UnityEngine.Physics.BoxCast(rayOrigin, boxHalfSize, Vector3.down, out var boxHit,
                            MatchMode.GetSpawnLocation(_selectedButton).rotation, 30f, fieldLayerMask)) {
                        MatchMode.RawSpawnLocations[_selectedButton].position = new Vector3(
                            hit.point.x, boxHit.point.y + boxHalfSize.y + SPAWN_HEIGHT_FROM_GROUND, hit.point.z);
                    }
                }
            }
        }

        /// <summary>
        /// Rotate the robots yaw based on scroll wheel input. On mac, holding shift switches the scroll wheel to
        /// the y axis, so the one line if statement checks for that
        /// </summary>
        private void RotateRobot() {
            Vector2 input     = Input.mouseScrollDelta;
            float scrollSpeed = (Mathf.Abs(input.x) > Mathf.Abs(input.y)) ? input.x : input.y;
            if (Mathf.Abs(scrollSpeed) < 0.005f)
                return;

            MatchMode.RawSpawnLocations[_selectedButton].rotation.eulerAngles +=
                Vector3.up * (Mathf.Sign(scrollSpeed) * ROTATE_ROBOT_SPEED);
        }

        public static (Vector3 position, Quaternion rotation)
            RoundSpawnLocation((Vector3 position, Quaternion rotation) spawnLocation) {
            return (
                new Vector3(Mathf.Round(spawnLocation.position.x / MOVEMENT_SNAPPING_METERS) * MOVEMENT_SNAPPING_METERS,
                    spawnLocation.position.y,
                    Mathf.Round(spawnLocation.position.z / MOVEMENT_SNAPPING_METERS) * MOVEMENT_SNAPPING_METERS),
                Quaternion.Euler(0,
                    Mathf.Round(spawnLocation.rotation.eulerAngles.y / ROTATION_SNAPPING_DEGREES) *
                        ROTATION_SNAPPING_DEGREES,
                    0));
        }

        /// <summary>
        /// Smoothly lerps all robot objects towards their spawn location and tilts them in the direction they are
        /// moving
        /// </summary>
        private void MoveRobots() {
            for (int i = 0; i < 6; i++) {
                if (MatchMode.Robots[i] == null)
                    continue;

                var robot = MatchMode.Robots[i].RobotNode.transform;
                var box   = _robotHighlights[i];

                Vector3 prevPos = box.position;
                Vector3 target  = MatchMode.GetSpawnLocation(i).position;
                box.position    = Vector3.Lerp(prevPos, target, ROBOT_MOVE_SPEED * Time.deltaTime);

                Vector3 robotTilt = (target - prevPos) * (45f * ROBOT_TILT_AMOUNT);

                Quaternion robotYaw = MatchMode.GetSpawnLocation(i).rotation;

                box.rotation = Quaternion.Euler(Mathf.Clamp(robotTilt.z, -MAX_TILT_DEGREES, MAX_TILT_DEGREES), 0,
                                   Mathf.Clamp(-robotTilt.x, -MAX_TILT_DEGREES, MAX_TILT_DEGREES)) *
                               robotYaw;

                RobotSimObject simObject = MatchMode.Robots[i];

                robot.rotation = Quaternion.identity;
                robot.position = Vector3.zero;

                robot.rotation = box.rotation * Quaternion.Inverse(simObject.GroundedNode.transform.rotation);
                robot.position = box.position - simObject.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(
                                                    simObject.GroundedBounds.center);
            }
        }
    }
}