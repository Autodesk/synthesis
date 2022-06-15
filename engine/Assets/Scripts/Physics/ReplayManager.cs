using System.Collections;
using System.Collections.Generic;
using Synthesis.Physics;
using UnityEngine;

namespace Synthesis.Replay {
    public static class ReplayManager {
        private static bool _isRecording = false;
        public static bool IsRecording {
            get => _isRecording;
            set {
                // TODO
            }
        }
        private static float _timeSpan = 5f;
        private static ReplayFrame _newestFrame = null;
        private static ReplayFrame _oldestFrame = null;

        public static void InvalidateRecording() {
            _newestFrame = null;
            _oldestFrame = null;
        }

        public static void RecordFrame() {
            var newFrame = ReplayFrame.RecordFrame();
            if (_newestFrame == null) {
                _newestFrame = newFrame;
                _oldestFrame = newFrame;
            } else {
                newFrame.LastFrame = _newestFrame;
                _newestFrame.NextFrame = newFrame;
                _newestFrame = newFrame;
                while (_newestFrame.TimeStamp - _oldestFrame.TimeStamp > _timeSpan) {
                    _oldestFrame = _oldestFrame.NextFrame;
                    if (_oldestFrame == _newestFrame)
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">Use negative range to show relative to the latest frame</param>
        public static void ShowTime(float t) {

        }
    }

    public class ReplayFrame {
        public float TimeStamp;
        public Dictionary<Rigidbody, RigidbodyFrameData> RigidbodyData;

        private ReplayFrame() { }

        public static ReplayFrame RecordFrame() {
            var frame = new ReplayFrame();
            PhysicsManager.GetAllOverridable().ForEach(x => {
                x.GetAllRigidbodies().ForEach(y => {
                    var data = new RigidbodyFrameData {
                        Position = y.transform.position,
                        Rotation = y.transform.rotation,
                        Velocity = y.velocity,
                        AngularVelocity = y.angularVelocity
                    };
                    frame.RigidbodyData[y] = data;
                    frame.TimeStamp = Time.realtimeSinceStartup;
                });
            });
            return frame;
        }

        public ReplayFrame NextFrame;
        public ReplayFrame LastFrame;

        public static ReplayFrame Lerp(ReplayFrame a, ReplayFrame b, float val) { return null; }
        public static void ApplyFrame(ReplayFrame a) { }
    }

    public struct RigidbodyFrameData {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
    }
}
