using System;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ZoneConfigPanel : PanelDynamic {
    private const float MODAL_WIDTH = 500f;
    private const float MODAL_HEIGHT = 600f;

    private const float VERTICAL_PADDING = 16f;

    private InputField _zoneNameInput;
    private Button _zoneAllianceButton;
    private LabeledButton _zoneParentButton;
    private NumberInputField _pointsInputField;
    private Toggle _deleteGamepieceToggle;
    private Slider _xScaleSlider;
    private Slider _yScaleSlider;
    private Slider _zScaleSlider;

    private ScoringZoneData _data = new ScoringZoneData();
    private ScoringZoneData _initialData = new ScoringZoneData();
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private ScoringZone _zone;

    private bool _isNewZone = true;
    
    private bool _selectingNode;

    private GameObject _zoneObject;
    
    private HighlightComponent _hoveringNode = null;
    private HighlightComponent _selectedNode = null;

    private Action<ScoringZone, bool> _callback;

    private SortedDictionary<int, bool> _initialFieldCollisions = new SortedDictionary<int, bool>();

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
        return u;
    };

    public ZoneConfigPanel(ScoringZone zone) : base(
        new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {
        _zone = zone;
        if (zone is not null) {
            _isNewZone = false;
            _initialData.Name = zone.Name;
            _initialData.Alliance = zone.Alliance;
            _initialData.Parent = zone.GameObject.transform.parent;
            _initialData.DestroyGamepiece = zone.DestroyObject;
            _initialData.Points = zone.Points;
            var scale = zone.GameObject.transform.localScale;
            _initialData.XScale = scale.x;
            _initialData.YScale = scale.y;
            _initialData.ZScale = scale.z;
            _initialPosition = zone.GameObject.transform.position;
            _initialRotation = zone.GameObject.transform.rotation;
            _data = _initialData;
        }
    }

    public override bool Create() {
        Title.SetText("Scoring Zone Config");

        AcceptButton.AddOnClickedEvent(b => {
            // call one last time to update data
            // don't want to update data and call callback on every character typed for name
            CopyDataToZone(_data, _zone);
            _data.Name = _zoneNameInput.Value;
            _zone.GameObject.name = _data.Name;
            
            if (_isNewZone)
                FieldSimObject.CurrentField.ScoringZones.Add(_zone);
            
            _callback.Invoke(_zone, _isNewZone);
            DynamicUIManager.ClosePanel<ZoneConfigPanel>();
        });

        CancelButton.AddOnClickedEvent(b => {
            if (_isNewZone)
                GameObject.Destroy(_zone.GameObject);
            else {
                CopyDataToZone(_initialData, _zone);
                _zone.GameObject.transform.position = _initialPosition;
                _zone.GameObject.transform.rotation = _initialRotation;
            }
            DynamicUIManager.ClosePanel<ZoneConfigPanel>();
        });

        _zoneNameInput = MainContent.CreateInputField().StepIntoLabel(l => l.SetText("Name"))
            .StepIntoHint(h => h.SetText("Zone Name"))
            .ApplyTemplate(VerticalLayout);
        _zoneAllianceButton = MainContent.CreateButton().StepIntoLabel(l => l.SetText("Blue Alliance")).AddOnClickedEvent(
                b => {
                    _data.Alliance = _data.Alliance == Alliance.Blue ? Alliance.Red : Alliance.Blue;
                    ConfigureAllianceButton();
                    DataUpdated();
                })
            .SetBackgroundColor<Button>(Color.blue)
            .ApplyTemplate(VerticalLayout);

        _zoneParentButton = MainContent.CreateLabeledButton()
            .StepIntoLabel(l => l.SetText(_initialData.Parent != null ? _initialData.Parent.name : "Parent Object"))
            .StepIntoButton(b => {
                b.StepIntoLabel(l => l.SetText("Click to select...")).AddOnClickedEvent(SelectParentButton);
            }).ApplyTemplate(VerticalLayout);

        _pointsInputField = MainContent.CreateNumberInputField()
            .StepIntoLabel(l => l.SetText("Points"))
            .StepIntoHint(l => l.SetText("Points"))
            .ApplyTemplate(VerticalLayout)
            .AddOnValueChangedEvent((f, n) => _data.Points = n);

        _deleteGamepieceToggle = MainContent.CreateToggle(false, "Destroy Gamepiece")
            .AddOnStateChangedEvent((t, v) => {
                _data.DestroyGamepiece = !_data.DestroyGamepiece;
                t.State = _data.DestroyGamepiece; // just in case
            }).ApplyTemplate(VerticalLayout);

        _xScaleSlider = MainContent.CreateSlider(label: "X Scale", minValue: 0.1f, maxValue: 10f, currentValue: 1)
            .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent(
                (s, v) => {
                    _data.XScale = v;
                    DataUpdated();
                });

        _yScaleSlider = MainContent.CreateSlider(label: "Y Scale", minValue: 0.1f, maxValue: 10f, currentValue: 1)
            .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent(
                (s, v) => {
                    _data.YScale = v;
                    DataUpdated();
                });

        _zScaleSlider = MainContent.CreateSlider(label: "Z Scale", minValue: 0.1f, maxValue: 10f, currentValue: 1)
            .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent(
                (s, v) => {
                    _data.ZScale = v;
                    DataUpdated();
                });

        GameObject obj;
        if (_zone is null) {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _zone = new ScoringZone(obj, "temp scoring zone", Alliance.Blue, 0, false);
        } else {
            obj = _zone.GameObject;
            UseZone(_zone);
        }

        GizmoManager.SpawnGizmo(obj.transform,
            t => {
                obj.transform.position = t.Position;
                obj.transform.rotation = t.Rotation;
            },
            t => { });

        return true;
    }

    private void CopyDataToZone(ScoringZoneData data, ScoringZone zone) {
        zone.Name = _zoneNameInput.Value;
        zone.GameObject.name = data.Name;
        zone.GameObject.tag = data.Alliance == Alliance.Red ? "red zone" : "blue zone";
        zone.GameObject.transform.parent = data.Parent;
        zone.Alliance = data.Alliance;
        zone.Points = data.Points;
        zone.GameObject.transform.localScale = new Vector3(data.XScale, data.YScale, data.ZScale);
        zone.DestroyObject = data.DestroyGamepiece;
    }

    private void ConfigureAllianceButton() {
        if (_data.Alliance == Alliance.Blue) {
            _zoneAllianceButton.StepIntoLabel(l => l.SetText("Blue Alliance")).SetBackgroundColor<Button>(Color.blue);
        } else {
            _zoneAllianceButton.StepIntoLabel(l => l.SetText("Red Alliance")).SetBackgroundColor<Button>(Color.red);
        }
    }

    public void UseZone(ScoringZone zone) {
        _zone = zone;
        _data.Name = _zone.Name;
        _zoneNameInput.SetValue(_zone.Name);

        _data.Alliance = zone.Alliance;
        ConfigureAllianceButton();

        _data.Points = zone.Points;
        _pointsInputField.SetValue(_zone.Points);
        
        _data.DestroyGamepiece = zone.DestroyObject;
        _deleteGamepieceToggle.SetState(_zone.DestroyObject, notify: false);
        
        var localScale = zone.GameObject.transform.localScale;
        _data.XScale = localScale.x;
        _xScaleSlider.SetValue(_data.XScale);
        _data.YScale = localScale.y;
        _yScaleSlider.SetValue(_data.YScale);
        _data.ZScale = localScale.z;
        _zScaleSlider.SetValue(_data.ZScale);
    }

    private void DataUpdated() {
        _zone.Alliance = _data.Alliance;
        _zone.Points = _data.Points;
        _zone.GameObject.transform.localScale = new Vector3(_data.XScale, _data.YScale, _data.ZScale);
    }

    public void SetCallback(Action<ScoringZone, bool> callback) {
        _callback = callback;
    }
    
    public void SelectParentButton(Button b) {
        if (!_selectingNode) {
            // I don't like this; do we just want all field rigidbodies to detect collisions?
            FieldSimObject.CurrentField.FieldObject.GetComponentsInChildren<Rigidbody>().ForEach(x => {
                _initialFieldCollisions.Add(x.GetHashCode(), x.detectCollisions);
                x.detectCollisions = true;
            });
            _selectingNode = true;
        } else {
            FieldSimObject.CurrentField.FieldObject.GetComponentsInChildren<Rigidbody>().ForEach(x => x.detectCollisions = _initialFieldCollisions[x.GetHashCode()]);
            _initialFieldCollisions.Clear();
            _selectingNode = false;
        }
        SetSelectUIState(_selectingNode);
    }
    
    public void SetSelectUIState(bool isUserSelecting) {
        if (isUserSelecting) {
            _zoneParentButton.StepIntoLabel(l => l.SetText("Selecting..."));
            _zoneParentButton.StepIntoButton(
                b => b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_BLACK_ACCENT))
                    .StepIntoLabel(l => l.SetText("..."))
            );
        } else {
            _zoneParentButton.StepIntoLabel(l => l.SetText(_selectedNode.name));
            _zoneParentButton.StepIntoButton(
                b => b.StepIntoImage(i => i.SetColor(ColorManager.SYNTHESIS_ORANGE))
                    .StepIntoLabel(l => l.SetText("Select"))
            );
        }
    }

    public override void Update() {
        if (_selectingNode) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(ray, out hitInfo);

            if (hit && hitInfo.rigidbody != null && hitInfo.rigidbody.transform.parent == FieldSimObject.CurrentField.FieldObject.transform && !hitInfo.rigidbody.transform.CompareTag("field")) {
                if (_hoveringNode != null && (_selectedNode == null || !_selectedNode.name.Equals(_hoveringNode.name))) {
                    _hoveringNode.enabled = false;
                }
                _hoveringNode = hitInfo.rigidbody.GetComponent<HighlightComponent>();
                if (_selectedNode == null || hitInfo.rigidbody.name != _selectedNode.name) {
                    _hoveringNode.enabled = true;
                    _hoveringNode.Color = ColorManager.TryGetColor(ColorManager.SYNTHESIS_HIGHLIGHT_HOVER);
                }

                if (Input.GetKeyDown(KeyCode.Mouse0)) {
                    if (_selectedNode != null) {
                        _selectedNode.enabled = false;
                    }

                    _selectedNode = _hoveringNode;
                    _selectedNode.enabled = true;
                    _selectedNode.Color = ColorManager.TryGetColor(ColorManager.SYNTHESIS_HIGHLIGHT_SELECT);
                    _data.Parent = _selectedNode.gameObject.transform;
                    _hoveringNode = null;

                    _selectingNode = false;
                    SetSelectUIState(false);
                }
            } else {
                if (_hoveringNode != null && (_selectedNode == null || !_selectedNode.name.Equals(_hoveringNode.name))) {
                    _hoveringNode.enabled = false;
                    _hoveringNode = null;
                }
            }
        }
    }

    public override void Delete() {
        if (_hoveringNode != null) {
            _hoveringNode.enabled = false;
        }
        if (_selectedNode != null) {
            _selectedNode.enabled = false;
        }
        GizmoManager.ExitGizmo();
    }
}
