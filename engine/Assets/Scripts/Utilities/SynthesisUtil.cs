using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirabuf;
using Mirabuf.Joint;

using Color = UnityEngine.Color;

namespace Synthesis.Util {
    public static class SynthesisUtil {

        public static void ForEach<T>(this IEnumerable<T> e, Action<T> a) {
            foreach (var i in e) {
                a(i);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> e, Action<T, int> a) {
            for (int i = 0; i < e.Count(); i++) {
                a(e.ElementAt(i), i);
            }
        }

        public static bool HasSignal(this JointInstance inst)
            => inst.SignalReference != null && !inst.SignalReference.Equals(string.Empty);

        public static Color ColorFromHex(uint hex) => new Color(
            (float)((hex & 0xFF000000) >> 24) / 255f,
            (float)((hex & 0x00FF0000) >> 16) / 255f,
            (float)((hex & 0x0000FF00) >> 8) / 255f,
            (float)(hex & 0x000000FF) / 255f
        );

        public static Color ColorFromHex(string hex) {
            if (hex.Substring(0, 2) == "0x")
                hex = hex.Substring(2);
            
            if (hex.Length > 6) {
                return new Color(
                    (float)HexToByte(hex.Substring(0, 2)) / 255f,
                    (float)HexToByte(hex.Substring(2, 2)) / 255f,
                    (float)HexToByte(hex.Substring(4, 2)) / 255f,
                    (float)HexToByte(hex.Substring(6, 2)) / 255f);
            } else {
                return new Color(
                    (float)HexToByte(hex.Substring(0, 2)) / 255f,
                    (float)HexToByte(hex.Substring(2, 2)) / 255f,
                    (float)HexToByte(hex.Substring(4, 2)) / 255f);
            }
        }

        public static byte HexToByte(string hex) => (byte)((HexToByte(hex[0]) * 16) + HexToByte(hex[1]));

        private static byte HexToByte(char hex) {
            switch (hex.ToString().ToUpper()[0]) {
                case 'A':
                    return 10;
                case 'B':
                    return 11;
                case 'C':
                    return 12;
                case 'D':
                    return 13;
                case 'E':
                    return 14;
                case 'F':
                    return 15;
                default:
                    return byte.Parse(hex.ToString());
            }
        }
        
    }
}
