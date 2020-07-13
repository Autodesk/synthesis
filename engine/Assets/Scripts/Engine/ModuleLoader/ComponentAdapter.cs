using UnityEngine;
namespace Engine.ModuleLoader
{
	public class ComponentAdapter : MonoBehaviour, IApiAdapter<Component>
	{
		private Component component;
		public void SetInstance(Component mesh)
		{
			component = mesh;
		}
	}
}