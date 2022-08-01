using System;
using System.Text.RegularExpressions;
using System.Threading;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using UnityEngine;

public class ChangeInputsModal : ModalDynamic
{
    public ChangeInputsModal() : base(new Vector2(800, 330)) {}

    private static bool RobotLoaded
    {
        get => !RobotSimObject.CurrentlyPossessedRobot.Equals(string.Empty);
    }

    private const float VERTICAL_PADDING = 12f;
    
    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 8, rightPadding: 8);
        return u;
    };

    private Analog _currentlyReassigning;
    private Button _reassigningButton;
    private string _reassigningKey;

    private void PopulateInputSelections()
    {
        (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(400, 0);
        if (RobotLoaded)
        {
            leftContent.CreateLabel()
                .SetText("Robot Controls")
                .ApplyTemplate(VerticalLayout);
            
            var inputScrollView = leftContent.CreateScrollView()
                .SetHeight<ScrollView>(300)
                .ApplyTemplate(VerticalLayout);

            SimObject robot = SimulationManager.SimulationObjects[RobotSimObject.CurrentlyPossessedRobot];
            if (robot == null) return;

            foreach (var inputKey in robot.GetAllReservedInputs())
            {
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
        else
        {
            leftContent.CreateLabel()
                .SetText("No robot loaded.")
                .ApplyTemplate(VerticalLayout);
        }

        rightContent.CreateLabel()
            .SetText("Global Controls")
            .ApplyTemplate(VerticalLayout);

        var globalControlView = rightContent.CreateScrollView()
            .SetHeight<ScrollView>(300)
            .ApplyTemplate(VerticalLayout);

        foreach (var inputKey in InputManager.MappedValueInputs)
        {
            if (inputKey.Key.StartsWith("input/") && !Regex.IsMatch(inputKey.Value.Name, ".*Mouse.*"))
            {
                var val = inputKey.Value;
                
                string capitalized = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
                    Regex.Replace(inputKey.Key.Replace("input/", ""), "[\\W_]", " "));
                globalControlView.Content.CreateLabeledButton().StepIntoLabel(l => l.SetText(capitalized))
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
                            _reassigningKey = inputKey.Key;
                        });
                    })
                    .ApplyTemplate(VerticalLayout);
            }
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