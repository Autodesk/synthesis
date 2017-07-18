using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.FSM
{
    /// <summary>
    /// The base class for any state.
    /// </summary>
    public abstract class SimState
    {
        /// <summary>
        /// Called when the SimState is started.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Called when the SimState is ended.
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Called when the SimState is paused.
        /// </summary>
        public virtual void Pause() { }

        /// <summary>
        /// Called when the SimState is resumed.
        /// </summary>
        public virtual void Resume() { }

        /// <summary>
        /// Called when the SimState is updated.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Called when the SimState is updated late.
        /// </summary>
        public virtual void LateUpdate() { }

        /// <summary>
        /// Called when the SimState is updated on a fixed basis.
        /// </summary>
        public virtual void FixedUpdate() { }

        /// <summary>
        /// Called when the GUI is updated in the SimState.
        /// </summary>
        public virtual void OnGUI() { }

        /// <summary>
        /// Called when the SimState is awaken.
        /// </summary>
        public virtual void Awake() { }
    }
}
