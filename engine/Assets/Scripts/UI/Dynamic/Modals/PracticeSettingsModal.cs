using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class PracticeSettingsModal : ModalDynamic
{

    private static Dictionary<string, GamepieceSimObject> _gamepieceMap = new Dictionary<string, GamepieceSimObject>();
    private static List<GamepieceSimObject> _gamepieceSimObjects = new List<GamepieceSimObject>();

    private const float VERTICAL_PADDING = 10f;
    
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
        return u;
    };
    
    public PracticeSettingsModal() : base(new Vector2(500, 315))
    {
        
    }

    public override void Create()
    {
        Title.SetText("Practice Settings");
        Description.SetText("Configuration actions for practice mode");
        
        AcceptButton.RootGameObject.SetActive(false);
        CancelButton
            .StepIntoLabel(l => l.SetText("Close"))
            .AddOnClickedEvent(b =>
            {
                ModeManager.ModalClosed();
            });

        float leftRightPadding = 8;
        float leftWidth = (MainContent.Size.x - leftRightPadding) / 2;
        (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(leftWidth, leftRightPadding);
        
        var gamepieceLabel = leftContent.CreateLabel()
            .SetText("Gamepiece Spawning")
            .ApplyTemplate(Label.BigLabelTemplate)
            .ApplyTemplate(VerticalLayout);
        
        rightContent.SetTopStretch<Content>(leftWidth + leftRightPadding, 0, anchoredY: gamepieceLabel.Size.y + VERTICAL_PADDING);

        var spawnButton = rightContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Spawn"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => ModeManager.SpawnGamepiece(1f, PracticeMode.ChosenPrimitive));
        
        var gamepieceDropdown = leftContent.CreateDropdown()
            .ApplyTemplate(VerticalLayout)
            .SetHeight<Dropdown>(spawnButton.Size.y)
            .SetWidth<Dropdown>(spawnButton.Size.x);
        
        FieldSimObject field = FieldSimObject.CurrentField;

        if (field == null)
        {
            gamepieceDropdown.SetOptions(Enum.GetNames(typeof(PrimitiveType)))
                .AddOnValueChangedEvent((d, i, data) =>
                {
                    PracticeMode.ChosenPrimitive = (PrimitiveType) Enum.Parse(typeof(PrimitiveType), data.text);
                });
        }
        else
        {
            if (_gamepieceSimObjects.Count == 0 && field.Gamepieces.Count > 0) {
                _gamepieceSimObjects = field.Gamepieces
                    .GroupBy(g => g.Name).Select(g => g.First()).ToList();
                _gamepieceSimObjects.ForEach(g => _gamepieceMap.Add(g.Name, g));
            }
            gamepieceDropdown.SetOptions(_gamepieceSimObjects.Map(g => g.Name).ToArray())
                .AddOnValueChangedEvent((d, i, data) =>
                {
                    PracticeMode.ChosenGamepiece = _gamepieceMap[data.text];
                });
        }

        leftContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(l => l.SetText("Gamepiece Spawnpoint"))
            .AddOnClickedEvent(b =>
            {
                DynamicUIManager.CloseActiveModal();
                ModeManager.ModalClosed();
                ModeManager.ConfigureGamepieceSpawnpoint();
            });

        var resetLabel = leftContent.CreateLabel()
            .SetText("Reset Gamepieces")
            .ApplyTemplate(Label.BigLabelTemplate)
            .ApplyTemplate(VerticalLayout);

        leftContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset All"))
            .AddOnClickedEvent(b => ModeManager.ResetAll());
        
        leftContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Gamepieces"))
            .AddOnClickedEvent(b => ModeManager.ResetGamepieces());
        rightContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Robot"))
            .AddOnClickedEvent(b => ModeManager.ResetRobot())
            .SetTopStretch<Button>(0, 0, VERTICAL_PADDING + (spawnButton.Size.y + VERTICAL_PADDING) * 2 + resetLabel.Size.y + VERTICAL_PADDING);

        rightContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Field"))
            .AddOnClickedEvent(b => ModeManager.ResetField());
    }
    
    public override void Update(){}
    
    public override void Delete(){}
}