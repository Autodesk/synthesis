namespace SynthesisAPI.EnvironmentManager
{
    public class Entity
    {
        public ulong GetId() => _id;

        public Entity()
        {
            Instantiate();
        }

        private void Instantiate()
        {
            // ApiProvider.Instantiate(this, ref _id); // TODO
        }


        private ulong _id;
    }
}
