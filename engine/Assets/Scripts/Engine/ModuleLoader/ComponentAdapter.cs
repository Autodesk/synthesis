using SynthesisAPI.EnvironmentManager;
namespace Engine.ModuleLoader
{
	public class ComponentAdapter : UnityEngine.MonoBehaviour, IApiAdapter<Component>
	{
		private Component component;
		public void SetInstance(Component component)
		{
			this.component = component;
		}
	}
}