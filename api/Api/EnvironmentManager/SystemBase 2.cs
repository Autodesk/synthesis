namespace SynthesisAPI.EnvironmentManager
{
	public abstract class SystemBase : Component
	{
		/// <summary>
		/// Called once to setup the SystemBase
		/// </summary>
		public abstract void Setup();

		/// <summary>
		/// Called on frame update
		/// </summary>
		public abstract void OnUpdate();

		/// <summary>
		/// Called on physics update
		/// </summary>
		public abstract void OnPhysicsUpdate();
	}
}