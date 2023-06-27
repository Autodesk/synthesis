using System;
using System.Collections;
using System.IO;
using System.Linq;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using UnityEngine;
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
        var robotsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira", '/');
        if (!Directory.Exists(robotsFolder))
            Directory.CreateDirectory(robotsFolder);
        var fieldsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira/Fields", '/');
        if (!Directory.Exists(fieldsFolder))
            Directory.CreateDirectory(fieldsFolder);

        _robotFiles = Directory.GetFiles(robotsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
        _fieldFiles = Directory.GetFiles(fieldsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

        Title.SetText("Match Mode");
        Description.SetText("Configure Match Mode");

        AcceptButton
            .StepIntoLabel(label => label.SetText("Load"))
            .AddOnClickedEvent(b =>
            {
                if (_robotIndex != -1 && _fieldIndex != -1)
                {
                    DynamicUIManager.CreateModal<LoadingScreenModal>(); 
                    MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>();
                    if (_mb != null)
                    {
                        Debug.Log("Found a MonoBehaviour.");
                        _mb.StartCoroutine(LoadMatch());
                    }
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

        /*MainContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select Spawn Position");
        var spawnPosition = MainContent.CreateDropdown().ApplyTemplate(Dropdown.VerticalLayoutTemplate)
            .SetOptions(new string[] { "Left", "Middle", "Right" })
            .AddOnValueChangedEvent((d, i, data) => _spawnPosition = i).ApplyTemplate(VerticalLayout);*/
    }
    
    public IEnumerator LoadMatch()
    {
        yield return new WaitForSeconds(0.05f);

        if(MatchMode.currentFieldIndex != _fieldIndex)
        {
            if (FieldSimObject.CurrentField != null) FieldSimObject.DeleteField();
            FieldSimObject.SpawnField(_fieldFiles[_fieldIndex]);
            MatchMode.currentFieldIndex = _fieldIndex;
        }
        
        if(MatchMode.currentRobotIndex != _robotIndex)
        {
            if (RobotSimObject.GetCurrentlyPossessedRobot() != null) RobotSimObject.GetCurrentlyPossessedRobot().Destroy();

            PreferenceManager.Load();
            if (PreferenceManager.ContainsPreference(MatchMode.PREVIOUS_SPAWN_LOCATION)
                && PreferenceManager.ContainsPreference(MatchMode.PREVIOUS_SPAWN_ROTATION))
            {

                var pos = PreferenceManager.GetPreference<float[]>(MatchMode.PREVIOUS_SPAWN_LOCATION);
                var rot = PreferenceManager.GetPreference<float[]>(MatchMode.PREVIOUS_SPAWN_ROTATION);
                
                RobotSimObject.SpawnRobot(_robotFiles[_robotIndex],
                    new Vector3(pos[0], pos[1], pos[2]),
                    new Quaternion(rot[0], rot[1], rot[2], rot[3]).normalized);
            }
            else
            {
                RobotSimObject.SpawnRobot(_robotFiles[_robotIndex]);
            }
            
            
            MatchMode.currentRobotIndex = _robotIndex;
        }
        Scoring.ResetScore();

        DynamicUIManager.CloseActiveModal();

        // DynamicUIManager.CreatePanel<Synthesis.UI.Dynamic.SpawnLocationPanel>();
        DynamicUIManager.CreatePanel<StartMatchModePanel>();
    }
    
    public override void Update() {
        // Shooting.Update();
    }

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
                    b += Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                default:
                    b += a[i];
                    break;
            }
            if (i != a.Length - 1)
                b += Path.AltDirectorySeparatorChar;
        }
        // Debug.Log(b);
        return b;
    }
}

