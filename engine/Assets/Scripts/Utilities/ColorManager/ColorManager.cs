using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Synthesis.Util;
using SynthesisAPI.EventBus;
using UnityEngine;

namespace Utilities.ColorManager {
    public static class ColorManager
    {
        private static readonly Color32 UNASSIGNED_COLOR = new Color32(200, 255, 0, 255);
        
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
            (SynthesisColor.SynthesisHighlightSelect, new Color32(255, 89, 133, 255))
        };

        private static Dictionary<SynthesisColor, Color32> _loadedColors = new();

        private const string DEFAULT_THEME = "default";
        private static string _selectedTheme;

        public static string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
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
            Debug.Log("Color manager static");
            LoadTheme(DEFAULT_THEME);
            LoadDefaultColors();
            SaveTheme(DEFAULT_THEME);
        }

        private static void LoadTheme(string themeName)
        {
            if (themeName == "default") return;
            
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

            jsonColors.ForEach(x => { 
                _loadedColors.Add(Enum.Parse<SynthesisColor>(x.Key), x.Value.ColorToHex()); 
            });
        }
        
        private static void SaveTheme(string themeName) {
            if (themeName == "default") return;
            
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

        private static Color GetColor(SynthesisColor colorName)
        {
            if (_loadedColors.TryGetValue(colorName, out Color32 color))
                return color;

            return UNASSIGNED_COLOR;
        }

        public static void AssignColor(SynthesisColor colorName, Action<Color> applyColor)
        {
            applyColor.Invoke(GetColor(colorName));
            
            EventBus.NewTypeListener<OnThemeChanged>(x =>
            {
                applyColor.Invoke(GetColor(colorName));
            });
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
            SynthesisHighlightSelect
        }
    }
}