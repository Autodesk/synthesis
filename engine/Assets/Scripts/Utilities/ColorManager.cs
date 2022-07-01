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

        private static Dictionary<string, Color32> _colors = new Dictionary<string, Color32>();

        static ColorManager() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.AltDirectorySeparatorChar + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar + "colors.json";
            // File.Delete(path);
            if (File.Exists(path)) {
                _colors = JsonConvert.DeserializeObject<Dictionary<string, Color32>>(File.ReadAllText(path));
            } else {
                LoadDefaultColors();
                File.WriteAllText(path, JsonConvert.SerializeObject(_colors));
            }
        }

        private static void LoadDefaultColors() {
            _colors[SYNTHESIS_ORANGE] = new Color32(250, 162, 27, 255);
            _colors[SYNTHESIS_ORANGE_ACCENT] = new Color32(204, 124, 0, 255);
            _colors[SYNTHESIS_BLACK] = new Color32(33, 37, 41, 255);
            _colors[SYNTHESIS_BLACK_ACCENT] = new Color32(52, 58, 64, 255);
            _colors[SYNTHESIS_WHITE] = new Color32(248, 249, 250, 255);
            _colors[SYNTHESIS_WHITE_ACCENT] = new Color32(213, 216, 223, 255);
        }

        public static Color TryGetColor(string color, Color defaultColor = default) {
            if (_colors.ContainsKey(color))
                return _colors[color];
            return defaultColor == default(Color) ? new Color(1, 1, 1, 1) : defaultColor;
        }
    }
}
