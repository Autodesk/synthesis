using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Synthesis.PreferenceManager;
using Synthesis.Util;
using SynthesisAPI.EventBus;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities.ColorManager {
    public static class ColorManager
    {
        public const string SELECTED_THEME_PREF = "color/selected_theme";
        
        private static readonly Color32 UNASSIGNED_COLOR = new(200, 255, 0, 255);
        
        private static readonly string PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                              Path.AltDirectorySeparatorChar + "Autodesk" +
                                              Path.AltDirectorySeparatorChar + "Synthesis" +
                                              Path.AltDirectorySeparatorChar + "Themes";

        private static readonly (SynthesisColor name, Color32 color)[] _defaultColors =
        {
            (SynthesisColor.SynthesisOrange, new Color32(250, 162, 27, 255)),
            (SynthesisColor.SynthesisOrangeAccent, new Color32(204, 124, 0, 255)),
            (SynthesisColor.SynthesisBlack, new Color32(33, 37, 41, 255)),
            (SynthesisColor.SynthesisBlackAccent, new Color32(52, 58, 64, 255)),
            (SynthesisColor.SynthesisWhite, new Color32(248, 249, 250, 255)),
            (SynthesisColor.SynthesisWhiteAccent, new Color32(213, 216, 223, 255)),
            (SynthesisColor.SynthesisAccept, new Color32(34, 139, 230, 255)),
            (SynthesisColor.SynthesisCancel, new Color32(250, 82, 82, 255)),
            (SynthesisColor.SynthesisOrangeContrastText, new Color32(0, 0, 0, 255)),
            (SynthesisColor.SynthesisIcon, new Color32(255, 255, 255, 255)),
            (SynthesisColor.SynthesisIconAlt, new Color32(0, 0, 0, 255)),
            (SynthesisColor.SynthesisHighlightHover, new Color32(89, 255, 133, 255)),
            (SynthesisColor.SynthesisHighlightSelect, new Color32(255, 89, 133, 255)),
            (SynthesisColor.SkyboxTop, new Color32(255, 255, 255, 255)),
            (SynthesisColor.SkyboxBottom, new Color32(255, 255, 255, 255)),
            (SynthesisColor.FloorGrid, new Color32(0, 0, 0, 255))
        };

        private static Dictionary<SynthesisColor, Color32> _loadedColors = new();
        
        public static Dictionary<SynthesisColor, Color32> LoadedColors
        {
            get => _loadedColors;
        }

        private const string DEFAULT_THEME = "Default";
        private static string _selectedTheme = DEFAULT_THEME;

        public static string[] AvailableThemes
        {
            get
            {
                var themes = Directory.GetFiles(PATH).Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
                themes.Insert(0, "Default");
                return themes.ToArray();
            }
        }

        public static string SelectedTheme
        {
            get => _selectedTheme;
            private set
            {
                Debug.Log($"Set Theme To {value}");
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
        
        public class OnThemeChanged : IEvent { }

        static ColorManager()
        {
            EventBus.NewTypeListener<PostPreferenceSaveEvent>(e => {
                string selectedTheme = PreferenceManager.GetPreference<string>(SELECTED_THEME_PREF);
                SelectedTheme = selectedTheme;
            });
            _selectedTheme = PreferenceManager.GetPreference<string>(SELECTED_THEME_PREF);
            
            LoadTheme(_selectedTheme);
            LoadDefaultColors();
            SaveTheme(_selectedTheme);
        }

        private static void LoadTheme(string themeName)
        {
            if (themeName == "Default") return;
            
            string themePath = PATH + Path.AltDirectorySeparatorChar + themeName + ".json";
            Debug.Log($"Loading theme: {themePath}");
            
            var dir = Path.GetFullPath(themePath).Replace(Path.GetFileName(themePath), "");
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
                return;
            } else if (!File.Exists(themePath)) {
                return;
            }

            var jsonColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(themePath));

            jsonColors?.ForEach(x => { 
                _loadedColors.Add(Enum.Parse<SynthesisColor>(x.Key), x.Value.ColorToHex()); 
            });
        }
        
        private static void SaveTheme(string themeName) {
            if (themeName == "Default") return;
            
            string themePath = PATH + Path.AltDirectorySeparatorChar + themeName + ".json";
            Debug.Log($"Saving theme: {themePath}");

            
            var jsonColors = new Dictionary<string, string>();
            
            _loadedColors.ForEach(x => {
                jsonColors.Add(x.Key.ToString(), ((Color)x.Value).ToHex());
            });
            
            File.WriteAllText(themePath, JsonConvert.SerializeObject(jsonColors));
        }

        private static void LoadDefaultColors()
        {
            _defaultColors.ForEach(c => {
                _loadedColors.TryAdd(c.name, c.color);
            });
        }

        public static void ModifyLoadedTheme(List<(SynthesisColor name, Color32 color)> changes)
        {
            if (_selectedTheme == null)
                return;
            
            changes.ForEach(c =>
                _loadedColors[c.name] = c.color);
            
            SaveTheme(_selectedTheme);
            
            EventBus.Push(new OnThemeChanged());
        }

        public static Color GetColor(SynthesisColor colorName)
        {
            if (_loadedColors.TryGetValue(colorName, out Color32 color))
                return color;

            return UNASSIGNED_COLOR;
        }

        public static int ThemeNameToIndex(string themeName)
        {
            int i = 0;
            foreach (string theme in AvailableThemes)
            {
                if (theme.Equals(themeName))
                    return i;
                i++;
            }
            
            return -1;
        }

        public enum SynthesisColor
        {
            SynthesisOrange,
            SynthesisOrangeAccent,
            SynthesisBlack,
            SynthesisBlackAccent,
            SynthesisWhite,
            SynthesisWhiteAccent,
            SynthesisAccept,
            SynthesisCancel,
            SynthesisOrangeContrastText,
            SynthesisIcon,
            SynthesisIconAlt,
            SynthesisHighlightHover,
            SynthesisHighlightSelect,
            SkyboxTop,
            SkyboxBottom,
            FloorGrid
        }
    }
}