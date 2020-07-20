using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockApi
{
    public static class Mock
    {
        public static void Init()
        {
            ApiProvider.RegisterApiProvider(new ApiInstance());
        }
    }
}
