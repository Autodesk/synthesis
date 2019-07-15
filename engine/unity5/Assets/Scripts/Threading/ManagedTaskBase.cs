namespace Synthesis
{
    public abstract class ManagedTaskBase
    {
        public abstract void OnExit();
        public abstract void OnPause();
        public abstract void OnResume();
        public abstract void OnStart();
        public abstract void OnStop();
        public abstract void OnCycle();
        public abstract void OnMessage();
    }
}
