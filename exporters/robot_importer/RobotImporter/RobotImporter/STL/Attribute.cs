using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotImporter.STL
{
    class Attribute
    {
        public enum AttribType { }

        public readonly AttribType Type;
        public readonly UInt32 AttributeID;
        public readonly UInt32 GlobalID;
       
        /// <summary>
        /// Note: mass, xScale, yScale, zScale, and gScale are nullable, but an exception will be thrown if they are null and values are expected.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributeID"></param>


        public Attribute(AttribType type, UInt32 attributeID, UInt32 globalID)
        {
            Type = type;
            AttributeID = attributeID;
            GlobalID = globalID;
        
        }
    }
}

