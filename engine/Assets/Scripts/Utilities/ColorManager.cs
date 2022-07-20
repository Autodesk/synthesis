using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Synthesis.UI {
    public static class ColorManager {

        public const string SYNTHESIS_BLACK = "synthesis-black";
        public const string SYNTHESIS_BLACK_ACCENT = "synthesis-black-accent";
        public const string SYNTHESIS_ORANGE = "synthesis-orange";
        public const string SYNTHESIS_ORANGE_ACCENT = "synthesis-orange-accent";
        public const string SYNTHESIS_WHITE = "synthesis-white";
        public const string SYNTHESIS_WHITE_ACCENT = "synthesis-white-accent";
        public const string SYNTHESIS_ACCEPT = "synthesis-accept";
        public const string SYNTHESIS_CANCEL = "synthesis-cancel";
        public const string SYNTHESIS_ORANGE_CONTRAST_TEXT = "synthesis-orange-constrast-text";

        private static Dictionary<string, Color32> _colors = new Dictionary<string, Color32>();

        private static (string, Color)[] _defaultColors = {
            (SYNTHESIS_ORANGE, new Color32(250, 162, 27, 255)),
            (SYNTHESIS_ORANGE_ACCENT, new Color32(204, 124, 0, 255)),
            (SYNTHESIS_BLACK, new Color32(33, 37, 41, 255)),
            (SYNTHESIS_BLACK_ACCENT, new Color32(52, 58, 64, 255)),
            (SYNTHESIS_WHITE, new Color32(248, 249, 250, 255)),
            (SYNTHESIS_WHITE_ACCENT, new Color32(213, 216, 223, 255)),
            (SYNTHESIS_ACCEPT, new Color32(34, 139, 230, 255)),
            (SYNTHESIS_CANCEL, new Color32(250, 82, 82, 255)),
            (SYNTHESIS_ORANGE_CONTRAST_TEXT, new Color32(0, 0, 0, 255))
        };

        static ColorManager() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.AltDirectorySeparatorChar + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar + "colors.json";
            // File.Delete(path);
            if (File.Exists(path)) {
                _colors = JsonConvert.DeserializeObject<Dictionary<string, Color32>>(File.ReadAllText(path));
            }

            LoadDefaultColors();
            File.WriteAllText(path, JsonConvert.SerializeObject(_colors));
        }

        private static void LoadDefaultColors() {
            foreach (var c in _defaultColors) {
                if (!_colors.ContainsKey(c.Item1))
                    _colors[c.Item1] = c.Item2;
            }
        }

        public static Color TryGetColor(string color, Color defaultColor = default) {
            if (_colors.ContainsKey(color))
                return _colors[color];
            return defaultColor == default(Color) ? new Color(1, 1, 1, 1) : defaultColor;
        }
    }
}
