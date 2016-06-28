using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

namespace InventorServices.Persistence
{
    public class ReferenceManager
    {
        public static ReferenceKeyManager KeyManager { get; set; }

        public static int? KeyContext { get; set; }

        public static byte[] KeyContextArray { get; set; }
    }
}
