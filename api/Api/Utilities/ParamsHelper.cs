﻿using System;

namespace SynthesisAPI.Utilities
{
    public static class ParamsHelper
    {
        public static object[] CollapseTuple<T1, T2>((T1 t1, T2 t2) t) => new object[] {t.t1!, t.t2!};
        public static object[] CollapseTuple<T1, T2, T3>((T1 t1, T2 t2, T3 t3) t) => new object[] {t.t1!, t.t2!, t.t3!};
        public static object[] CollapseTuple<T1, T2, T3, T4>((T1 t1, T2 t2, T3 t3, T4 t4) t) => new object[] {t.t1!, t.t2!, t.t3!, t.t4!};


        public static (T1, T2) PackParams<T1, T2>(params object[] args)
        {
            if(args.Length != 2)
                throw new Exception("Invalid argument count.");
            return ((T1) args[0], (T2) args[1]);
        }

        public static (T1, T2, T3) PackParams<T1, T2, T3>(params object[] args)
        {
            if(args.Length != 3)
                throw new Exception("Invalid argument count.");
            return ((T1) args[0], (T2) args[1], (T3) args[2]);
        }

        public static (T1, T2, T3, T4) PackParams<T1, T2, T3, T4>(params object[] args)
        {
            if(args.Length != 4) 
                throw new Exception("Invalid argument count.");
            return ((T1) args[0], (T2) args[1], (T3) args[2], (T4) args[3]);
        }
    }
}
