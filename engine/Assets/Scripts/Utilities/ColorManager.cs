using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Synthesis.Util;

namespace Synthesis.UI {
    public static class ColorManager {
        public const string SYNTHESIS_BLACK                = "synthesis-black";
        public const string SYNTHESIS_BLACK_ACCENT         = "synthesis-black-accent";
        public const string SYNTHESIS_ORANGE               = "synthesis-orange";
        public const string SYNTHESIS_ORANGE_ACCENT        = "synthesis-orange-accent";
        public const string SYNTHESIS_WHITE                = "synthesis-white";
        public const string SYNTHESIS_WHITE_ACCENT         = "synthesis-white-accent";
        public const string SYNTHESIS_ACCEPT               = "synthesis-accept";
        public const string SYNTHESIS_CANCEL               = "synthesis-cancel";
        public const string SYNTHESIS_ORANGE_CONTRAST_TEXT = "synthesis-orange-constrast-text";
        public const string SYNTHESIS_ICON                 = "synthesis-icon";
        public const string SYNTHESIS_ICON_ALT             = "synthesis-icon-alt";
        public const string SYNTHESIS_HIGHLIGHT_HOVER      = "synthesis-highlight-hover";
        public const string SYNTHESIS_HIGHLIGHT_SELECT     = "synthesis-highlight-select";
        public const string SYNTHESIS_RED_ALLIANCE         = "synthesis-red-alliance";
        public const string SYNTHESIS_BLUE_ALLIANCE        = "synthesis-blue-alliance";

        private static Dictionary<string, Color32> _colors = new Dictionary<string, Color32>();

        private static readonly string PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                              Path.AltDirectorySeparatorChar + "Autodesk" +
                                              Path.AltDirectorySeparatorChar + "Synthesis" +
                                              Path.AltDirectorySeparatorChar + "default_colors.json";

        private static (string, Color)[] _defaultColors = { (SYNTHESIS_ORANGE, new Color32(250, 162, 27, 255)),
            (SYNTHESIS_ORANGE_ACCENT, new Color32(204, 124, 0, 255)), (SYNTHESIS_BLACK, new Color32(33, 37, 41, 255)),
            (SYNTHESIS_BLACK_ACCENT, new Color32(52, 58, 64, 255)), (SYNTHESIS_WHITE, new Color32(248, 249, 250, 255)),
            (SYNTHESIS_WHITE_ACCENT, new Color32(213, 216, 223, 255)),
            (SYNTHESIS_ACCEPT, new Color32(34, 139, 230, 255)), (SYNTHESIS_CANCEL, new Color32(250, 82, 82, 255)),
            (SYNTHESIS_ORANGE_CONTRAST_TEXT, new Color32(0, 0, 0, 255)),
            (SYNTHESIS_ICON, new Color32(255, 255, 255, 255)), (SYNTHESIS_ICON_ALT, new Color32(0, 0, 0, 255)),
            (SYNTHESIS_HIGHLIGHT_HOVER, new Color32(89, 255, 133, 255)),
            (SYNTHESIS_HIGHLIGHT_SELECT, new Color32(255, 89, 133, 255)),
            (SYNTHESIS_RED_ALLIANCE, new Color32(240, 62, 62, 255)),
            (SYNTHESIS_BLUE_ALLIANCE, new Color32(62, 80, 240, 255)) };

        static ColorManager() {
            // File.Delete(path);
            Load();

            LoadDefaultColors();
            Save();
        }

        private static void LoadDefaultColors() {
            foreach (var c in _defaultColors) {
                if (!_colors.ContainsKey(c.Item1))
                    _colors[c.Item1] = c.Item2;
            }
        }

        private static void Save() {
            var jsonColors = new Dictionary<string, string>();
            _colors.ForEach(x => { jsonColors.Add(x.Key, ((Color) x.Value).ToHex()); });
            File.WriteAllText(PATH, JsonConvert.SerializeObject(jsonColors));
        }

        private static void Load() {
            var dir = Path.GetFullPath(PATH).Replace(Path.GetFileName(PATH), "");
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
                return;
            } else if (!File.Exists(PATH)) {
                return;
            }

            var jsonColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PATH));
            jsonColors.ForEach(x => { _colors.Add(x.Key, x.Value.ColorToHex()); });
        }

        public static Color TryGetColor(string color, Color defaultColor = default) {
            if (_colors.ContainsKey(color))
                return _colors[color];
            return defaultColor == default(Color) ? new Color(1, 1, 1, 1) : defaultColor;
        }

        public static bool HasColor(string color) => _colors.ContainsKey(color);
    }
}
