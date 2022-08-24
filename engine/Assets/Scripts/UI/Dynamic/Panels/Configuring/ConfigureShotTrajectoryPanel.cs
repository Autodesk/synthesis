using System;
using Synthesis.Gizmo;
using UnityEngine;

using STD = RobotSimObject.ShotTrajectoryData;

namespace Synthesis.UI.Dynamic {
    public class ConfigureShotTrajectoryPanel : PanelDynamic
    {
        public ConfigureShotTrajectoryPanel() : base(new Vector2(275, 150)) { }

        private bool _selectingNode;
        private LabeledButton _selectNodeButton;
        private Slider _exitSpeedSlider;

        private GameObject _arrowObject;

        private bool _exiting = false;
        private bool _save = false;

        private STD _resultingData;

        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
            return u;
        };

        public override void Create() {
            Title.SetText("Configure Shooting");

            if (RobotSimObject.CurrentlyPossessedRobot == string.Empty) {
                DynamicUIManager.CloseActivePanel();
                return;
            }

            var robot = RobotSimObject.GetCurrentlyPossessedRobot();
            var existingData = robot.TrajectoryData;
            if (existingData.HasValue) {
                _resultingData = existingData.Value;
            } else {
                _resultingData = new STD {
                    NodeName = "grounded",
                    RelativePosition = robot.GroundedBounds.center.ToArray(),
                    RelativeRotation = Quaternion.identity.ToArray(),
                    EjectionSpeed = 2f
                };
            }

            AcceptButton.AddOnClickedEvent(b => {
                _save = true;
                DynamicUIManager.CloseActivePanel();
            }).StepIntoLabel(l => l.SetText("Save"));

            _arrowObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _arrowObject.transform.localScale = new Vector3(0.15f, 0.15f, 1f);
            var renderer = _arrowObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisShader"));
            renderer.material.SetColor("Color_2aa135b32e7e4808b9be05c544657380", new Color(0f, 1f, 0f, 0.4f));
            renderer.material.SetFloat("Vector1_dd87d7fcd1f1419f894566001d248ab9", 0f);
            var node = robot.RobotNode.transform.Find(_resultingData.NodeName);
            _arrowObject.transform.rotation = node.transform.rotation * _resultingData.RelativeRotation.ToQuaternion();
            _arrowObject.transform.position = node.transform.localToWorldMatrix.MultiplyPoint(_resultingData.RelativePosition.ToVector3());

            GizmoManager.SpawnGizmo(
                _arrowObject.transform,
                t => {
                    _arrowObject.transform.rotation = t.Rotation;
                    _arrowObject.transform.position = t.Position;
                    _arrowObject.transform.position += _arrowObject.transform.forward * 0.5f;
                },
                t => {
                    var node = robot.RobotNode.transform.Find(_resultingData.NodeName);
                    _resultingData.RelativePosition =
                        node.transform.worldToLocalMatrix.MultiplyPoint(t.Position).ToArray();
                    _resultingData.RelativeRotation =
                        (node.transform.worldToLocalMatrix * Matrix4x4.TRS(Vector3.zero, t.Rotation, Vector3.one))
                        .rotation.ToArray();
                    if (!_exiting)
                        DynamicUIManager.CloseActivePanel();
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
            _exitSpeedSlider = MainContent.CreateSlider(label: "Speed", minValue: 0f, maxValue: 10f)
                .ApplyTemplate<Slider>(VerticalLayout)
                .AddOnValueChangedEvent(
                    (s, v) => {
                        _resultingData.EjectionSpeed = v;
                        // TODO: Change the arrow?
                    })
                .SetValue(_resultingData.EjectionSpeed);
        }

        public void SelectNodeButton(Button b) {
            _selectingNode = !_selectingNode;
            SetSelectUIState(_selectingNode);
        }

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
            // Handle panel
            _exiting = true;
            GizmoManager.ExitGizmo();

            // Save data
            if (_save) {
                RobotSimObject.GetCurrentlyPossessedRobot().TrajectoryData = _resultingData;
            }

            // Cleanup
            GameObject.Destroy(_arrowObject);
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
