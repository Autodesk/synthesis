using System;
using Synthesis.Gizmo;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class ZoneConfigPanel : PanelDynamic {
    private const float MODAL_WIDTH = 400f;
    private const float MODAL_HEIGHT = 600f;

    private const float VERTICAL_PADDING = 16f;

    private InputField _zoneNameInput;
    private Button _zoneAllianceButton;
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

    private Action<ScoringZone, bool> _callback;

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
            _initialData.DestroyGamepiece = zone.DestroyObject;
            _initialData.Points = zone.Points;
            var scale = zone.GameObject.transform.localScale;
            _initialData.XScale = scale.x;
            _initialData.YScale = scale.y;
            _initialData.ZScale = scale.z;
            _initialPosition = zone.GameObject.transform.position;
            _initialRotation = zone.GameObject.transform.rotation;
        }
    }

    public override bool Create() {
        Title.SetText("Scoring Zone Config");

        AcceptButton.AddOnClickedEvent(b => {
            // call one last time to update data
            // don't want to update data and call callback on every character typed for name
            CopyDataToZone(_data, _zone);
            _data.Name = _zoneNameInput.Value;
            
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

    public override void Update() { }

    public override void Delete() {
        GizmoManager.ExitGizmo();
    }
}
