namespace SynthesisAPI.Modules
{
	/// <summary>
	/// <c>GlobalBehavior</c> encapsulates a script that is initialized at plugin load, that
	/// lives in the background, running constantly.
	/// </summary>
	/// <typeparam name="TBehavior"></typeparam>
	public class GlobalBehavior<TBehavior> : Object where TBehavior : Behavior
	{
		protected GlobalBehavior()
		{
			AddComponent<TBehavior>();
		}
	}
}