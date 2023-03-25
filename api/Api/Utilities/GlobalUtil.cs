using System;
using System.Collections.Generic;
using System.Text;

internal static class GlobalUtil {

    public static V TryGetDefault<K, V>(this Dictionary<K, V> dict, K key) where V : struct {
        var success = dict.TryGetValue(key, out V res);
        return success ? res : default;
    }

    public static V TryGetDefault<K, V>(this Dictionary<K, V> dict, K key, V defa) {
        var success = dict.TryGetValue(key, out V res);
        return success ? res : defa;
    }

}
