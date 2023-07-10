using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using UnityEngine;
using Utilities.ColorManager;

namespace UI.Dynamic.Modals.Configuring.ThemeEditor {
    /// <summary>
    /// A modal to select, create, remove, and edit themes
    /// </summary>
    public class EditThemeModal : ModalDynamic {
        private const float MODAL_WIDTH        = 1350;
        private const float MODAL_HEIGHT       = 500;
        private const float ROW_HEIGHT         = 60;
        private const float HORIZONTAL_PADDING = 15;

        private Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public class SelectedThemeChanged : IEvent {}

        private string[] _availableThemes = ColorManager.AvailableThemes;

        private Dictionary<ColorManager.SynthesisColor, (Color32 color, Content image, Content background, Label label)>
            _colors = new();

        private ColorManager.SynthesisColor? _selectedColor = null;
        private int _selectedThemeIndex;

        private Label _colorPickerLabel;
        private Slider _rSlider;
        private Slider _gSlider;
        private Slider _bSlider;

        private Button _deleteThemeButton;

        public EditThemeModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        public override void Create() {
            Title.SetText("Theme Editor");
            Description.SetText("Select and Customize Themes");

            AcceptButton.StepIntoLabel(l => l.SetText("Save")).AddOnClickedEvent(x => {
                SaveThemeChanges();
                DynamicUIManager.CloseActiveModal();
            });

            CancelButton.RootGameObject.SetActive(false);

            var (left, right) = MainContent.SplitLeftRight(500 - (HORIZONTAL_PADDING / 2), HORIZONTAL_PADDING);

            CreateThemeSelection(left);
            CreateColorSliders(left);

            CreateColorSelection(right);

            SelectTheme(_selectedThemeIndex);
        }

        public override void Update() {
            if (_selectedColor == null)
                return;

            Color32 colorInput = new Color32((byte) _rSlider.Value, (byte) _gSlider.Value, (byte) _bSlider.Value, 255);

            var valueTuple = _colors[_selectedColor.Value];
            valueTuple.image.SetBackgroundColor<Button>(colorInput);
            valueTuple.color = colorInput;

            _colors[_selectedColor.Value] = (colorInput, valueTuple.image, valueTuple.background, valueTuple.label);
        }

        public override void Delete() {}

        /// <summary>Creates the region on the top left to select, add, or delete a theme</summary>
        /// <param name="content">The region to create the theme selection UI</param>
        private void CreateThemeSelection(Content content) {
            GetThemePref();

            float padding = 15;

            var selectThemeLabel = content.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select a Theme");

            var themeChooser = content.CreateDropdown()
                                   .ApplyTemplate(VerticalLayout)
                                   .SetOptions(ColorManager.AvailableThemes)
                                   .SetValue(_selectedThemeIndex);

            var (addContent, deleteContent) = content.CreateSubContent(new Vector2(content.Size.x, 50))
                                                  .ApplyTemplate(VerticalLayout)
                                                  .SplitLeftRight((content.Size.x - padding) / 2f, padding);

            var addThemeButton = addContent.CreateButton()
                                     .ApplyTemplate(VerticalLayout)
                                     .StepIntoLabel(l => l.SetText("Create Theme"))
                                     .AddOnClickedEvent(b => { DynamicUIManager.CreateModal<NewThemeModal>(); });

            _deleteThemeButton = deleteContent.CreateButton()
                                     .ApplyTemplate(VerticalLayout)
                                     .StepIntoLabel(l => l.SetText("Delete Theme"))
                                     .AddOnClickedEvent(b => {
                                         if (_selectedThemeIndex != 0)
                                             DynamicUIManager.CreateModal<DeleteThemeModal>();
                                     });

            void UpdateDeleteThemeButton() {
                if (_selectedThemeIndex == 0)
                    _deleteThemeButton.DisableEvents<Button>();
                _deleteThemeButton.EnableEvents<Button>();
            }
            UpdateDeleteThemeButton();

            themeChooser.AddOnValueChangedEvent((dropdown, index, data) => {
                SelectTheme(index);
                DynamicUIManager.CreateModal<EditThemeModal>();
                UpdateDeleteThemeButton();
            });
        }

