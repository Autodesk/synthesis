namespace Engine.ModuleLoader.Adapters {
    public interface IApiAdapter<in TBase> {
        void SetInstance(TBase baseTypeInstance);
    }
}