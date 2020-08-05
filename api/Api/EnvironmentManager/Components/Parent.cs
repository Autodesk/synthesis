using SynthesisAPI.Modules.Attributes;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class Parent : Component
    {
        private Entity parentEntity = 0;
        public static implicit operator Entity(Parent p) => p.parentEntity;
        //TODO wont set if entity does not exist
        public void Set(Entity entity)
        {
            parentEntity = entity;
            Changed = true;
        }
        public bool Changed { get; private set; }
        internal void ProcessedChanges() => Changed = false;
    }
}
