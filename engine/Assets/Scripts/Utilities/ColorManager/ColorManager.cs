using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Synthesis.PreferenceManager;
using Synthesis.Util;
using SynthesisAPI.EventBus;
using UI.Dynamic.Modals.Configuring.ThemeEditor;
using UnityEngine;

namespace Utilities.ColorManager {
    public static class ColorManager {
        public const string SELECTED_THEME_PREF = "color/selected_theme";
        public const string DEFAULT_THEME       = "Default";

        public class OnThemeChanged : IEvent {}

        private static readonly Color UNASSIGNED_COLOR = new(200, 255, 0, 255);

        private static string THEMES_FOLDER_PATH {
            get {
                string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                             Path.AltDirectorySeparatorChar + "Autodesk" + Path.AltDirectorySeparatorChar +
                             "Synthesis" + Path.AltDirectorySeparatorChar + "Themes";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }


        private static Dictionary<SynthesisColor, Color> _tempPreviewColors = null;
        private static Dictionary<SynthesisColor, Color> _loadedColors  = new();
        public static Dictionary<SynthesisColor, Color> LoadedColors   => _loadedColors;
        public static Dictionary<SynthesisColor, Color> ActiveColors => _tempPreviewColors ?? LoadedColors;

        private static string _selectedTheme;
        public static string SelectedTheme {
            get => _selectedTheme;
            set {
                if (value == _selectedTheme)
                    return;

                _selectedTheme = value;

                _loadedColors = new();
                LoadTheme(_selectedTheme);
                LoadDefaultColors();
                SaveTheme(_selectedTheme);

                EventBus.Push(new OnThemeChanged());
            }
        }

        /// <summary>A list of themes found in Appdata plus the default theme</summary>
        public static string[] AvailableThemes {
            get {
                var themes = Directory.GetFiles(THEMES_FOLDER_PATH).Select(Path.GetFileNameWithoutExtension).ToList();

                themes.Insert(0, DEFAULT_THEME);
                return themes.ToArray();
            }
        }

        static ColorManager() {
            EventBus.NewTypeListener<EditThemeModal.SelectedThemeChanged>(e => {
                string selectedTheme = PreferenceManager.GetPreference<string>(SELECTED_THEME_PREF);
                SelectedTheme        = selectedTheme;
            });
            _selectedTheme = PreferenceManager.GetPreference<string>(SELECTED_THEME_PREF);
            if (_selectedTheme is "" or null) {
                PreferenceManager.SetPreference(SELECTED_THEME_PREF, DEFAULT_THEME);
                _selectedTheme = DEFAULT_THEME;
            }

            LoadTheme(_selectedTheme);
            LoadDefaultColors();
            SaveTheme(_selectedTheme);
        }

        /// <summary>Loads the default theme into the _colors dictionary. Will fill missing colors in a custom
        /// theme</summary>
        private static void LoadDefaultColors() {
            DefaultColors.SYNTHESIS_DEFAULT.ForEach(c => { _loadedColors.TryAdd(c.name, c.color); });
        }

        /// <summary>Loads a theme from the synthesis appdata folder. Will create a theme if it does not exist</summary>
        /// <param name="themeName">The theme to load</param>
        private static void LoadTheme(string themeName) {
            if (themeName is DEFAULT_THEME or "")
                return;
            
            string themePath = THEMES_FOLDER_PATH + Path.AltDirectorySeparatorChar + themeName + ".json";

            var dir = Path.GetFullPath(themePath).Replace(Path.GetFileName(themePath), "");
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
                return;
            } else if (!File.Exists(themePath)) {
                return;
            }

            var jsonColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(themePath));

            jsonColors?.ForEach(x => {
                if (Enum.TryParse<SynthesisColor>(x.Key, out var colorName))
                    _loadedColors.Add(colorName, x.Value.ColorToHex());
            });
        }

