using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synthesis.Physics;
using SynthesisAPI.EventBus;
using UnityEngine;

#nullable enable

namespace Synthesis.Replay {
    public static class ReplayManager {
        private static bool _isRecording = false;
        public static bool IsRecording {
            get => _isRecording;
            set {
                _isRecording = value;
                // if (!_isRecording) {
                //     InvalidateRecording();
                // }
            }
        }
        private static float _timeSpan = 5f;
        public static float TimeSpan => _timeSpan;
        private static ReplayFrame? _newestFrame = null;
        public static ReplayFrame? NewestFrame => _newestFrame;
        private static ReplayFrame? _oldestFrame = null;
        public static ReplayFrame CurrentFrame = null!;

        private static float _desyncTime = 0;
        public static float DesyncTime => _desyncTime;

        private static Action<ContactReport, float>? _createContactMarker;
        public static Action<ContactReport, float>? CreateContactMarker => _createContactMarker;
        private static Action? _eraseContactMarkers;
        public static Action? EraseContactMarkers => _eraseContactMarkers;

        private static SortedList<float, ContactReport> _contactReports = new SortedList<float, ContactReport>();

        private static float _startDesync = float.NaN;
        public static void SetupDesyncTracker() {
            EventBus.NewTypeListener<PhysicsFreezeChangeEvent>(e => {
                var pe = e as PhysicsFreezeChangeEvent;
                
                if (pe!.IsFrozen) {
                    _startDesync = Time.realtimeSinceStartup;
                } else {
                    if (float.IsNaN(_startDesync))
                        return;
                    _desyncTime += Time.realtimeSinceStartup - _startDesync;
                    _startDesync = float.NaN;
                }
            });
        }
        public static void SetupContactUI(Action<ContactReport, float> create, Action erase) {
            _createContactMarker = create;
            _eraseContactMarkers = erase;
        }

        public static void PushContactReport(ContactReport r) {
            if (_contactReports.ContainsKey(r.TimeStamp))
                return;
            _contactReports.Add(r.TimeStamp, r);
        }

        public static void InvalidateRecording() {
            _newestFrame = null;
            _oldestFrame = null;
        }

        /// <summary>
        /// MARK: Bug in here I think
        /// </summary>
        public static void RecordFrame() {
            if (!IsRecording || PhysicsManager.IsFrozen)
                return;

            var newFrame = ReplayFrame.RecordFrame();
            if (_newestFrame == null) {
                _newestFrame = newFrame;
                _oldestFrame = newFrame;
            } else {
                newFrame.LastFrame = _newestFrame;
                _newestFrame.NextFrame = newFrame;
                _newestFrame = newFrame;
                while (_newestFrame.TimeStamp - _oldestFrame?.TimeStamp > _timeSpan) {
                    _oldestFrame = _oldestFrame?.NextFrame;
                    _oldestFrame.LastFrame = null!;
                    // Debug.Log("Frame dropped");
                    if (_oldestFrame == _newestFrame)
                        break;
                }
            }

            while (_contactReports.Count > 0 && (Time.realtimeSinceStartup - _desyncTime) - _contactReports.ElementAt(0).Value.TimeStamp > _timeSpan) {
                _contactReports.RemoveAt(0);
            }

            // int frameCount = 1;
            // var tmp = _newestFrame;
            // while (tmp.LastFrame != null) {
            //     tmp = tmp.LastFrame;
            //     frameCount++;
            // }
            // Debug.Log($"Replay Frames: {frameCount}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Use negative range to show relative to the latest frame</param>
        public static ReplayFrame? GetFrameAtTime(float t) {

            if (_newestFrame == null)
                return null;

            if (t > 0) {
                throw new NotImplementedException();
            } else {
                // Time relative to the latest frame
                ReplayFrame? a = _newestFrame, b = null;
                while (a.TimeStamp > _newestFrame.TimeStamp + t) {
                    if (a.LastFrame == null) {
                        break;
                    }
                    a = a.LastFrame;
                }
                if (a.NextFrame == null)
                    return a;
                b = a.NextFrame;
                return ReplayFrame.Lerp(a, b, ((_newestFrame.TimeStamp + t) - a.TimeStamp) / (b.TimeStamp - a.TimeStamp));
            }
        }

        public static void ShowContactsAtTime(float t, float giveOrTake = 0.25f, float impulseThreshold = 10f) {
            if (_newestFrame == null
                || _contactReports.Count < 1
                || CreateContactMarker == null
                || EraseContactMarkers == null)
                return;

            EraseContactMarkers();
            
            var targetTimeStamp = t > 0 ? t : _newestFrame.TimeStamp + t;

            // locate closest index using binary search (NOTE FOR FUTURE DEVS: I was too lazy to google a binary search algo so I made this in like a minute)
            int a = 0, b = _contactReports.Count - 1;
            int index = (a + b) / 2;
            while (a != b) {
                var tmpIndex = (a + b) / 2;
                if (tmpIndex == index)
                    break;
                index = tmpIndex;
                if (_contactReports.ElementAt(index).Value.TimeStamp > t) {
                    b = index;
                } else if (_contactReports.ElementAt(index).Value.TimeStamp < t) {
                    a = index;
                } else {
                    break;
                }
            }

            // Before Markers
            int indexBefore = index;
            while (indexBefore > -1) {
                var dist = Math.Abs(_contactReports.ElementAt(indexBefore).Value.TimeStamp - targetTimeStamp);
                if (dist >= giveOrTake)
                    break;
                if (_contactReports.ElementAt(indexBefore).Value.Impulse.magnitude >= impulseThreshold)
                    CreateContactMarker(_contactReports.ElementAt(indexBefore).Value, (giveOrTake - dist) / giveOrTake);
                indexBefore--;
            }
            int indexAfter = index + 1;
            while (indexAfter < _contactReports.Count) {
                var dist = Math.Abs(_contactReports.ElementAt(indexAfter).Value.TimeStamp - targetTimeStamp);
                if (dist >= giveOrTake)
                    break;
                if (_contactReports.ElementAt(indexAfter).Value.Impulse.magnitude >= impulseThreshold)
                    CreateContactMarker(_contactReports.ElementAt(indexAfter).Value, (giveOrTake - dist) / giveOrTake);
                indexAfter++;
            }
        }

        public static void MakeCurrentNewestFrame() {
            if (_newestFrame == null || CurrentFrame == null)
                return;

            var originalNewestTimestamp = _newestFrame.TimeStamp;
            
            while (_newestFrame.TimeStamp > CurrentFrame.TimeStamp) {
                if (_newestFrame == _oldestFrame) {
                    throw new Exception("Current Frame is too outdated");
                }
                _newestFrame = _newestFrame.LastFrame;
            }

            _desyncTime += originalNewestTimestamp - CurrentFrame.TimeStamp;

            while (_contactReports.Count > 0 && _contactReports.ElementAt(_contactReports.Count - 1).Value.TimeStamp > CurrentFrame.TimeStamp) {
                _contactReports.RemoveAt(_contactReports.Count - 1);
            }

            _newestFrame.NextFrame = CurrentFrame;
            CurrentFrame.LastFrame = _newestFrame;
            CurrentFrame.NextFrame = null!;
            _newestFrame = CurrentFrame;

            CurrentFrame = null!;
        }

        public static void Teardown() {
            _startDesync = float.NaN;
            _desyncTime = 0;
            InvalidateRecording();
            CurrentFrame = null!;
            _isRecording = false;
        }
    }

