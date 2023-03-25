using System;
using System.Text.RegularExpressions;
using System.Threading;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using TMPro;
using UnityEngine;

public class ChangeInputsModal : ModalDynamic
{
    public ChangeInputsModal() : base(new Vector2(1200, CONTENT_HEIGHT + 30)) {}

    private static bool RobotLoaded {
        get => !RobotSimObject.CurrentlyPossessedRobot.Equals(string.Empty);
    }

    private const float VERTICAL_PADDING = 16f;
    private const float TITLE_INDENT = 10f;
    private const int CONTENT_HEIGHT = 400;
    
    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
        return u;
    };

    private Analog _currentlyReassigning;
    private Button _reassigningButton;
    private string _reassigningKey;

    private void PopulateInputSelections()
    {
        (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(580, 20);
        if (RobotLoaded)
        {
            leftContent.CreateLabel()
                .SetText("Robot Controls")
                .ApplyTemplate(VerticalLayout)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                .SetTopStretch(leftPadding: 0f/*TITLE_INDENT*/);
            
            var inputScrollView = leftContent.CreateScrollView()
                .SetHeight<ScrollView>(CONTENT_HEIGHT)
                .ApplyTemplate(VerticalLayout);

            // make background transparent
            inputScrollView.RootGameObject.GetComponent<UnityEngine.UI.Image>().color = Color.clear;

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
            var noRobotLoadedLabel = leftContent.CreateLabel()
                .SetText("No robot loaded.");
            
            TMP_Text text = noRobotLoadedLabel.RootGameObject.GetComponent<TMP_Text>();

            text.alignment = TextAlignmentOptions.Center;
            
            noRobotLoadedLabel
                .SetTopStretch(anchoredY: CONTENT_HEIGHT / 2);
        }

        rightContent.CreateLabel()
            .SetText("Global Controls")
            .ApplyTemplate(VerticalLayout)
            .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
            .SetTopStretch(leftPadding: 0f/*TITLE_INDENT*/);

        var globalControlView = rightContent.CreateScrollView()
            .SetHeight<ScrollView>(CONTENT_HEIGHT)
            .ApplyTemplate(VerticalLayout);
        
        globalControlView.RootGameObject.GetComponent<UnityEngine.UI.Image>().color = Color.clear;


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
        AcceptButton.AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal()).StepIntoLabel(l => l.SetText("Close"));
        CancelButton.RootGameObject.SetActive(false);
        
        PopulateInputSelections();
    }

    public override void Update()
    {
        if (_currentlyReassigning != null)
        {
            var input = InputManager.GetAny();
            if (input != null && Mathf.Abs(input.Value) < 0.15f)
                input = null;

            // if we allow mouse inputs the input will always get set to Mouse0
            // because the user clicks on the button
            if (input != null && !Regex.IsMatch(input.Name, ".*Mouse.*"))
            {
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