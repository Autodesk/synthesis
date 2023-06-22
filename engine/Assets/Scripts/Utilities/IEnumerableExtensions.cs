using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Engine.Util {
    public static class IEnumerableExtensions {
        public static T PopAt<T>(this IList<T> list, int index) {
            T r = list.ElementAt(index);
            list.RemoveAt(index);
            return r;
        }
    }
}