    public class ReplayFrame {
        public float TimeStamp;
        public Dictionary<Rigidbody, RigidbodyFrameData> RigidbodyData = new Dictionary<Rigidbody, RigidbodyFrameData>();

        [JsonConstructor]
        private ReplayFrame() { }

        /// <summary>
        /// Record the current simulation into a <see cref="Synthesis.Replay.ReplayFrame">Replay Frame</see>
        /// </summary>
        /// <returns></returns>
        public static ReplayFrame RecordFrame() {
            var frame = new ReplayFrame();
            frame.TimeStamp = Time.realtimeSinceStartup - ReplayManager.DesyncTime;
            PhysicsManager.GetAllOverridable().ForEach(x => {
                x.GetAllRigidbodies().ForEach(y => {
                    var data = new RigidbodyFrameData {
                        Position = y.transform.position,
                        Rotation = y.transform.rotation,
                        Velocity = y.velocity,
                        AngularVelocity = y.angularVelocity
                    };
                    frame.RigidbodyData[y] = data;
                });
                // PhysicsManager.GetContactRecorders(x)?.ForEach(y => {
                //     frame.ContactReports[x] = y.Reports;
                //     y.Reports = new List<ContactReport>();
                // });
            });
            return frame;
        }

        [JsonIgnore]
        public ReplayFrame NextFrame;
        [JsonIgnore]
        public ReplayFrame LastFrame;

        /// <summary>
        /// Lerps between two frames
        /// </summary>
        /// <param name="a">From Frame</param>
        /// <param name="b">To Frame</param>
        /// <param name="t">t is between 0.0 to 1.0</param>
        /// <returns></returns>
        public static ReplayFrame Lerp(ReplayFrame a, ReplayFrame b, float t) {
            var c = new ReplayFrame();
            c.TimeStamp = Mathf.Lerp(a.TimeStamp, b.TimeStamp, t);
            a.RigidbodyData.ForEach(x => {
                if (b.RigidbodyData.ContainsKey(x.Key)) {
                    var data = new RigidbodyFrameData {
                        Position = Vector3.Lerp(x.Value.Position, b.RigidbodyData[x.Key].Position, t),
                        Rotation = Quaternion.Lerp(x.Value.Rotation, b.RigidbodyData[x.Key].Rotation, t),
                        Velocity = Vector3.Lerp(x.Value.Velocity, b.RigidbodyData[x.Key].Velocity, t),
                        AngularVelocity = Vector3.Lerp(x.Value.AngularVelocity, b.RigidbodyData[x.Key].AngularVelocity, t)
                    };
                    c.RigidbodyData[x.Key] = data;
                    // c.ContactReports = new Dictionary<IPhysicsOverridable, List<ContactReport>>(b.ContactReports);
                }
            });
            return c;
        }

        /// <summary>
        /// Apply this frame to current simulation
        /// </summary>
        public void ApplyFrame() => ApplyFrame(this);
        /// <summary>
        /// Apply a frame to the current simulation
        /// </summary>
        /// <param name="a">Frame to apply</param>
        public static void ApplyFrame(ReplayFrame a) {
            a.RigidbodyData.ForEach(x => {
                x.Key.transform.position = x.Value.Position;
                x.Key.transform.rotation = x.Value.Rotation;
                x.Key.velocity = x.Value.Velocity;
                x.Key.angularVelocity = x.Value.AngularVelocity;
            });
            // if (ReplayManager.ResetContactUI != null) {
            //     ReplayManager.ResetContactUI();
            //     a.ContactReports.ForEach(x => x.Value.ForEach(y => ReplayManager.CreateContactUI!(y)));
            // }
            ReplayManager.CurrentFrame = a;
        }
    }

    public struct RigidbodyFrameData {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public List<ContactReport> ContactReports;
    }
}
