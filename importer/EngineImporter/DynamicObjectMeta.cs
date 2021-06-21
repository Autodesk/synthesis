using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Synthesis.Proto;

namespace Synthesis.Import {
    
    /// <summary>
    /// Meta class to pass on data from the deserialized objects
    ///
    /// TODO: Get rid of this?
    /// </summary>
    public class DynamicObjectMeta : EntityMeta {

        public string Name;
        public Dictionary<Guid, GameObject> Nodes;
        public Dictionary<Guid, EntityFlag> Flags = new Dictionary<Guid, EntityFlag>();

        public void Init(string name, Dictionary<Guid, GameObject> nodes) {
            Name = name;
            Nodes = nodes;
        }

        public void AddFlag(Guid guid, EntityFlag flag) {
            if (!Flags.ContainsKey(guid))
                Flags[guid] = 0x00;

            Flags[guid] = Flags[guid] | flag;
        }

        public void RemoveFlag(Guid guid, EntityFlag flag) {
            if (!Flags.ContainsKey(guid))
                return;

            Flags[guid] = Flags[guid] ^ flag;
        }

        public bool HasFlag(Guid guid, EntityFlag flag) {
            if (!Flags.ContainsKey(guid))
                return false;

            return Flags[guid] == (Flags[guid] | flag);
        }

        public class ComponentRequest : MonoBehaviour {
            public string ComponentName;
            public Dictionary<string, object> ComponentProperties = new Dictionary<string, object>();
        }

        public class NodeSource : MonoBehaviour {
            public Node SourceNode;
            public float collectiveMass;
        }
    }
    
    [Flags]
    public enum EntityFlag {
        Hinge = 0x01,
        Wheel = 0x02
    }
}
