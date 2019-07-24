using System;
using System.Collections.Generic;

namespace BxDRobotExporter.OGLViewer
{
    public class SelectManager
    {
        // For selections
        private static readonly Stack<UInt32> FreeNodeIds = new Stack<UInt32>();
        private static UInt32 nodesCreated = 0;
        private static readonly Dictionary<UInt32, object> NodeMapping = new Dictionary<UInt32, object>();

        public static object GetByGuid(UInt32 guid)
        {
            object output;
            if (NodeMapping.TryGetValue(guid, out output))
            {
                return output;
            }
            else
            {
                return null;
            }
        }

        public static UInt32 AllocateGuid(object obj)
        {
            UInt32 myGuid;
            if (FreeNodeIds.Count == 0)
            {
                myGuid = ++nodesCreated;
            }
            else
            {
                myGuid = FreeNodeIds.Pop();
            }
            try
            {
                NodeMapping.Add(myGuid, obj);
            }
            catch
            {
                NodeMapping[myGuid] = obj;
            }
            return myGuid;
        }

        public static bool FreeGuid(UInt32 guid)
        {
            if (guid == nodesCreated)
            {
                nodesCreated--;
            }
            else
            {
                FreeNodeIds.Push(guid);
            }
            return NodeMapping.Remove(guid);
        }

        public static byte[] GuidToColor(uint guid)
        {
            return BitConverter.GetBytes(guid);
        }

        public static UInt32 ColorToGuid(byte[] data)
        {
            return BitConverter.ToUInt32(data, 0);
        }
    }
}
