using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring
{
    public class EditThemeModal : ModalDynamic
    {
        private const float MODAL_WIDTH = 1350;
        private const float MODAL_HEIGHT = 500;
        private const float ROW_HEIGHT = 60;
        private const float HORIZONTAL_PADDING = 15;
        
        public EditThemeModal()
            : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        private Dictionary<ColorManager.SynthesisColor, 
                (Color32 color, Button button, InputField inputField)> _colors = new();
        
        private ColorManager.SynthesisColor? _selectedColor = null;

        public override void Create() {
            Title.SetText("Theme Editor");
            Description.SetText($"Customize Theme {ColorManager.SelectedTheme}");
            
            var (left, right) = MainContent.SplitLeftRight(
                MODAL_WIDTH - 500 - (HORIZONTAL_PADDING / 2), HORIZONTAL_PADDING);

            CreateInputFields(left);
            CreateColorPicker(right);

            AcceptButton.AddOnClickedEvent(x =>
            {
                List<(ColorManager.SynthesisColor name, Color32 color)> colors = new();
                _colors.ForEach(c =>
                {
                    colors.Add((c.Key, c.Value.color));
                });
                
                ColorManager.ModifyLoadedTheme(colors);
                DynamicUIManager.CloseActiveModal();
            });
        }

        private void CreateInputFields(Content content)
        {
            var (left, rightRegion) = content.SplitLeftRight(
                (content.Size.x / 3f) - (HORIZONTAL_PADDING / 2f), HORIZONTAL_PADDING);
            var (middle, right) = rightRegion.SplitLeftRight(
                (content.Size.x / 3f) - (HORIZONTAL_PADDING / 2f), HORIZONTAL_PADDING);
            
            var (A, B) = left.SplitLeftRight(ROW_HEIGHT, HORIZONTAL_PADDING);
            var (C, D) = middle.SplitLeftRight(ROW_HEIGHT, HORIZONTAL_PADDING);
            var (E, F) = right.SplitLeftRight(ROW_HEIGHT, HORIZONTAL_PADDING);

            Content[] columns = { A, B, C, D, E, F };

            int i = 0;
            ColorManager.LoadedColors.ForEach(c =>
            {
                var column = i % 3;

                var button = columns[column*2].CreateSubContent(new Vector2(ROW_HEIGHT, ROW_HEIGHT))
                    .ApplyTemplate(VerticalLayout)
                    .CreateButton()
                    .ApplyTemplate(VerticalLayout)
                    .SetStretch<Button>()
                    .SetBackgroundColor<Button>(c.Value)
                    .StepIntoLabel(l => l.SetText(""))
                    .AddOnClickedEvent(x =>
                    {
                        SelectColor(c.Key);
                    });

                var inputField = columns[column * 2 + 1]
                    .CreateSubContent(new Vector2(columns[column * 2 + 1].Size.x, ROW_HEIGHT))
                    .ApplyTemplate(VerticalLayout)
                    .CreateInputField()
                    .SetAnchoredPosition<InputField>(Vector3.zero)
                    .StepIntoLabel(l => l.SetText(
                        // Adds spaces to pascal case ("ExampleColor" -> "Example Color")
                        Regex.Replace(c.Key.ToString(), "(\\B[A-Z])", " $1")))
                    .SetCharacterLimit(7)
                    .SetValue(((Color)c.Value).ToHex());
                
                _colors.Add(c.Key, (c.Value, button, inputField));
                
                i++;
            });
        }

        private Label _colorPickerLabel;
        private Slider _rSlider;
        private Slider _gSlider;
        private Slider _bSlider;

        private void CreateColorPicker(Content content)
        {
            _colorPickerLabel = content.CreateLabel()
                .ApplyTemplate(VerticalLayout)
                .SetText("No Color Selected");
            
            _rSlider = content.CreateSlider()
                .ApplyTemplate(VerticalLayout)
                .StepIntoTitleLabel(l => l.SetText("Red"))
                .SetRange(0, 255);
            _gSlider = content.CreateSlider()
                .ApplyTemplate(VerticalLayout)
                .StepIntoTitleLabel(l => l.SetText("Green"))
                .SetRange(0, 255);
            _bSlider = content.CreateSlider()
                .ApplyTemplate(VerticalLayout)
                .StepIntoTitleLabel(l => l.SetText("Blue"))
                .SetRange(0, 255);
        }

        public void SelectColor(ColorManager.SynthesisColor colorName)
        {
            _selectedColor = colorName;
            
            var colorInfo = _colors[colorName];

            _colorPickerLabel.SetText(Regex.Replace(colorName.ToString(), "(\\B[A-Z])", " $1"));

            //var color = colorInfo.Value;
            _rSlider.SetValue(colorInfo.color.r);
            _gSlider.SetValue(colorInfo.color.g);
            _bSlider.SetValue(colorInfo.color.b);
        }

        public override void Delete() {}

        public override void Update()
        {
            if (_selectedColor == null)
                return;

            Color32 colorInput = new Color32((byte)_rSlider.Value, (byte)_gSlider.Value, (byte)_bSlider.Value, 255);
            
            var valueTuple = _colors[_selectedColor.Value];
            valueTuple.button.SetBackgroundColor<Button>(colorInput);
            valueTuple.color = colorInput;

            _colors[_selectedColor.Value] = (colorInput, valueTuple.button, valueTuple.inputField);
        }
    }
}
