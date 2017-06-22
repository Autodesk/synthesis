using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FSM
{
    public abstract class SimState
    {
        public virtual void Start() { }
        public virtual void End() { }
        public virtual void Pause() { }
        public virtual void Resume() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void OnGUI() { }
        public virtual void Awake() { }
    }
}
