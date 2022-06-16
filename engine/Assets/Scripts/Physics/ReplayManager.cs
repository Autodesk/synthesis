using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Synthesis.Physics;
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
        private static ReplayFrame? _oldestFrame = null;
        public static ReplayFrame CurrentFrame;

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

        public static void MakeCurrentNewestFrame() {
            if (_newestFrame == null || CurrentFrame == null)
                return;
            
            while (_newestFrame.TimeStamp > CurrentFrame.TimeStamp) {
                if (_newestFrame == _oldestFrame) {
                    throw new Exception("Current Frame is too outdated");
                }
                _newestFrame = _newestFrame.LastFrame;
            }

            _newestFrame.NextFrame = CurrentFrame;
            CurrentFrame.LastFrame = _newestFrame;
            CurrentFrame.NextFrame = null!;
            _newestFrame = CurrentFrame;

            CurrentFrame = null!;
        }
    }

    public class ReplayFrame {
        public float TimeStamp;
        public Dictionary<Rigidbody, RigidbodyFrameData> RigidbodyData = new Dictionary<Rigidbody, RigidbodyFrameData>();

        [JsonConstructor]
        private ReplayFrame() { }

        public static ReplayFrame RecordFrame() {
            var frame = new ReplayFrame();
            frame.TimeStamp = Time.realtimeSinceStartup;
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
            });
            return frame;
        }

        [JsonIgnore]
        public ReplayFrame NextFrame;
        [JsonIgnore]
        public ReplayFrame LastFrame;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
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
                }
            });
            return c;
        }
        public void ApplyFrame() => ApplyFrame(this);
        public static void ApplyFrame(ReplayFrame a) {
            a.RigidbodyData.ForEach(x => {
                x.Key.transform.position = x.Value.Position;
                x.Key.transform.rotation = x.Value.Rotation;
                x.Key.velocity = x.Value.Velocity;
                x.Key.angularVelocity = x.Value.AngularVelocity;
            });
            ReplayManager.CurrentFrame = a;
        }
    }

    public struct RigidbodyFrameData {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
    }
}
