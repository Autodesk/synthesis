using Synthesis.UI.Dynamic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Synthesis.UI.Dynamic;

public class MatchModeModal : ModalDynamic
{

    private int _robotIndex = -1;
    private int _fieldIndex = -1;
    private string[] _robotFiles;
    private string[] _fieldFiles;

    private int _allianceColor = 0;
    private int _spawnPosition = 0;


    public Func<UIComponent, UIComponent> VerticalLayout = (u) =>
    {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 15f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
        return u;
    };

    public MatchModeModal() : base(new Vector2(500, 500)) { }

    public override void Create()
    {
        _robotFiles = Directory.GetFiles(ParsePath("$appdata/Autodesk/Synthesis/Mira", '/')).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
        _fieldFiles = Directory.GetFiles(ParsePath("$appdata/Autodesk/Synthesis/Mira/Fields", '/')).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

        Title.SetText("Match Mode");
        Description.SetText("Configure Match Mode");

        AcceptButton
            .StepIntoLabel(label => label.SetText("Load"))
            .AddOnClickedEvent(b =>
            {
                if (_robotIndex != -1 && _fieldIndex != -1)
                {
                    if (FieldSimObject.CurrentField != null) FieldSimObject.CurrentField.DeleteField();
                    FieldSimObject.SpawnField(_fieldFiles[_fieldIndex]);

                    if (RobotSimObject.GetCurrentlyPossessedRobot() != null) RobotSimObject.GetCurrentlyPossessedRobot().Destroy();
                    RobotSimObject.SpawnRobot(_robotFiles[_robotIndex]);

                    DynamicUIManager.CloseActiveModal();
                    DynamicUIManager.CreatePanel<MatchmodeScoreboardPanel>();
                }
            });
        CancelButton.AddOnClickedEvent(b =>
        {//need to add in isMatchModalOpen integration
            DynamicUIManager.CloseActiveModal();
        });

        MainContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select Robot");
        var chooseRobotDropdown = MainContent.CreateDropdown()
            .SetOptions(_robotFiles.Select(x => Path.GetFileName(x)).ToArray())
            .AddOnValueChangedEvent((d, i, data) => _robotIndex = i).ApplyTemplate(VerticalLayout);

        _robotIndex = _robotFiles.Length > 0 ? 0 : -1;

        MainContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select Field");
        var chooseFieldDropdown = MainContent.CreateDropdown()
            .SetOptions(_fieldFiles.Select(x => Path.GetFileName(x)).ToArray())
            .AddOnValueChangedEvent((d, i, data) => _fieldIndex = i).ApplyTemplate(VerticalLayout);

        _fieldIndex = _fieldFiles.Length > 0 ? 0 : -1;

        MainContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select Alliance Color");
        var allianceSelection = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "Red", "Blue" })
            .AddOnValueChangedEvent((d, i, data) => _allianceColor = i).ApplyTemplate(VerticalLayout);

        MainContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select Spawn Position");
        var spawnPosition = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "Left", "Middle", "Right" })
            .AddOnValueChangedEvent((d, i, data) => _spawnPosition = i).ApplyTemplate(VerticalLayout);
    }
    
    public override void Update() {}


    public override void Delete()
    {
    }

    public static string ParsePath(string p, char c)
    {
        string[] a = p.Split(c);
        string b = "";
        for (int i = 0; i < a.Length; i++)
        {
            switch (a[i])
            {
                case "$appdata":
                    b += System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                    break;
                default:
                    b += a[i];
                    break;
            }
            if (i != a.Length - 1)
                b += System.IO.Path.AltDirectorySeparatorChar;
        }
        // Debug.Log(b);
        return b;
    }
}

