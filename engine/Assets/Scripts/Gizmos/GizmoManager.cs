using System;
using Synthesis.Physics;
using Synthesis.Runtime;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;

#nullable enable

namespace Synthesis.Gizmo {
    /// <summary>
    /// Manages Gizmos and ensures only one gizmo spawns at a time.
    /// </summary>
    public static class GizmoManager {
        private static GizmoConfig? _currentGizmoConfig;
        public static GizmoConfig? CurrentGizmoConfig => _currentGizmoConfig;
        private static Transform? _currentTargetTransform;

        static GizmoManager() {
            SimulationRunner.OnUpdate += UpdateGizmo;

            InputManager.AssignDigitalInput(
                "exit-gizmo", new Digital("Return", context: SimulationRunner.GIZMO_SIM_CONTEXT), e => ExitGizmo());
        }

        public static void SpawnGizmo<T>(T gizmoObject)
            where T : IGizmo => SpawnGizmo(new GizmoConfig { Transform          = gizmoObject.GetGizmoData(),
                                                                 UpdateCallback = gizmoObject.Update,
                                                                 EndCallback    = gizmoObject.End });

        public static void SpawnGizmo(TransformData data, Action<TransformData> update,
            Action<TransformData> end) => SpawnGizmo(new GizmoConfig { Transform = data, UpdateCallback = update,
            EndCallback = end });

        public static void SpawnGizmo(Vector3 position, Quaternion rotation, Action<TransformData> update,
            Action<TransformData> end) => SpawnGizmo(new GizmoConfig {
            Transform = new TransformData {Position = position, Rotation = rotation},
                UpdateCallback = update,
            EndCallback = end
        });

        public static void SpawnGizmo(GizmoConfig config) {
            if (_currentGizmoConfig.HasValue)
                ExitGizmo();

            // Check if modal is opened?
            SimulationRunner.AddContext(SimulationRunner.GIZMO_SIM_CONTEXT);
            _currentGizmoConfig = config;

            // Really duct-taped way of doing this
            _currentTargetTransform          = new GameObject("GIZMO_MARKER").transform;
            _currentTargetTransform.position = _currentGizmoConfig.Value.Transform.Position;
            _currentTargetTransform.rotation = _currentGizmoConfig.Value.Transform.Rotation;

            var gizmo  = GameObject.Instantiate(SynthesisAssetCollection.GizmoPrefabStatic, _currentTargetTransform);
            gizmo.name = "GIZMO";
        }

        private static void UpdateGizmo() {
            if (!_currentGizmoConfig.HasValue || _currentTargetTransform == null)
                return;

            _currentGizmoConfig.Value.UpdateCallback(_currentTargetTransform);
        }

        public static void ExitGizmo() {
            if (!_currentGizmoConfig.HasValue || _currentTargetTransform == null) {
                return;
            }

            SimulationRunner.RemoveContext(SimulationRunner.GIZMO_SIM_CONTEXT);

            _currentGizmoConfig.Value.EndCallback(_currentTargetTransform);
            GameObject.Destroy(_currentTargetTransform.gameObject);
            // GameObject.Destroy(gizmo);

            _currentGizmoConfig     = null;
            _currentTargetTransform = null;
        }
    }

    public struct TransformData {
        public static TransformData Default =
            new TransformData { Position = Vector3.zero, Rotation = Quaternion.identity };

        public Vector3 Position;
        public Quaternion Rotation;

        public static implicit operator TransformData(
            Vector3 position) => new TransformData { Position = position, Rotation = Quaternion.identity };
        public static implicit operator TransformData(
            Quaternion rotation) => new TransformData { Position = Vector3.zero, Rotation = rotation };
        public static implicit operator TransformData(
            (Vector3, Quaternion) data) => new TransformData { Position = data.Item1, Rotation = data.Item2 };
        public static implicit operator TransformData(
            Transform t) => new TransformData { Position = t.position, Rotation = t.rotation };
    }

    public struct GizmoConfig {
        public TransformData Transform;
        public Action<TransformData> UpdateCallback;
        public Action<TransformData> EndCallback;
    }

    public interface IGizmo {
        public TransformData GetGizmoData();
        public void Update(TransformData data);
        public void End(TransformData data);
    }
}
