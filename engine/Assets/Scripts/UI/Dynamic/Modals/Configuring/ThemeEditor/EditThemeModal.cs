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
        private const float MODAL_HEIGHT       = 430;
        private const float ROW_HEIGHT         = 60;
        private const float HORIZONTAL_PADDING = 15;

        private Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset);
            return u;
        };

        public class SelectedThemeChanged : IEvent {}

        private string[] _availableThemes = ColorManager.AvailableThemes;

        private Dictionary<ColorManager.SynthesisColor, (Color color, Content image, Content background, Label label)>
            _colors = new();

        private ColorManager.SynthesisColor? _selectedColor = null;
        private int _selectedThemeIndex;

        private Label _colorPickerLabel;
        private Slider _hSlider;
        private Slider _sSlider;
        private Slider _vSlider;

        private Button _deleteButton;
        private Button _deleteAllButton;

        public EditThemeModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        public override void Create() {
            Title.SetText("Theme Editor");
            
            AcceptButton.StepIntoLabel(l => l.SetText("Save")).AddOnClickedEvent(x => {
                SaveThemeChanges();
                DynamicUIManager.CloseActiveModal();
                ColorManager.SetTempPreviewColors(null);
            });

            CancelButton.AddOnClickedEvent(x => {
                ColorManager.SetTempPreviewColors(null);
                DynamicUIManager.CloseActiveModal();
            });

            MiddleButton.AddOnClickedEvent(x => { PreviewColors(); }).StepIntoLabel(l => l.SetText("Preview"));

            var (left, right) = MainContent.SplitLeftRight(500 - (HORIZONTAL_PADDING / 2), HORIZONTAL_PADDING);

            CreateThemeSelection(left);
            CreateColorSliders(left);

            CreateColorSelection(right);

            SelectTheme(_selectedThemeIndex, false);

            UpdateDeleteButtons();
        }

        public override void Update() {
            if (_selectedColor == null)
                return;

            Color colorInput = Color.HSVToRGB(_hSlider.Value / 360f, _sSlider.Value / 100f, _vSlider.Value / 100f);

            var valueTuple = _colors[_selectedColor.Value];
            valueTuple.image.SetBackgroundColor<Button>(colorInput);
            valueTuple.color = colorInput;

            _colors[_selectedColor.Value] = (colorInput, valueTuple.image, valueTuple.background, valueTuple.label);

            _hSlider.SetValue((int) _hSlider.Value);
            _sSlider.SetValue((int) _sSlider.Value);
            _vSlider.SetValue((int) _vSlider.Value);
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

            var (addContent, right) = content.CreateSubContent(new Vector2(content.Size.x, 50))
                                          .ApplyTemplate(VerticalLayout)
                                          .SplitLeftRight((content.Size.x - padding) / 3f, padding);

            var (deleteContent, deleteAllContent) = right.SplitLeftRight((content.Size.x - padding) / 3f, padding);

            var addThemeButton = addContent.CreateButton()
                                     .ApplyTemplate(VerticalLayout)
                                     .StepIntoLabel(l => l.SetText("New"))
                                     .AddOnClickedEvent(b => { DynamicUIManager.CreateModal<NewThemeModal>(); });

            _deleteButton = deleteContent.CreateButton()
                                .ApplyTemplate(VerticalLayout)
                                .StepIntoLabel(l => l.SetText("Delete"))
                                .StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.InteractiveElementLeft,
                                    ColorManager.SynthesisColor.InteractiveElementRight))
                                .AddOnClickedEvent(b => {
                                    if (_selectedThemeIndex != 0) {
                                        SaveThemeChanges();
                                        DynamicUIManager.CreateModal<DeleteThemeModal>();
                                    }
                                });

            _deleteAllButton = deleteAllContent.CreateButton()
                                   .ApplyTemplate(VerticalLayout)
                                   .StepIntoLabel(l => l.SetText("Delete All"))
                                   .StepIntoImage(i => i.SetColor(ColorManager.SynthesisColor.CancelButton))
                                   .AddOnClickedEvent(b => {
                                       if (_selectedThemeIndex != 0) {
                                           SaveThemeChanges();
                                       }

                                       DynamicUIManager.CreateModal<DeleteAllThemesModal>();
                                   });

            themeChooser.AddOnValueChangedEvent((dropdown, index, data) => {
                SelectTheme(index);
                DynamicUIManager.CreateModal<EditThemeModal>();
                UpdateDeleteButtons();
            });
        }

        /// <summary>Updates the color of the delete buttons and if they can be pressed</summary>
        private void UpdateDeleteButtons() {
            if (_selectedThemeIndex < 1)
                _deleteButton.DisableEvents<Button>().StepIntoImage(i => i.SetColor(
                    ColorManager.SynthesisColor.InteractiveBackground));
            else
                _deleteButton.EnableEvents<Button>().StepIntoImage(i => i.SetColor(
                    ColorManager.SynthesisColor.InteractiveElementLeft,
                    ColorManager.SynthesisColor.InteractiveElementRight));

            if (_availableThemes.Length == 1)
                _deleteAllButton.DisableEvents<Button>().StepIntoImage(i => i.SetColor(
                    ColorManager.SynthesisColor.InteractiveBackground));
            else
                _deleteAllButton.EnableEvents<Button>().StepIntoImage(i => i.SetColor(
                    ColorManager.SynthesisColor.CancelButton));
        }

        /// <summary>Creates the color sliders at the bottom left of the modal</summary>
        /// <param name="content">The region to create the color sliders</param>
        private void CreateColorSliders(Content content) {
            var gap = content.CreateSubContent(new Vector2(content.Size.x, 50)).ApplyTemplate(VerticalLayout);

            _colorPickerLabel =
                content.CreateLabel().ApplyTemplate(VerticalLayout).SetText("Select a Color to Customize");

            _hSlider = content.CreateSlider()
                           .ApplyTemplate(VerticalLayout)
                           .StepIntoTitleLabel(l => l.SetText("Hue"))
                           .SetRange(0, 360);
            _sSlider = content.CreateSlider()
                           .ApplyTemplate(VerticalLayout)
                           .StepIntoTitleLabel(l => l.SetText("Saturation"))
                           .SetRange(0, 100);
            _vSlider = content.CreateSlider()
                           .ApplyTemplate(VerticalLayout)
                           .StepIntoTitleLabel(l => l.SetText("Value"))
                           .SetRange(0, 100);
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
            ColorManager.ActiveColors.ForEach(c => {
                var colorContent =
                    columns[i % 3].CreateSubContent(new Vector2(left.Size.x, ROW_HEIGHT)).ApplyTemplate(VerticalLayout);

                var (colorImage, name) = colorContent.SplitLeftRight(ROW_HEIGHT, HORIZONTAL_PADDING);

                colorContent.StepIntoImage(i => i.SetSprite(null!).SetCornerRadius(10))
                    .SetBackgroundColor<Content>(ColorManager.SynthesisColor.BackgroundSecondary);
                
                colorImage.SetBackgroundColor<Content>(c.Value);

                // Regex.Replace formats color's name with spaces (ColorName -> Color Name)
                var label = name.CreateLabel().SetText(Regex.Replace(c.Key.ToString(), "(\\B[A-Z])", " $1"));

                var button = colorContent.CreateButton()
                    .StepIntoLabel(l => l.RootGameObject.SetActive(false))
                    .AddOnClickedEvent(x => { SelectColor(c.Key); })
                    .SetStretch<Button>()
                    .SetAnchoredPosition<Button>(Vector3.zero)
                    .StepIntoImage(i => i.SetColor(Color.clear));

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

            if (colorName == null || _selectedThemeIndex < 1) {
                _colorPickerLabel.SetText("Cannot Edit Default Theme");
                return;
            }

            _selectedColor = colorName;

            var newSelected = _colors[_selectedColor.Value];
            newSelected.background.SetBackgroundColor<Image>(ColorManager.SynthesisColor.InteractiveElementLeft,
                ColorManager.SynthesisColor.InteractiveElementRight);
            newSelected.label.SetColor(ColorManager.SynthesisColor.InteractiveElementText);

            // Regex.Replace formats color's name with spaces (ColorName -> Color Name)
            _colorPickerLabel.SetText(Regex.Replace(colorName.ToString(), "(\\B[A-Z])", " $1"));

            var rgbColor = _colors[colorName.Value].color;
            Color.RGBToHSV(rgbColor, out float h, out float s, out float v);

            _hSlider.SetValue((int) (h * 360));
            _sSlider.SetValue((int) (s * 100));
            _vSlider.SetValue((int) (v * 100));
        }

        /// <summary>Selects a theme by index to use and/or edit</summary>
        /// <param name="index">The theme index to select</param>
        private void SelectTheme(int index, bool saveChanges = true) {
            if (index == 0)
                SelectColor(null);

            if (saveChanges) {
                ColorManager.SetTempPreviewColors(null);
                SaveThemeChanges();
            }

            _selectedThemeIndex = index;

            SetThemePref();

            EventBus.Push(new SelectedThemeChanged());

            ColorManager.ActiveColors.ForEach(c => {
                var valueTuple   = _colors[c.Key];
                valueTuple.color = c.Value;
                valueTuple.image.SetBackgroundColor<Button>(c.Value);
                _colors[c.Key] = valueTuple;
            });
        }

        /// <summary>Saves all changes to the currently selected theme</summary>
        private void SaveThemeChanges() {
            List<(ColorManager.SynthesisColor name, Color color)> colors = new();
            _colors.ForEach(c => { colors.Add((c.Key, c.Value.color)); });
            ColorManager.ModifySelectedTheme(colors);
        }

        /// <summary>Gets the selected theme preference</summary>
        private void GetThemePref() {
            _selectedThemeIndex = ColorManager.ThemeNameToIndex(
                PreferenceManager.GetPreference<string>(ColorManager.SELECTED_THEME_PREF));
        }

        /// <summary>Sets the selected theme preference</summary>
        private void SetThemePref() {
            PreferenceManager.SetPreference(
                ColorManager.SELECTED_THEME_PREF, ColorManager.ThemeIndexToName(_selectedThemeIndex));
        }

        /// <summary>Update all colors of this modal to preview selected colors</summary>
        private void PreviewColors() {
            Dictionary<ColorManager.SynthesisColor, Color> colors = new();
            _colors.ForEach(c => { colors.Add(c.Key, c.Value.color); });
            ColorManager.SetTempPreviewColors(colors);
            DynamicUIManager.CreateModal<EditThemeModal>();
        }
    }
}
