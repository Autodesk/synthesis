using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using SynthesisAPI.EventBus;
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

        public override void Create()
        {
            Title.SetText("Theme Editor");
            Description.SetText("Select and Customize Themes");

            var (left, right) = MainContent.SplitLeftRight(
                500 - (HORIZONTAL_PADDING / 2), HORIZONTAL_PADDING);

            CreateThemeSelection(left);
            CreateColorPicker(left);
            CreateInputFields(right);

            AcceptButton
                .StepIntoLabel(l => l.SetText("Exit"))
                .AddOnClickedEvent(x =>
                {
                    DynamicUIManager.CloseActiveModal();
                    SaveThemeChanges();
                });
            
            CancelButton.RootGameObject.SetActive(false);
        }

        private void CreateInputFields(Content content)
        {
            var (left, rightRegion) = content.SplitLeftRight(
                (content.Size.x / 3f) - (HORIZONTAL_PADDING / 2f), HORIZONTAL_PADDING);
            var (center, right) = rightRegion.SplitLeftRight(
                (content.Size.x / 3f) - (HORIZONTAL_PADDING / 2f), HORIZONTAL_PADDING);
            
            Content[] columns = { left, center, right };

            int i = 0;
            ColorManager.LoadedColors.ForEach(c =>
            {
                var colorContent = columns[i % 3].CreateSubContent(new Vector2(left.Size.x ,ROW_HEIGHT))
                    .ApplyTemplate(VerticalLayout);

                var (colorDisplay, name) = colorContent.SplitLeftRight(ROW_HEIGHT, HORIZONTAL_PADDING);

                var button = colorDisplay
                    .CreateButton()
                    .SetStretch<Button>()
                    .SetBackgroundColor<Button>(c.Value)
                    .StepIntoLabel(l => l.SetText(""))
                    .AddOnClickedEvent(x =>
                    {
                        SelectColor(c.Key);
                    });

                var inputField = name
                    .CreateInputField()
                    .SetAnchoredPosition<InputField>(Vector3.zero)
                    .StepIntoLabel(l => l.SetText(
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
        
        private int _selectedThemeIndex;
        private string[] _availableThemes = ColorManager.AvailableThemes;

        private void CreateThemeSelection(Content content)
        {
            GetThemePref();
            
            float padding = 15;

            content.CreateLabel()
                .ApplyTemplate(VerticalLayout)
                .SetText("Select a Theme");
            
            var dropdown = content
                .CreateDropdown()
                .ApplyTemplate(VerticalLayout)
                .SetOptions(ColorManager.AvailableThemes)
                .SetValue(_selectedThemeIndex)
                .AddOnValueChangedEvent((dropdown, index, data) =>
                {
                    SelectTheme(index);
                });

            var (left, right) = content
                .CreateSubContent(new Vector2(content.Size.x, 50))
                .ApplyTemplate(VerticalLayout).SplitLeftRight((content.Size.x-padding)/2f, padding);

            var addThemeButton = left.CreateButton()
                .ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Create Theme"))
                .AddOnClickedEvent(b =>
                {
                    DynamicUIManager.CreateModal<NewThemeModal>();
                });

            var deleteThemeButton = right.CreateButton()
                .ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Delete Theme"))
                .AddOnClickedEvent(b =>
                {
                    DynamicUIManager.CreateModal<DeleteThemeModal>();
                });
        }
        
        private void CreateColorPicker(Content content)
        {
            var gap = content.CreateSubContent(new Vector2(content.Size.x, 50))
                .ApplyTemplate(VerticalLayout);

            _colorPickerLabel = content.CreateLabel()
                .ApplyTemplate(VerticalLayout)
                .SetText("Select a Color to Customize");
            
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

            _rSlider.SetValue(colorInfo.color.r);
            _gSlider.SetValue(colorInfo.color.g);
            _bSlider.SetValue(colorInfo.color.b);
        }

        private void GetThemePref() => _selectedThemeIndex = ColorManager.ThemeNameToIndex(
            PreferenceManager.GetPreference<string>(ColorManager.SELECTED_THEME_PREF));
        
        private void SetThemePref() => PreferenceManager.SetPreference(
            ColorManager.SELECTED_THEME_PREF, ColorManager.AvailableThemes[_selectedThemeIndex]);

        private void SelectTheme(int index)
        {
            SaveThemeChanges();
            _selectedThemeIndex = index;

            SetThemePref();

            EventBus.Push(new SelectedThemeChanged());

            ColorManager.LoadedColors.ForEach(c =>
            {
                var valueTuple = _colors[c.Key];
                valueTuple.color = c.Value;
                valueTuple.button.SetBackgroundColor<Button>(c.Value);
                _colors[c.Key] = valueTuple;
            });
        }

        private void SaveThemeChanges()
        {
            List<(ColorManager.SynthesisColor name, Color32 color)> colors = new();
            _colors.ForEach(c => { colors.Add((c.Key, c.Value.color)); });
            ColorManager.ModifySelectedTheme(colors);
        }

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
        
        public override void Delete() {}

        public class SelectedThemeChanged : IEvent {}
    }
}
