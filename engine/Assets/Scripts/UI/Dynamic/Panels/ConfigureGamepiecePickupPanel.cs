using System;
using Synthesis.Gizmo;
using UnityEngine;

namespace Synthesis.UI.Dynamic {
    public class ConfigureGamepiecePickupPanel : PanelDynamic
    {
        public ConfigureGamepiecePickupPanel() : base(new Vector2(275, 150)) { }

        private bool _selectingNode;
        private LabeledButton _selectNodeButton;
        private LabeledButton _moveTriggerButton;
        private Slider _zoneSizeSlider;

        private GameObject _zoneObject;

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
                DynamicUIManager.CloseActivePanel();
                return;
            }

            AcceptButton.AddOnClickedEvent(b => {
                _save = true;
                DynamicUIManager.CloseActivePanel();
            }).StepIntoLabel(l => l.SetText("Save"));

            _zoneObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var renderer = _zoneObject.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Shader Graphs/DefaultSynthesisTransparentShader"));
            renderer.material.SetColor("Color_48545d7793c14f3d9e1dd2264f072068", new Color(0f, 0f, 0f, 0.2f));
            renderer.material.SetFloat("Vector1_d66a0e8b289a457c85b3b4408b4f3c2f", 0f);

            GizmoManager.SpawnGizmo(
                _zoneObject.transform,
                t => _zoneObject.transform.position = t.Position,
                t => {
                    if (!_exiting)
                        DynamicUIManager.CloseActivePanel();
                }
            );

            _selectNodeButton = MainContent.CreateLabeledButton()
                .SetHeight<LabeledButton>(30)
                .StepIntoLabel(l => l.SetText("Select a node"))
                .StepIntoButton(b => b.StepIntoLabel(l => l.SetText("Select")).AddOnClickedEvent(SelectNodeButton))
                .ApplyTemplate<LabeledButton>(VerticalLayout);
            // _moveTriggerButton = MainContent.CreateLabeledButton()
            //     .SetHeight<LabeledButton>(30)
            //     .StepIntoLabel(l => l.SetText("Move pickup zone"))
            //     .StepIntoButton(b => b.StepIntoLabel(l => l.SetText("Move")))
            //     .ApplyTemplate<LabeledButton>(VerticalLayout);
            _zoneSizeSlider = MainContent.CreateSlider(label: "Zone Size", minValue: 0.1f, maxValue: 1f)
                .ApplyTemplate<Slider>(VerticalLayout)
                .AddOnValueChangedEvent(
                    (s, v) => _zoneObject.transform.localScale = new Vector3(v, v, v))
                .SetValue(0.5f);
        }

        public void SelectNodeButton(Button b) {
            if (!_selectingNode) {
                _selectingNode = true;
                _selectNodeButton.StepIntoLabel(l => l.SetText("Selecting..."));
                SetSelectButtonState(false);
            } else {
                _selectingNode = false;
                _selectNodeButton.StepIntoLabel(l => l.SetText("Select a node"));
                SetSelectButtonState(true);
            }
        }

        // TODO: Maybe make node selection into its own component?
        public void SetSelectButtonState(bool isButtonEnabled) {
            if (isButtonEnabled) {
                _selectNodeButton.StepIntoButton(
                    b => b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                        .StepIntoLabel(l => l.SetText("Select"))
                );
            } else {
                _selectNodeButton.StepIntoButton(
                    b => b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                        .StepIntoLabel(l => l.SetText("..."))
                );
            }
        }

        public override void Delete() {
            // Handle Panel
            _exiting = true;
            GizmoManager.ExitGizmo();

            // Save Data
            if (_save) {
                Debug.Log("TODO: Save trigger");
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
                                _selectingNode = false;
                                _selectNodeButton.StepIntoLabel(l => l.SetText("Selected"));
                                SetSelectButtonState(true);
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
