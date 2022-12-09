using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SynthesisAPI.RoboRIO;
using Google.Protobuf.WellKnownTypes;
using System;
using SynthesisAPI.Utilities;

namespace Synthesis.WS.Translation {
    public class RioTranslationLayer {

        public Dictionary<string, Grouping> Groupings = new Dictionary<string, Grouping>();
        public List<ISensor> Sensors = new List<ISensor>();

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

        public interface ISensor {
            object GetData(ControllableState state);
        }

        /// <summary>
        /// Quadrature Encoder sensor that reads data from a provided
        /// signal.
        /// </summary>
        public class QuadratureEncoderSensor : ISensor {

            private string _signal;
            private int _conversionFactor;

            /// <summary>
            /// Constructs a quadrature encoder sensor for rio sim.
            /// </summary>
            /// <param name="signal">Signal compatible with supplying quadrature encoder data</param>
            /// <param name="conversionFactor">Factor that is multiplied by the encoder value</param>
            public QuadratureEncoderSensor(string signal, int conversionFactor) {
                _signal = signal;
                _conversionFactor = conversionFactor;
            }

            public object GetData(ControllableState state) {
                return state.CurrentSignals[_signal].Value.NumberValue * _conversionFactor;
            }
        }
    }
}
