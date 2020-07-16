using Entity = System.UInt32;

namespace SynthesisAPI.EnvironmentManager
{
	public class Component
	{
		protected Entity? Entity { get; private set; } = null;

		internal void SetEntity(Entity entity)
		{
			Entity = entity;
		}
	}
}