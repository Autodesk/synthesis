using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synthesis.FSM
{
    /// <summary>
    /// The base class for any state.
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// The <see cref="FSM.StateMachine"/> running this state.
        /// </summary>
        public StateMachine StateMachine { get; set; }

        /// <summary>
        /// Called when the State is awaken.
        /// </summary>
        public virtual void Awake() { }

        /// <summary>
        /// Called when the State is started.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Called when the State is ended.
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Called when the State is paused.
        /// </summary>
        public virtual void Pause() { }

        /// <summary>
        /// Called when the State is resumed.
        /// </summary>
        public virtual void Resume() { }

        /// <summary>
        /// Called when the State is updated.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Called when the State is updated late.
        /// </summary>
        public virtual void LateUpdate() { }

        /// <summary>
        /// Called when the State is updated on a fixed basis.
        /// </summary>
        public virtual void FixedUpdate() { }

        /// <summary>
        /// Called when the GUI is updated in the State.
        /// </summary>
        public virtual void OnGUI() { }

        /// <summary>
        /// Called to hide or unhide the display.
        /// </summary>
        public virtual void ToggleHidden() { }
    }
}
