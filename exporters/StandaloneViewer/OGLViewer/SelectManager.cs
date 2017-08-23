using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGLViewer
{
    public class SelectManager
    {
        // For selections
        private static readonly Stack<UInt32> FREE_NODE_IDS = new Stack<UInt32>();
        private static UInt32 NODES_CREATED = 0;
        private static readonly Dictionary<UInt32, object> NODE_MAPPING = new Dictionary<UInt32, object>();

        public static object GetByGUID(UInt32 guid)
        {
            object output;
            if (NODE_MAPPING.TryGetValue(guid, out output))
            {
                return output;
            }
            else
            {
                return null;
            }
        }

        public static UInt32 AllocateGUID(object obj)
        {
            UInt32 myGUID;
            if (FREE_NODE_IDS.Count == 0)
            {
                myGUID = ++NODES_CREATED;
            }
            else
            {
                myGUID = FREE_NODE_IDS.Pop();
            }
            try
            {
                NODE_MAPPING.Add(myGUID, obj);
            }
            catch
            {
                NODE_MAPPING[myGUID] = obj;
            }
            return myGUID;
        }

        public static bool FreeGUID(UInt32 guid)
        {
            if (guid == NODES_CREATED)
            {
                NODES_CREATED--;
            }
            else
            {
                FREE_NODE_IDS.Push(guid);
            }
            return NODE_MAPPING.Remove(guid);
        }

        public static byte[] GUIDToColor(uint guid)
        {
            return BitConverter.GetBytes(guid);
        }

        public static UInt32 ColorToGUID(byte[] data)
        {
            return BitConverter.ToUInt32(data, 0);
        }
    }
}
