using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities {
    public class DynamicLayerManager {
        private static readonly int FIELD_LAYER = LayerMask.NameToLayer("FieldCollisionLayer");

        private static readonly Queue<int> _dynamicLayers = new(new[] {
            LayerMask.NameToLayer("D1CollisionLayer"),
            LayerMask.NameToLayer("D2CollisionLayer"),
            LayerMask.NameToLayer("D3CollisionLayer"),
            LayerMask.NameToLayer("D4CollisionLayer"),
            LayerMask.NameToLayer("D5CollisionLayer"),
            LayerMask.NameToLayer("D6CollisionLayer"),
        });

        public static int NextRobotLayer {
            get {
                if (_dynamicLayers.Count == 0)
                    throw new Exception("No more dynamic layers");

                return _dynamicLayers.Dequeue();
            }
        }

        public static void ReturnRobotLayer(int layer) => _dynamicLayers.Enqueue(layer);

        public static int FieldLayer => FIELD_LAYER;
    }

    public class DynamicLayerReserver : MonoBehaviour {
        private void OnDestroy() {
            DynamicLayerManager.ReturnRobotLayer(gameObject.layer);
        }
    }
}