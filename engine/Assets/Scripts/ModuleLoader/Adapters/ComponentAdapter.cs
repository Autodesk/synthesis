using SynthesisAPI.EnvironmentManager;
namespace Engine.ModuleLoader.Adapters
{
	public class ComponentAdapter : UnityEngine.MonoBehaviour, IApiAdapter<Component>
	{
		public string Name;

		private Component component;

		public void Awake()
		{
			if (component == null)
			{
				gameObject.SetActive(false);
			}
		}

		public void SetInstance(Component component)
		{
			this.component = component;
			Name = this.component.GetType().FullName;
			gameObject.SetActive(true);
		}
	}
}