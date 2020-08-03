using System;
using SynthesisAPI.Modules.Attributes;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class Parent : Component
    {
        private Entity value = 0;
        public static implicit operator Entity(Parent p) => p.value;
        //TODO wont set if entity does not exist
        public void Set(Entity entity) => value = entity;
        public bool Changed { get; private set; }
        internal void ProcessedChanges() => Changed = false;
    }
}
