using SynthesisAPI.Modules.Attributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class Parent : Component
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal Entity parentEntity = 0;
        public static implicit operator Entity(Parent p) => p.parentEntity;
        //TODO wont set if entity does not exist

        public Entity ParentEntity {
            get => parentEntity;
            set {
                parentEntity = value;
                OnPropertyChanged();
            }
        }

        public bool Changed { get; private set; }
        internal void ProcessedChanges() => Changed = false;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
