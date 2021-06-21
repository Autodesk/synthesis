using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Import;
using Synthesis.ModelManager;
using UnityEditor;
using UnityEngine;

namespace Synthesis.Entitys {

    public static class EntityManager {

        private static Dictionary<Guid, Entity> _entities = new Dictionary<Guid, Entity>();

        public static Guid RegisterEntity(Entity entity) {
            var g = Guid.NewGuid();
            _entities.Add(g, entity);
            entity.GUID = g;
            return g;
        }

        public static List<(Guid GUID, T robot)> GetEntitiesOfType<T>() where T : Entity {
            var res = new List<(Guid GUID, T robot)>();
            foreach (var kvp in _entities) {
                if (kvp.Value is T)
                    res.Add((kvp.Key, kvp.Value as T));
            }
            return res;
        }
        
    }

    public class Entity {

        public Guid GUID;
        public GameObject RootObject;

        public Entity(GameObject rootObject) {
            RootObject = rootObject;
        }
    }

    public class Robot: Entity {

        public string Name => _meta.Name;
        private DynamicObjectMeta _meta;
        public Dictionary<Guid, GameObject> Nodes => _meta.Nodes;
        public Dictionary<Guid, EntityFlag> Tags => _meta.Flags;
        
        public float MaxSpeed = 1000;
        
        public bool[] WheelConfig;
        
        public List<HingeJoint> Wheels = new List<HingeJoint>();
        public List<HingeJoint> Arms = new List<HingeJoint>();
        
        public Robot( GameObject rootObject, DynamicObjectMeta meta) : base(rootObject) {
            _meta = meta;
            var wheelMotor = new JointMotor();
            wheelMotor.force = 100;
            wheelMotor.freeSpin = true;
            wheelMotor.targetVelocity = MaxSpeed;
            
            Nodes.ForEach((key, value) => {
                if (HasFlag(key, EntityFlag.Hinge)) {
                    var hinge = value.GetComponent<HingeJoint>();
                    if (HasFlag(key, EntityFlag.Wheel)) {
                        hinge.motor = wheelMotor;
                        hinge.useMotor = true;
                        Wheels.Add(hinge);
                        Debug.Log("Adding motor");
                    } else {
                        Arms.Add(hinge);
                    }
                }
            });
            
            WheelConfig = new bool[Wheels.Count()];
        }
        
        public bool HasFlag(Guid guid, EntityFlag flag) => _meta.HasFlag(guid, flag);
    }
}
