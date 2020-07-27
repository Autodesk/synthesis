namespace SynthesisAPI.EnvironmentManager
{
	public class Component
	{
		public Entity? Entity { get; private set; } = null;

		internal void SetEntity(Entity entity)
		{
			Entity = entity;
		}
	}
}