using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.Utilities
{
    public static class GeneralHelper
    {
        public static TResult Convert<TResult>(this Enum i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }
}
