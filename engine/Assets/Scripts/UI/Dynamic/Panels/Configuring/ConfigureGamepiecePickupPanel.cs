using System;
using Synthesis.Gizmo;
using UnityEngine;

using ITD = RobotSimObject.IntakeTriggerData;

namespace Synthesis.UI.Dynamic {
    public class ConfigureGamepiecePickupPanel : PanelDynamic
    {
        public ConfigureGamepiecePickupPanel() : base(new Vector2(275, 150)) { }

        private bool _selectingNode;
        private LabeledButton _selectNodeButton;
        private LabeledButton _moveTriggerButton;
        private Slider _zoneSizeSlider;

        private GameObject _zoneObject;
        private ITD _resultingData;

        private bool _exiting = false;
        private bool _save = false;

        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
            return u;
        };

        public override void Create() {
            Title.SetText("Configure Pickup");

            if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
                DynamicUIManager.ClosePanel<ConfigureGamepiecePickupPanel>();
                return;
            }

            var robot = RobotSimObject.GetCurrentlyPossessedRobot();
            var existingData = robot.IntakeData;
            if (existingData.HasValue) {
                _resultingData = existingData.Value;
            } else {
                _resultingData = new ITD {
                    NodeName = "grounded",
                    RelativePosition = robot.GroundedBounds.center.ToArray(),
                    TriggerSize = 0.5f,
                    StorageCapacity = 1
                };
            }

            // TODO: Limit to one for now before we add UI for it
            _resultingData.StorageCapacity = 1;

            AcceptButton.AddOnClickedEvent(b => {
                _save = true;
                DynamicUIManager.ClosePanel<ConfigureGamepiecePickupPanel>();
            }).StepIntoLabel(l => l.SetText("Save"));

            _zoneObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var renderer = _zoneObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisShader"));
            renderer.material.SetColor("Color_2aa135b32e7e4808b9be05c544657380", new Color(0f, 1f, 0f, 0.4f));
            renderer.material.SetFloat("Vector1_dd87d7fcd1f1419f894566001d248ab9", 0f);
            var node = robot.RobotNode.transform.Find(_resultingData.NodeName);
            _zoneObject.transform.rotation = node.transform.rotation;
            _zoneObject.transform.position = node.transform.localToWorldMatrix.MultiplyPoint(_resultingData.RelativePosition.ToVector3());

            GizmoManager.SpawnGizmo(
                _zoneObject.transform,
                t => _zoneObject.transform.position = t.Position,
                t => {
                    _resultingData.RelativePosition =
                        robot.RobotNode.transform.Find(_resultingData.NodeName) // Get Node
                        .transform.worldToLocalMatrix.MultiplyPoint(t.Position).ToArray(); // Transform point to local space
                    if (!_exiting)
                        DynamicUIManager.ClosePanel<ConfigureGamepiecePickupPanel>();
                }
            );

            _selectNodeButton = MainContent.CreateLabeledButton()
                .SetHeight<LabeledButton>(30)
                .StepIntoLabel(l => l.SetText("Select a node")
                    .SetLeftStretch<Label>()
                    .SetWidth<Label>(125))
                .StepIntoButton(b => b.StepIntoLabel(l => l.SetText("Select")).AddOnClickedEvent(SelectNodeButton)
                    .SetWidth<Button>(125))
                .ApplyTemplate<LabeledButton>(VerticalLayout);
            SetSelectUIState(false);
            // _moveTriggerButton = MainContent.CreateLabeledButton()
            //     .SetHeight<LabeledButton>(30)
            //     .StepIntoLabel(l => l.SetText("Move pickup zone"))
            //     .StepIntoButton(b => b.StepIntoLabel(l => l.SetText("Move")))
            //     .ApplyTemplate<LabeledButton>(VerticalLayout);
            _zoneSizeSlider = MainContent.CreateSlider(label: "Zone Size", minValue: 0.1f, maxValue: 1f)
                .ApplyTemplate<Slider>(VerticalLayout)
                .AddOnValueChangedEvent(
                    (s, v) => {
                        _resultingData.TriggerSize = v;
                        _zoneObject.transform.localScale = new Vector3(v, v, v);
                    })
                .SetValue(_resultingData.TriggerSize);
        }

        public void SelectNodeButton(Button b) {
            if (!_selectingNode) {
                _selectingNode = true;
            } else {
                _selectingNode = false;
            }
            SetSelectUIState(_selectingNode);
        }

        // TODO: Maybe make node selection into its own component?
        public void SetSelectUIState(bool isUserSelecting) {
            if (isUserSelecting) {
                _selectNodeButton.StepIntoLabel(l => l.SetText("Selecting..."));
                _selectNodeButton.StepIntoButton(
                    b => b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                        .StepIntoLabel(l => l.SetText("..."))
                );
            } else {
                _selectNodeButton.StepIntoLabel(l => l.SetText(_resultingData.NodeName));
                _selectNodeButton.StepIntoButton(
                    b => b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                        .StepIntoLabel(l => l.SetText("Select"))
                );
            }
        }

        public override void Delete() {
            // Handle Panel
            _exiting = true;
            GizmoManager.ExitGizmo();

            // Save Data
            if (_save) {
                RobotSimObject.GetCurrentlyPossessedRobot().IntakeData = _resultingData;
            }

            // Cleanup
            GameObject.Destroy(_zoneObject);
        }

        public override void Update() {
            if (_selectingNode) {
                if (Input.GetKeyDown(KeyCode.Mouse0)) {

                    // Enable Collision Detection for the Robot
                    RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.GetComponentsInChildren<Rigidbody>().ForEach(x => x.detectCollisions = true);

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;
                    bool hit = UnityEngine.Physics.Raycast(ray, out hitInfo);
                    if (hit) {
                        if (hitInfo.rigidbody != null) {
                            Debug.Log(hitInfo.rigidbody.name);
                            if (hitInfo.rigidbody.transform.parent == RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform) {
                                Debug.Log($"Selecting Node: {hitInfo.rigidbody.name}");

                                _resultingData.NodeName = hitInfo.rigidbody.name;

                                _selectingNode = false;
                                SetSelectUIState(false);
                            }
                        }
                    }

                    // Disable Collision Detection for the Robot
                    RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.GetComponentsInChildren<Rigidbody>().ForEach(x => x.detectCollisions = true);
                }
            }
        }
    }
}
