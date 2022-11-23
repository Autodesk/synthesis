using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SynthesisAPI.RoboRIO;
using Google.Protobuf.WellKnownTypes;
using System;

namespace Synthesis.WS.Translation {
    public class RioTranslationLayer {

        public Dictionary<string, Grouping> Groupings = new Dictionary<string, Grouping>();

        public interface IRioPointer {
            float Get(RoboRIOState state);
            void Set(RoboRIOState state, float value);
        }

        public class PWMRioPointer : IRioPointer {

            private string _device;
            public string Device => _device;
            private DataSelection _selection;

            public PWMRioPointer(string device, DataSelection selection) {
                _device = device;
                _selection = selection;
            }

            public float Get(RoboRIOState state) {
                var data = state.GetData<PWMData>("PWM", _device);
                switch (_selection) {
                    case DataSelection.Speed:
                        return (float)data.Speed;
                    case DataSelection.Position:
                        return (float)data.Position;
                    default:
                        return 0f;
                }
            }

            public void Set(RoboRIOState state, float value) {
                throw new Exception("No values to be written in PMW");
            }

            public enum DataSelection {
                Speed, Position
            }
        }

        public class Grouping {
            public List<IRioPointer> RioPointers = new List<IRioPointer>();
            public List<string> Signals = new List<string>();

            public float GetValue(RoboRIOState state) {
                float sum = 0f;
                RioPointers.Select(x => x.Get(state)).ForEach(x => sum += x);
                return sum / RioPointers.Count;
            }
        }
    }
}
