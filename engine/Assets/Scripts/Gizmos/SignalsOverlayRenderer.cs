using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignalsOverlayRenderer : MonoBehaviour {
    public static SignalsOverlayRenderer Instance { get; private set; }

    public Font GuiFont;
    [SerializeField]
    private Transform Canvas;
    [SerializeField]
    private GameObject LinePrefab;

    public void Awake() {
        Instance = this;
    }

    public void OnGUI() {

        var style = new GUIStyle() {
            alignment = TextAnchor.MiddleCenter,
            font      = GuiFont,
            fontSize  = 36,
            normal    = new GUIStyleState() { textColor = Color.white },
        };

        GUI.contentColor = Color.white;

        GUI.Label(new Rect(100f, 100f, 50f, 50f), "0", style);
    }

    public static GameObject BuildNewLinePrefab() => Instantiate(Instance.LinePrefab, Instance.Canvas);
}

public class Line {

    private const string POINT_A              = "_PointA";
    private const string POINT_B              = "_PointB";
    private const string COLOR                = "_Color";
    private const string STROKE               = "_Stroke";
    private const string ACTUAL_SCREEN_PARAMS = "_ActualScreenParams";

    private Material _lineMaterial;
    private GameObject _lineObject;

    private Vector2 _pointA = new Vector2(0, 0);
    public Vector2 PointA {
        get => _pointA;
        set {
            _pointA = value;
            _lineMaterial.SetVector(POINT_A, new Vector4(_pointA.x, _pointA.y, 0f, 1f));
        }
    }
    private Vector2 _pointB = new Vector2(0, 0);
    public Vector2 PointB {
        get => _pointB;
        set {
            _pointB = value;
            _lineMaterial.SetVector(POINT_B, new Vector4(_pointB.x, _pointB.y, 0f, 1f));
        }
    }
    private Color _color = Color.white;
    public Color Color {
        get => _color;
        set {
            _color = value;
            _lineMaterial.SetColor(COLOR, _color);
        }
    }
    private float _stroke = 1.0f;
    public float Stroke {
        get => _stroke;
        set {
            _stroke = value;
            _stroke = Mathf.Clamp(_stroke, 1.0f, 100.0f);
            _lineMaterial.SetFloat(STROKE, _stroke);
        }
    }
    private Vector2 _ASP = new Vector2(1920, 1080);
    public Vector2 ActualScreenParams {
        get => _ASP;
        set {
            _ASP = value;
            _lineMaterial.SetVector(ACTUAL_SCREEN_PARAMS, new Vector4(_ASP.x, _ASP.y, 0f, 1f));
        }
    }

    public Line(Vector2 a, Vector2 b) {
        _lineObject   = SignalsOverlayRenderer.BuildNewLinePrefab();
        var img       = _lineObject.GetComponent<Image>();
        _lineMaterial = new Material(img.material);
        img.material  = _lineMaterial;

        // Init
        PointA             = a;
        PointB             = b;
        Color              = _color;
        Stroke             = _stroke;
        ActualScreenParams = _ASP;
    }

    public void DeleteLine() {
        _lineMaterial = null;
        GameObject.Destroy(_lineObject);
    }
}
