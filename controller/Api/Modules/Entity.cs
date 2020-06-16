using Api.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Modules
{
    class Entity
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