        /// <summary>Creates the color sliders at the bottom left of the modal</summary>
        /// <param name="content">The region to create the color sliders</param>
        private void CreateColorSliders(Content content) {
            var gap = content.CreateSubContent(new Vector2(content.Size.x, 50)).ApplyTemplate(VerticalLayout);

            _colorPickerLabel =
                content.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select a Color to Customize");

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

        /// <summary>Creates the color selection grid on the right of the modal</summary>
        /// <param name="content">The region to create the color selection grid</param>
        private void CreateColorSelection(Content content) {
            var (left, rightRegion) =
                content.SplitLeftRight((content.Size.x / 3f) - (HORIZONTAL_PADDING / 2f), HORIZONTAL_PADDING);
            var (center, right) =
                rightRegion.SplitLeftRight((content.Size.x / 3f) - (HORIZONTAL_PADDING / 2f), HORIZONTAL_PADDING);

            Content[] columns = { left, center, right };

            int i = 0;
            ColorManager.LoadedColors.ForEach(c => {
                var colorContent =
                    columns[i % 3].CreateSubContent(new Vector2(left.Size.x, ROW_HEIGHT)).ApplyTemplate(VerticalLayout);

                var (colorImage, name) = colorContent.SplitLeftRight(ROW_HEIGHT, HORIZONTAL_PADDING);

                colorContent.SetBackgroundColor<Content>(ColorManager.SynthesisColor.BackgroundSecondary);
                colorImage.SetBackgroundColor<Content>(c.Value);

                // Regex.Replace formats color's name with spaces (ColorName -> Color Name)
                var label = name.CreateLabel().SetText(Regex.Replace(c.Key.ToString(), "(\\B[A-Z])", " $1"));

                var button = colorContent.CreateButton()
                                 .StepIntoLabel(l => l.RootGameObject.SetActive(false))
                                 .AddOnClickedEvent(x => { SelectColor(c.Key); })
                                 .SetBackgroundColor<Button>(Color.clear)
                                 .SetStretch<Button>()
                                 .SetAnchoredPosition<Button>(Vector3.zero);

                _colors.Add(c.Key, (c.Value, colorImage, colorContent, label));

                i++;
            });
        }

        /// <summary>Selects a color to change with the RGB slider</summary>
        /// <param name="colorName">The color to select</param>
        private void SelectColor(ColorManager.SynthesisColor? colorName) {
            if (_selectedColor != null) {
                var prevSelected = _colors[_selectedColor.Value];
                prevSelected.background.SetBackgroundColor<Image>(ColorManager.SynthesisColor.BackgroundSecondary);
                prevSelected.label.SetColor(ColorManager.SynthesisColor.MainText);
            }

            if (colorName == null || _selectedThemeIndex == 0) {
                _colorPickerLabel.SetText("Cannot Edit Default Theme");
                return;
            }

            _selectedColor = colorName;

            var newSelected = _colors[_selectedColor.Value];
            newSelected.background.SetBackgroundColor<Image>(ColorManager.SynthesisColor.InteractiveElement);
            newSelected.label.SetColor(ColorManager.SynthesisColor.InteractiveElementText);

            // Regex.Replace formats color's name with spaces (ColorName -> Color Name)
            _colorPickerLabel.SetText(Regex.Replace(colorName.ToString(), "(\\B[A-Z])", " $1"));

            var colorInfo = _colors[colorName.Value];
            _rSlider.SetValue(colorInfo.color.r);
            _gSlider.SetValue(colorInfo.color.g);
            _bSlider.SetValue(colorInfo.color.b);
        }

        /// <summary>Selects a theme by index to use and/or edit</summary>
        /// <param name="index">The theme index to select</param>
        private void SelectTheme(int index) {
            if (index == 0)
                SelectColor(null);

            SaveThemeChanges();
            _selectedThemeIndex = index;

            SetThemePref();

            EventBus.Push(new SelectedThemeChanged());

            ColorManager.LoadedColors.ForEach(c => {
                var valueTuple   = _colors[c.Key];
                valueTuple.color = c.Value;
                valueTuple.image.SetBackgroundColor<Button>(c.Value);
                _colors[c.Key] = valueTuple;
            });
        }

        /// <summary>Saves all changes to the currently selected theme</summary>
        private void SaveThemeChanges() {
            List<(ColorManager.SynthesisColor name, Color32 color)> colors = new();
            _colors.ForEach(c => { colors.Add((c.Key, c.Value.color)); });
            ColorManager.ModifySelectedTheme(colors);
        }

        /// <summary>Gets the selected theme preference</summary>
        private void GetThemePref() => _selectedThemeIndex = ColorManager.ThemeNameToIndex(
            PreferenceManager.GetPreference<string>(ColorManager.SELECTED_THEME_PREF));

        /// <summary>Sets the selected theme preference</summary>
        private void SetThemePref() => PreferenceManager.SetPreference(
            ColorManager.SELECTED_THEME_PREF, ColorManager.AvailableThemes[_selectedThemeIndex]);
    }
}
