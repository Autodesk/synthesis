using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InventorServices.Persistence
{
    [Serializable]
    public class ObjectId : ISerializableId<byte[]>,
                            ISerializable
    {
        private byte[] _id;
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("referenceKey", Id, Id.GetType());
        }

        public ObjectId()
        {
            _id = new byte[]{};
        }

        public ObjectId(SerializationInfo info, StreamingContext context)
        {
            Id = (byte[])info.GetValue("referenceKey", typeof(byte[]));
        }
        public byte[] Id
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}
