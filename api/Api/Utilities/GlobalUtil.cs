﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;

internal static class GlobalUtil {

    public static V TryGetDefault<K, V>(this Dictionary<K, V> dict, K key) where V : struct {
        var success = dict.TryGetValue(key, out V res);
        return success ? res : default;
    }

    public static V TryGetDefault<K, V>(this Dictionary<K, V> dict, K key, V defa) {
        var success = dict.TryGetValue(key, out V res);
        return success ? res : defa;
    }

    public static Task<U> CastTask<T, U>(this Task<T> task, int timeout = -1) where U : class
        => Task<U>.Factory.StartNew(() => {
            if (timeout < 0) {
                while (!task.IsCompleted) {
                    task.Wait();
                }
            } else {
                task.Wait(timeout);
            }
            return task.Result as U;
        });

}
