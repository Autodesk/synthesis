using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.RoboRIO;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.Simulation;

using Type = System.Type;
using SynthesisAPI.Controller;

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
                var signalVal = signalState.GetValue($"{_signal}_encoder");
                int val       = (int) (signalVal == null ? 0 : signalVal.NumberValue);

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
                    _signals.ForEach(x => signalState.SetValue(x, Value.ForNumber(0)));
                } else {
                    float avg = 0f;
                    _ports.ForEach(x => { avg += (float) rioState.GetData<PWMData>(x).Speed; });
                    avg /= _ports.Count;
                    _signals.ForEach(x => { signalState.SetValue(x, Value.ForNumber(avg)); });
                }
            }
        }
    }
}
