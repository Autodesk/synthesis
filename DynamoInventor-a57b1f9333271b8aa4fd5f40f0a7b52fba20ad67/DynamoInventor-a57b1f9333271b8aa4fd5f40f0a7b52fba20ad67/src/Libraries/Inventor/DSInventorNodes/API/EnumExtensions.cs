using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventorLibrary.API
{
    public static class EnumExtensions
    {
        public static T As<T>(this InvDocumentTypeEnum c) where T : struct
        {
            return (T)System.Enum.Parse(typeof(T), c.ToString(), false);
        }

        public static T As<T>(this Inventor.DocumentTypeEnum c) where T : struct
        {
            return (T)System.Enum.Parse(typeof(T), c.ToString(), false);
        }

        public static T As<T>(this InvObjectTypeEnum c) where T : struct
        {
            return (T)System.Enum.Parse(typeof(T), c.ToString(), false);
        }

        public static T As<T>(this Inventor.ObjectTypeEnum c) where T : struct
        {
            return (T)System.Enum.Parse(typeof(T), c.ToString(), false);
        }
    }
}
