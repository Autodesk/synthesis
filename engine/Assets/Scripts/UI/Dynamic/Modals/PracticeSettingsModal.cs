using System;
using System.Collections.Generic;
using System.Linq;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class PracticeSettingsModal : ModalDynamic
{

    private static Dictionary<string, GamepieceSimObject> _gamepieceMap = new Dictionary<string, GamepieceSimObject>();
    private static List<GamepieceSimObject> _gamepieceSimObjects = new List<GamepieceSimObject>();
    
    public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 10f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0f); // used to be 15f
        return u;
    };
    
    public PracticeSettingsModal() : base(new Vector2(500, 300))
    {
        
    }

    public override void Create()
    {
        Title.SetText("Practice Settings");
        Description.SetText("Configuration actions for practice mode");
        
        AcceptButton.RootGameObject.SetActive(false);
        CancelButton.Label.SetText("Close");

        float leftRightPadding = 8;
        (Content leftContent, Content rightContent) = MainContent.SplitLeftRight((MainContent.Size.x - leftRightPadding) / 2, leftRightPadding);

        var spawnButton = rightContent.CreateButton()
            .StepIntoLabel(l => l.SetText("Spawn"))
            .ApplyTemplate(VerticalLayout)
            .AddOnClickedEvent(b => ModeManager.SpawnGamepiece(1f, PracticeMode.ChosenPrimitive));
        
        // TODO dropdown is slightly wider than other buttons
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
                ModeManager.ConfigureGamepieceSpawnpoint();
            });

        leftContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset All"))
            .AddOnClickedEvent(b => ModeManager.ResetAll());
        
        rightContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Robot"))
            .AddOnClickedEvent(b => ModeManager.ResetRobot());

        leftContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Gamepieces"))
            .AddOnClickedEvent(b => ModeManager.ResetGamepieces());

        rightContent.CreateButton()
            .ApplyTemplate(VerticalLayout)
            .StepIntoLabel(label => label.SetText("Reset Field"))
            .AddOnClickedEvent(b => ModeManager.ResetField());
    }
    
    public override void Update(){}
    
    public override void Delete(){}
}