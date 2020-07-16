using Entity = System.UInt32;

namespace SynthesisAPI.EnvironmentManager
{
	public class Component
	{
		protected Entity? entity { get; private set; } = null;

		internal void SetEntity(Entity entity)
		{
			this.entity = entity;
		}
	}
}