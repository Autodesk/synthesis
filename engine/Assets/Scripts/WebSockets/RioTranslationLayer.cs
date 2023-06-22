using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.RoboRIO;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.Utilities;

using Type = System.Type;

namespace Synthesis.WS.Translation {
    public class RioTranslationLayer {

        // I seriously hate json and its like 4am so deal
        public List<PWMGroup> PWMGroups = new List<PWMGroup>();
        public List<Encoder> Encoders   = new List<Encoder>();

        public class Encoder {
            private string _guid;
            public string GUID => _guid;
            private string _signal;
            public string Signal => _signal;
            private string _channelA;
            public string ChannelA => _channelA;
            private string _channelB;
            public string ChannelB => _channelB;
            private float _mod;
            public float Mod => _mod;

            private string _rioDevice = string.Empty;

            private (float time, float val) _periodTracker;

            public Encoder(string guid, string channelA, string channelB, string signal, float mod) {
                _guid     = guid;
                _channelA = channelA;
                _channelB = channelB;
                _signal   = signal;
                _mod      = mod;

                _periodTracker = (Time.realtimeSinceStartup, 0);
            }

            public void Update(ControllableState signalState) {
                int val = (int)(_mod * signalState.CurrentSignals[$"{_signal}_encoder"].Value.NumberValue);
                // TODO: Update riostate
                if (_rioDevice.Length == 0 && !AquireRioDevice(WebSocketManager.RioState))
                    return;

                float period;
                var deltaVal = val - _periodTracker.val;
                if (deltaVal == 0)
                    period = float.MaxValue;
                else
                    period = (Time.realtimeSinceStartup - _periodTracker.time) / deltaVal;

                WebSocketManager.UpdateData<EncoderData>(_rioDevice, e => e.Count = val);
                WebSocketManager.UpdateData<EncoderData>(_rioDevice, e => e.Period = period);
            }

            private bool AquireRioDevice(RoboRIOState rioState) {
                _rioDevice = string.Empty;
                int i      = 0;
                while (i < 8 && _rioDevice.Length == 0) {
                    var data = rioState.GetData<EncoderData>(i.ToString());
                    if (data != null) {
                        if (data.ChannelA.ToString() == _channelA && data.ChannelB.ToString() == _channelB) {
                            _rioDevice = i.ToString();
                        }
                    }
                    i++;
                }

                return _rioDevice.Length != 0;
            }
        }

        /// <summary>
        /// A Grouping of signals in which you can write the value of the the signals
        /// </summary>
        public class PWMGroup {

            private string _name;
            public string Name => _name;
            private List<string> _ports;
            public IReadOnlyList<string> Ports => _ports.AsReadOnly();
            private List<string> _signals;
            public IReadOnlyList<string> Signals => _signals.AsReadOnly();

            public PWMGroup(string name, string[] ports, string[] signals) {
                _name    = name;
                _signals = new List<string>();
                _ports   = new List<string>();
                _signals.AddRange(signals);
                _ports.AddRange(ports);
            }

            public void Update(RoboRIOState rioState, ControllableState signalState) {
                if (!WebSocketManager.RioState.GetData<DriverStationData>("").Enabled) {
                    _signals.ForEach(x => signalState.CurrentSignals[x].Value = Value.ForNumber(0));
                } else {
                    float avg = 0f;
                    _ports.ForEach(x => { avg += (float)rioState.GetData<PWMData>(x).Speed; });
                    avg /= _ports.Count;
                    _signals.ForEach(x => { signalState.CurrentSignals[x].Value = Value.ForNumber(avg); });
                }
            }
        }

        // public class Grouping {
        //     public List<IRioPointer> RioPointers = new List<IRioPointer>();
        //     public List<string> Signals = new List<string>();

        //     public float GetValue(RoboRIOState state) {
        //         float sum = 0f;
        //         RioPointers.Select(x => x.Get(state)).ForEach(x => sum += x);
        //         return sum / RioPointers.Count;
        //     }
        // }

        // public interface ISensor {
        //     object GetData(ControllableState state);
        // }

        // /// <summary>
        // /// Quadrature Encoder sensor that reads data from a provided
        // /// signal.
        // /// </summary>
        // public class QuadratureEncoderSensor : ISensor {

        //     private string _signal;
        //     private int _conversionFactor;

        //     /// <summary>
        //     /// Constructs a quadrature encoder sensor for rio sim.
        //     /// </summary>
        //     /// <param name="signal">Signal compatible with supplying quadrature encoder data</param>
        //     /// <param name="conversionFactor">Factor that is multiplied by the encoder value</param>
        //     public QuadratureEncoderSensor(string signal, int conversionFactor) {
        //         _signal = signal;
        //         _conversionFactor = conversionFactor;
        //     }

        //     public object GetData(ControllableState state) {
        //         return state.CurrentSignals[_signal].Value.NumberValue * _conversionFactor;
        //     }
        // }
    }
}
