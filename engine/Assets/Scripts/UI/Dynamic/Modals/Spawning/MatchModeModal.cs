using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modes.MatchMode;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class MatchModeModal : ModalDynamic {
    private int _fieldIndex            = -1;
    private List<String> _robotOptions = new List<string>();
    private string[] _fieldFiles;
    
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 15f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
        return u;
    };

    public MatchModeModal() : base(new Vector2(500, 600)) {}

    public override void Create() {
        Title.SetText("Field and Robot Selection");
        
        ModalImage.SetSprite(SynthesisAssetCollection.GetSpriteByName("wrench-icon"));

        var robotsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira", '/');
        if (!Directory.Exists(robotsFolder))
            Directory.CreateDirectory(robotsFolder);
        var fieldsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira/Fields", '/');
        if (!Directory.Exists(fieldsFolder))
            Directory.CreateDirectory(fieldsFolder);

        var _robotFiles = Directory.GetFiles(robotsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
        _robotOptions.Add("None");
        _robotFiles.ForEach(x => _robotOptions.Add(x));

        _fieldFiles = Directory.GetFiles(fieldsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

        AcceptButton.StepIntoLabel(label => label.SetText("Load")).AddOnClickedEvent(b => {
            if (_fieldIndex != -1) {
                DynamicUIManager.CreateModal<LoadingScreenModal>();
                MonoBehaviour _mb = GameObject.FindObjectOfType<MonoBehaviour>();
                if (_mb != null) {
                    _mb.StartCoroutine(LoadMatch());
                }
            }
        });

        for (int robot = 0; robot < 6; robot++) {
            int robotIndex = robot;
            if (robotIndex % 3 == 0)
                MainContent.CreateLabel()
                    .ApplyTemplate(VerticalLayout)
                    .SetText($"Select {(robotIndex == 0 ? "Red" : "Blue")} Robots");

            MainContent.CreateDropdown()
                .SetOptions(_robotOptions.Select(x => Path.GetFileName(x)).ToArray())
                .AddOnValueChangedEvent((d, i, data) => MatchMode.SelectedRobots[robotIndex] =
                                            i - 1 // Subtract 1 to account for "None" option
                    )
                .ApplyTemplate(VerticalLayout)
                .SetValue(0);
        }

        MainContent.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select Field");
        var chooseFieldDropdown = MainContent.CreateDropdown()
                                      .SetOptions(_fieldFiles.Select(x => Path.GetFileName(x)).ToArray())
                                      .AddOnValueChangedEvent((d, i, data) => _fieldIndex = i)
                                      .ApplyTemplate(VerticalLayout);

        _fieldIndex = _fieldFiles.Length > 0 ? 0 : -1;
    }

    public IEnumerator LoadMatch() {
        yield return new WaitForSeconds(0.05f);

        if (FieldSimObject.CurrentField != null)
            FieldSimObject.DeleteField();

        FieldSimObject.SpawnField(_fieldFiles[_fieldIndex], false);

        DynamicUIManager.CloseActiveModal();
        DynamicUIManager.CreatePanel<SpawnLocationPanel>(true);
    }

    public override void Update() {}

    public override void Delete() {}

    public static string ParsePath(string p, char c) {
        string[] a = p.Split(c);
        string b   = "";
        for (int i = 0; i < a.Length; i++) {
            switch (a[i]) {
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
        return b;
    }
}
