using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Simulation;
using TMPro;
using UnityEngine;
using Utilities.ColorManager;

public class ChangeInputsModal : ModalDynamic {
    public ChangeInputsModal() : base(new Vector2(1200, CONTENT_HEIGHT + 30)) {}

    private static bool RobotLoaded => MainHUD.SelectedRobot != null;

    private const float VERTICAL_PADDING = 6f;
    private const float TITLE_INDENT     = 10f;
    private const int CONTENT_HEIGHT     = 400;

    private const float ENTRY_HEIGHT        = 46f;
    private const float ENTRY_RIGHT_PADDING = 5f;

    private readonly Func<UIComponent, UIComponent> VerticalLayout = (u) => {
        var offset = u.Parent!.ChildrenReadOnly.Count > 1 ? (-u.Parent!.RectOfChildren(u).yMin) + VERTICAL_PADDING : 0f;
        u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 0, rightPadding: 0);
        return u;
    };

    private Analog _currentlyReassigning;
    private Button _reassigningButton;
    private string _reassigningKey;
    private Dictionary<string, Analog> _changedInputs;
    public Boolean isSave = false;

    private void PopulateInputSelections() {
        (Content leftContent, Content rightContent) = MainContent.SplitLeftRight(580, 20);
        if (RobotLoaded) {
            leftContent.CreateLabel()
                .SetText("Robot Controls")
                .ApplyTemplate(VerticalLayout)
                .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
                .SetTopStretch(leftPadding: 0f /*TITLE_INDENT*/);

            var inputScrollView =
                leftContent.CreateScrollView().SetHeight<ScrollView>(CONTENT_HEIGHT).ApplyTemplate(VerticalLayout);

            // make background transparent
            inputScrollView.RootGameObject.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
            if (MainHUD.SelectedRobot != null) {
                foreach (var inputKey in MainHUD.SelectedRobot.GetAllReservedInputs()) {
                    var val = InputManager.MappedValueInputs[inputKey.key];

                    var item = inputScrollView.Content.CreateLabeledButton()
                                   .SetHeight<LabeledButton>(ENTRY_HEIGHT)
                                   .StepIntoLabel(l => l.SetText(inputKey.displayName))
                                   .StepIntoButton(b => {
                                       b.SetRightStretch<Button>(anchoredX: ENTRY_RIGHT_PADDING).SetWidth<Button>(200);
                                       UpdateUI(b, val, val is Digital);
                                       b.AddOnClickedEvent(
                                           _ => {
                                               // handle changing input keybind here
                                               b.StepIntoLabel(l => l.SetText("Press anything"));
                                               _currentlyReassigning = val;
                                               _reassigningButton    = b;
                                               _reassigningKey       = inputKey.key;
                                           });
                                   })
                                   .ApplyTemplate(VerticalLayout);
                }

                inputScrollView.Content.SetHeight<Content>(Mathf.Abs(inputScrollView.Content.RectOfChildren().yMin));
            }
        } else {
            var noRobotLoadedLabel = leftContent.CreateLabel().SetText("No robot loaded.");

            TMP_Text text = noRobotLoadedLabel.RootGameObject.GetComponent<TMP_Text>();

            text.alignment = TextAlignmentOptions.Center;

            noRobotLoadedLabel.SetTopStretch(anchoredY: CONTENT_HEIGHT / 2);
        }

        rightContent.CreateLabel()
            .SetText("Global Controls")
            .ApplyTemplate(VerticalLayout)
            .SetHorizontalAlignment(HorizontalAlignmentOptions.Center)
            .SetTopStretch(leftPadding: 0f /*TITLE_INDENT*/);

        var globalControlView =
            rightContent.CreateScrollView().SetHeight<ScrollView>(CONTENT_HEIGHT).ApplyTemplate(VerticalLayout);

        globalControlView.RootGameObject.GetComponent<UnityEngine.UI.Image>().color = Color.clear;

        foreach (var inputKey in InputManager.MappedValueInputs) {
            if (inputKey.Key.StartsWith("input/") && !Regex.IsMatch(inputKey.Value.Name, ".*Mouse.*")) {
                var val = inputKey.Value;

                string capitalized = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(
                    Regex.Replace(inputKey.Key.Replace("input/", ""), "[\\W_]", " "));
                globalControlView.Content.CreateLabeledButton()
                    .SetHeight<LabeledButton>(ENTRY_HEIGHT)
                    .StepIntoLabel(l => l.SetText(capitalized))
                    .StepIntoButton(b => {
                        b.SetRightStretch<Button>(anchoredX: ENTRY_RIGHT_PADDING).SetWidth<Button>(200);
                        UpdateUI(b, val, val is Digital);
                        b.AddOnClickedEvent(
                            _ => {
                                // handle changing input keybind here
                                b.StepIntoLabel(l => l.SetText("Press anything"));
                                _currentlyReassigning = val;
                                _reassigningButton    = b;
                                _reassigningKey       = inputKey.Key;
                            });
                    })
                    .ApplyTemplate(VerticalLayout);
            }
        }

        globalControlView.Content.SetHeight<Content>(-globalControlView.Content.RectOfChildren().yMin);
    }

    private void UpdateUI(Button button, Analog input, bool isDigital) {
        AcceptButton.StepIntoLabel(l => l.SetText($"Save ({_changedInputs.Count})"));
        MiddleButton.StepIntoLabel(l => l.SetText($"Session Save ({_changedInputs.Count})"));
        var l = button.Label;
        if (l == null)
            return;
        // don't show positive or negative if the input is digital
        var text = (!isDigital ? input.UsePositiveSide ? "(+) " : "(-) " : "") + input.Name;

        var modifier = input.Modifier;
        if ((modifier & (int) ModKey.LeftShift) != 0) {
            text += " + Left Shift";
        }
        if ((modifier & (int) ModKey.LeftCommand) != 0) {
            text += " + Left Command";
        }
        if ((modifier & (int) ModKey.LeftAlt) != 0) {
            text += " + Left Alt";
        }
        if ((modifier & (int) ModKey.RightShift) != 0) {
            text += " + Right Shift";
        }
        if ((modifier & (int) ModKey.RightCommand) != 0) {
            text += " + Right Command";
        }
        if ((modifier & (int) ModKey.RightAlt) != 0) {
            text += " + Right Alt";
        }
        if ((modifier & (int) ModKey.LeftControl) != 0) {
            text += " + Left Control";
        }
        if ((modifier & (int) ModKey.RightControl) != 0) {
            text += " + Right Control";
        }

        l.SetText(text);
    }

    public override void Create() {
        _changedInputs = new Dictionary<string, Analog>();
        Title.SetText("Keybinds");

        ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("settings"));

        AcceptButton
            .AddOnClickedEvent(b => {
                isSave = true;
                _changedInputs.ForEach(x => {
                    InputManager.AssignValueInput(x.Key, x.Value);
                    if (x.Value is Digital) {
                        PreferenceManager.SetPreference<Digital>(x.Key, x.Value as Digital);
                        PreferenceManager.Save();
                    }
                });
                DynamicUIManager.CloseActiveModal();
            })
            .StepIntoLabel(l => l.SetText("Save"));
        CancelButton.AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal());
        MiddleButton
            .AddOnClickedEvent(b => {
                isSave = false;
                _changedInputs.ForEach(x => {
                    InputManager.AssignValueInput(x.Key, x.Value);
                });
                DynamicUIManager.CloseActiveModal();
            })
            .StepIntoLabel(l => l.SetText("Session"))
            .SetWidth<Button>(140);

        PopulateInputSelections();
    }

    public override void Update() {
        if (_currentlyReassigning != null) {
            var input = InputManager.GetAny();
            if (input != null && Mathf.Abs(input.Value) < 0.15f)
                input = null;

            // if we allow mouse inputs the input will always get set to Mouse0
            // because the user clicks on the button
            if (input != null && !Regex.IsMatch(input.Name, ".*Mouse.*")) {
                if (_changedInputs.ContainsKey(_reassigningKey))
                    _changedInputs.Remove(_reassigningKey);
                _changedInputs.Add(_reassigningKey, input);
                
                UpdateUI(_reassigningButton, input, input is Digital);
                _reassigningButton.Parent.SetBackgroundColor<Content>(ColorManager.SynthesisColor.BackgroundSecondary);

                

                _currentlyReassigning = null;
                _reassigningButton    = null;
                _reassigningKey       = null;
            }
        }
    }

    public override void Delete() {}
}