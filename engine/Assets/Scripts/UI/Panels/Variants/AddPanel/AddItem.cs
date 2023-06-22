using Synthesis.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddItem : MonoBehaviour {
    [SerializeField]
    TextMeshProUGUI _name;

    public Image background;

    private string _fullPath;
    GameObject p;
    private bool _isRobot;

    private void ItemAnalytics(string item) {
        var update = new AnalyticsEvent(category: $"{item}", action: "Loaded", label: $"{item} Loaded");
        AnalyticsManager.LogEvent(update);
        AnalyticsManager.PostData();
    }

    public void AddModel(bool reverseSideMotors) {
        RobotSimObject.SpawnRobot(_fullPath);
        ItemAnalytics("Robot");
    }

    public void AddField() {
        FieldSimObject.SpawnField(_fullPath);
        ItemAnalytics("Field");
    }

    public void Init(string name, string path) {
        _name.text = name;
        _fullPath  = path;
        p          = GameObject.Find("Tester");
    }

    public void Darken() {
        background.color = new Color(0.972549f, 0.9568627f, 0.9568627f);
    }
    public void Lighten() {
        background.color = Color.white;
    }
}
