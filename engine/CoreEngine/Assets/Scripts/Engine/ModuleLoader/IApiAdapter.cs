namespace Engine.ModuleLoader
{
	public interface IApiAdapter<in TBase>
	{
		void SetInstance(TBase mesh);
	}
}