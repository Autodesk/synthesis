using SynthesisAPI.EnvironmentManager;

namespace SynthesisAPI.InputManager
{
    /// <summary>
    /// A behavior to actively update the InputManager
    /// </summary>
    public class InputSystem : SystemBase
    {
        public override void Setup() { }

        /// <summary>
        /// Updates all the key presses.
        /// </summary>
        public override void OnUpdate()
        {
            InputManager.UpdateControllerTypes();
            InputManager.UpdateDigitalStates();
        }

        public override void OnPhysicsUpdate() { }
    }
}
