namespace SynthesisAPI.Modules
{
	public class SystemBase : Component
	{
		public virtual void Start() {}
		public virtual void Update() {}
		public virtual void FixedUpdate() {}
		public virtual void LateUpdate() {}
		public virtual void OnGUI() {}
		public virtual void OnDisable() {}
		public virtual void OnEnable() {}

	}
}