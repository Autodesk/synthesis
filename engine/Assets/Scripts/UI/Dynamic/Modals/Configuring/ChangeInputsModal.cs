using System;
using System.Text.RegularExpressions;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEngine;

public class ChangeInputsModal : ModalDynamic
{
    public ChangeInputsModal() : base(new Vector2(400, 400)) {}
    
    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + 12f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 8, rightPadding: 8);
        return u;
    };

    private Analog _currentlyReassigning;
    private Button _reassigningButton;
    private string _reassigningKey;

    private void PopulateInputSelections()
    {
        if (RobotSimObject.CurrentlyPossessedRobot.Equals(string.Empty))
        {
            MainContent.CreateLabel().SetText("No robot loaded.");
            return;
        }

        var inputScrollView = MainContent.CreateScrollView()
            .SetHeight<ScrollView>(400);

        SimObject robot = SimulationManager.SimulationObjects[RobotSimObject.CurrentlyPossessedRobot];
        if (robot == null) return;
        
        foreach (var inputKey in robot.GetAllReservedInputs()) {
            var val = InputManager.MappedValueInputs[inputKey];

            inputScrollView.Content.CreateLabeledButton()
                .StepIntoLabel(l => l.SetText(inputKey))
                .StepIntoButton(b =>
                {
                    b.SetHeight<Button>(8)
                        .SetWidth<Button>(200);
                    UpdateAnalogInputButton(b, val, val is Digital);
                    b.AddOnClickedEvent(_ =>
                    {
                        // handle changing input keybind here
                        b.StepIntoLabel(l => l.SetText("Press anything"));
                        _currentlyReassigning = val;
                        _reassigningButton = b;
                        _reassigningKey = inputKey;
                    });
                })
                .ApplyTemplate(VerticalLayout);
        }
    }

    private void UpdateAnalogInputButton(Button button, Analog input, bool isDigital)
    {
        var l = button.Label;
        if (l == null) return;
        // don't show positive or negative if the input is digital
        var text = (!isDigital ? input.UsePositiveSide ? "(+) " : "(-) " : "") + input.Name;
        
        var modifier = input.Modifier;
        if ((modifier & (int)ModKey.LeftShift) != 0) {
            text += " + Left Shift";
        }
        if ((modifier & (int)ModKey.LeftCommand) != 0) {
            text += " + Left Command";
        }
        if ((modifier & (int)ModKey.LeftAlt) != 0) {
            text += " + Left Alt";
        }
        if ((modifier & (int)ModKey.RightShift) != 0) {
            text += " + Right Shift";
        }
        if ((modifier & (int)ModKey.RightCommand) != 0) {
            text += " + Right Command";
        }
        if ((modifier & (int)ModKey.RightAlt) != 0) {
            text += " + Right Alt";
        }
        if ((modifier & (int)ModKey.LeftControl) != 0) {
            text += " + Left Control";
        }
        if ((modifier & (int)ModKey.RightControl) != 0) {
            text += " + Right Control";
        }

        l.SetText(text);
    }
    

    public override void Create()
    {
        Title.SetText("Keybinds");
        Description.SetText("Configure keybinds for your robot.");
        
        // no cancel button because keybinds are saved automatically when set
        AcceptButton.AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal());
        CancelButton.RootGameObject.SetActive(false);
        
        PopulateInputSelections();
    }

    public override void Update()
    {
        if (_currentlyReassigning != null)
        {
            var input = InputManager.GetAny();

            // if we allow mouse inputs the input will always get set to Mouse0
            // because the user clicks on the button
            if (input != null && !Regex.IsMatch(input.Name, ".*Mouse.*"))
            {
                RobotSimObject robot = SimulationManager.SimulationObjects[RobotSimObject.CurrentlyPossessedRobot] as RobotSimObject;
                if (robot == null) return;

                InputManager.AssignValueInput(_reassigningKey, input);
                
                UpdateAnalogInputButton(_reassigningButton, input, input is Digital);

                _currentlyReassigning = null;
                _reassigningButton = null;
                _reassigningKey = null;
            }
        }
    }

    public override void Delete()
    {
        
    }
}