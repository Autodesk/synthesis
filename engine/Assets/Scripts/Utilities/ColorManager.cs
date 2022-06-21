using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Synthesis.UI {
    public static class ColorManager {
        private static Dictionary<string, Color32> _colors = new Dictionary<string, Color32>();

        static ColorManager() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.AltDirectorySeparatorChar + "Autodesk" + Path.AltDirectorySeparatorChar + "Synthesis" + Path.AltDirectorySeparatorChar + "colors.json";
            // File.Delete(path);
            if (File.Exists(path)) {
                _colors = JsonConvert.DeserializeObject<Dictionary<string, Color32>>(File.ReadAllText(path));
            } else {
                _colors["SAMPLE"] = new Color32(255, 255, 0, 255);
                File.WriteAllText(path, JsonConvert.SerializeObject(_colors));
            }
        }

        public static Color GetColor(string color) {
            if (_colors.ContainsKey(color))
                return _colors[color];
            return new Color32(255, 0, 255, 255);
        }
    }
}
