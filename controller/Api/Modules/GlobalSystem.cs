namespace SynthesisAPI.Modules
{
	/// <summary>
	/// <c>GlobalBehavior</c> encapsulates a script that is initialized at plugin load, that
	/// lives in the background, running constantly.
	/// </summary>
	/// <typeparam name="TSystem"></typeparam>
	public class GlobalSystem<TSystem> : Object where TSystem : System
	{
		protected GlobalSystem()
		{
			AddComponent<TSystem>();
		}
	}
}