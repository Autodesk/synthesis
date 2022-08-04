using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class PracticeSettingsModal : ModalDynamic
{

    private static Dictionary<string, PracticeMode.GamepieceData> _gamepieceMap = new Dictionary<string, PracticeMode.GamepieceData>();
    private static List<PracticeMode.GamepieceData> _gamepieceSimObjects = new List<PracticeMode.GamepieceData>();
    
    private static string lastField = "";

    private const float VERTICAL_PADDING = 10f;
    
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
        return u;
    };
    
    public PracticeSettingsModal() : base(new Vector2(500, 315)) { }

    public override void Create()
    {
        Title.SetText("Practice Settings");
        // Description.SetText("Configuration actions for practice mode");
        
        AcceptButton.RootGameObject.SetActive(false);
        CancelButton
            // .SetAnchoredPosition<Button>(new Vector2(20f, CancelButton.RootRectTransform.anchoredPosition.y))
            .StepIntoLabel(l => l.SetText("Close"))
            .AddOnClickedEvent(b =>
            {
                ModeManager.ModalClosed();
            });

        var gamepieceLabel = MainContent.CreateLabel()
            .SetText("Gamepiece Spawning")
            .ApplyTemplate(Label.BigLabelTemplate)
            .ApplyTemplate(VerticalLayout);

        float leftRightPadding = 8;
        // float leftWidth = (MainContent.Size.x - leftRightPadding) / 2;
        // (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(leftWidth, leftRightPadding);

        var top = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 110f))
            .ApplyTemplate(VerticalLayout)
            .SetPivot<Content>(new Vector2(0.5f, 1f));

        (Content topleft, Content topRight) = top.SplitLeftRight((top.Size.x - leftRightPadding) / 2f, leftRightPadding);
        
        // rightContent.SetTopStretch<Content>(leftWidth + leftRightPadding, 0, anchoredY: gamepieceLabel.Size.y + VERTICAL_PADDING);

        var spawnButton = topRight.CreateButton()
            .StepIntoLabel(l => l.SetText("Spawn"))
            .SetTopStretch<Button>()
            .ShiftOffsetMax<Button>(new Vector2(-7.5f, 0f))
            .AddOnClickedEvent(b => PracticeMode.SpawnGamepiece(1f, PracticeMode.ChosenPrimitive));
        
        var gamepieceDropdown = topleft.CreateDropdown()
            .SetHeight<Dropdown>(spawnButton.Size.y)
            .SetTopStretch<Dropdown>()
            .ShiftOffsetMin<Dropdown>(new Vector2(7.5f, 0f));
        
        FieldSimObject field = FieldSimObject.CurrentField;

        if (field == null)
        {
            string[] primitives = Enum.GetNames(typeof(PrimitiveType));
            gamepieceDropdown.SetOptions(primitives)
                .SetValue(0)
                .AddOnValueChangedEvent((d, i, data) =>
                {
                    PracticeMode.ChosenPrimitive = (PrimitiveType) Enum.Parse(typeof(PrimitiveType), data.text);
                });
            PracticeMode.ChosenPrimitive = (PrimitiveType) Enum.Parse(typeof(PrimitiveType), primitives[0]);
        }
        else
        {
            if (field.MiraAssembly.Info.GUID != lastField)
            {
                _gamepieceSimObjects.Clear();
                _gamepieceMap.Clear();
                lastField = field.MiraAssembly.Info.GUID;
            }
            if (_gamepieceSimObjects.Count == 0 && field.Gamepieces.Count > 0) {
                // group the gamepieces by their types, found via the name of the first child before :
                var groups = field.Gamepieces.GroupBy(g =>
                {
                    if (g.GamepieceObject.transform.childCount > 0)
                        return g.GamepieceObject.transform.GetChild(0).name.Split(':')[0];
                    return g.GamepieceObject.name;
                });
                var uniqueGamepieces = groups.Select(g => g.First()).ToList();
                
                _gamepieceSimObjects = uniqueGamepieces.Map(g => new PracticeMode.GamepieceData(g.GamepieceObject));
                _gamepieceSimObjects.ForEach(g => _gamepieceMap.Add(g.Name, g));
            }
            gamepieceDropdown.SetOptions(_gamepieceSimObjects.Map(g => g.Name).ToArray())
                .SetValue(0)
                
                .AddOnValueChangedEvent((d, i, data) =>
                {
                    PracticeMode.ChosenGamepiece = _gamepieceMap[data.text];
                });
            PracticeMode.ChosenGamepiece = _gamepieceMap.First().Value;
        }

        topleft.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
            .StepIntoLabel(l => l.SetText("Gamepiece Spawnpoint"))
            .AddOnClickedEvent(b =>
            {
                DynamicUIManager.CloseActiveModal();
                ModeManager.ModalClosed();
                PracticeMode.ConfigureGamepieceSpawnpoint();
            });

        var resetLabel = MainContent.CreateLabel()
            .SetText("Reset Gamepieces")
            .ApplyTemplate(Label.BigLabelTemplate)
            .ApplyTemplate(VerticalLayout);

        var bottom = MainContent.CreateSubContent(new Vector2(MainContent.Size.x, 110f))
            .ApplyTemplate(VerticalLayout)
            .SetPivot<Content>(new Vector2(0.5f, 1f));

        (Content bottomLeft, Content bottomRight) = bottom.SplitLeftRight((bottom.Size.x - leftRightPadding) / 2f, leftRightPadding);

        bottomLeft.CreateButton()
            .SetTopStretch<Button>()
            .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
            .StepIntoLabel(label => label.SetText("Reset All"))
            .AddOnClickedEvent(b => PracticeMode.ResetAll());
        
        bottomLeft.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .ShiftOffsetMin<Button>(new Vector2(7.5f, 0f))
            .StepIntoLabel(label => label.SetText("Reset Gamepieces"))
            .AddOnClickedEvent(b => PracticeMode.ResetGamepieces());
        bottomRight.CreateButton()
            .SetTopStretch<Button>()
            .ShiftOffsetMax<Button>(new Vector2(-7.5f, 0f))
            .StepIntoLabel(label => label.SetText("Reset Robot"))
            .AddOnClickedEvent(b => PracticeMode.ResetRobot());
            // .SetTopStretch<Button>(0, 0, VERTICAL_PADDING + (spawnButton.Size.y + VERTICAL_PADDING) * 2 + resetLabel.Size.y + VERTICAL_PADDING);

        bottomRight.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .ShiftOffsetMax<Button>(new Vector2(-7.5f, 0f))
            .StepIntoLabel(label => label.SetText("Reset Field"))
            .AddOnClickedEvent(b => PracticeMode.ResetField());
    }
    
    public override void Update(){}
    
    public override void Delete(){}
}