        /// <summary>Loads a theme to the synthesis appdata folder</summary>
        /// <param name="themeName">The theme to save</param>
        private static void SaveTheme(string themeName) {
            if (themeName is DEFAULT_THEME or "")
                return;

            string themePath = THEMES_FOLDER_PATH + Path.AltDirectorySeparatorChar + themeName + ".json";

            var jsonColors = new Dictionary<string, string>();

            _loadedColors.ForEach(x => { jsonColors.Add(x.Key.ToString(), ((Color) x.Value).ToHex()); });

            File.WriteAllText(themePath, JsonConvert.SerializeObject(jsonColors));
        }

        /// <summary>Deletes a theme from the synthesis appdata folder</summary>
        /// <param name="themeName">The theme to delete</param>
        private static void DeleteTheme(string themeName) {
            if (themeName is DEFAULT_THEME or "")
                return;

            string themePath = THEMES_FOLDER_PATH + Path.AltDirectorySeparatorChar + themeName + ".json";

            File.Delete(themePath);
        }
        
        /// <summary>Permanently deletes the selected theme unless it is Default</summary>
        public static void DeleteSelectedTheme() {
            DeleteTheme(_selectedTheme);
            SelectedTheme = DEFAULT_THEME;
        }

        public static void DeleteAllThemes()
        {
            AvailableThemes.ForEach(DeleteTheme);
            SelectedTheme = DEFAULT_THEME;
        }

        /// <summary>Modifies the colors of the selected theme</summary>
        /// <param name="changes">A list of new colors. Does not have to contain every color</param>
        public static void ModifySelectedTheme(List<(SynthesisColor name, Color color)> changes) {
            SetTempPreviewColors(null);
            
            if (_selectedTheme == null)
                return;

            changes.ForEach(c => _loadedColors[c.name] = c.color);

            SaveTheme(_selectedTheme);

            EventBus.Push(new OnThemeChanged());
        }

        /// <summary>Returns a color, or <see cref="UNASSIGNED_COLOR">UNASSIGNED_COLOR</see> if it is not
        /// found</summary> <param name="colorName">The name of the color to get</param> <returns>The corresponding
        /// color of the current theme</returns>
        public static Color GetColor(SynthesisColor colorName) {
            if (_tempPreviewColors != null) {
                if (_tempPreviewColors.TryGetValue(colorName, out Color color))
                    return color;
            }
            else {
                if (_loadedColors.TryGetValue(colorName, out Color color))
                    return color;
            }

            return UNASSIGNED_COLOR;
        }

        /// <summary>Temporarily preview colors without saving them. Call with null to switch back to loaded colors</summary>
        /// <param name="colors">The colors to preview. If null, color manager will switch back to loaded colors</param>
        public static void SetTempPreviewColors(Dictionary<SynthesisColor, Color> colors) {
            _tempPreviewColors = colors;
            EventBus.Push(new OnThemeChanged());
        }

        /// <summary>Finds the index of a theme</summary>
        /// <param name="themeName">A theme name</param>
        /// <returns>The index of the given theme</returns>
        public static int ThemeNameToIndex(string themeName) {
            int i = 0;
            foreach (string theme in AvailableThemes) {
                if (theme.Equals(themeName))
                    return i;
                i++;
            }

            return -1;
        }

        /// <summary>The theme name at that index, or default if it does not exist</summary>
        /// <param name="index">A theme index</param>
        /// <returns>The name of the given theme</returns>
        public static string ThemeIndexToName(int index) {
            if (index >= AvailableThemes.Length || index == -1)
                return DEFAULT_THEME;
            else
                return AvailableThemes[index];
        }

        /// <summary>Each value represents a different color that can differ across themes</summary>
        public enum SynthesisColor {
            InteractiveElement,
            InteractiveSecondary,
            Background,
            BackgroundSecondary,
            MainText,
            Scrollbar,
            AcceptButton,
            CancelButton,
            InteractiveElementText,
            Icon,
            HighlightHover,
            HighlightSelect,
            SkyboxTop,
            SkyboxBottom,
            FloorGrid
        }
    }
}