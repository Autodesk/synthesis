using System.Collections.Generic;

#nullable enable

namespace Synthesis.Import {
    public static class MirabufCache {
        private const short MAX_CACHE_SIZE = 4;

        private static LinkedList<MirabufLive> _cache = new();

        public static MirabufLive Get(string path) {
            LinkedListNode<MirabufLive>? cursor = _cache.First;
            while (cursor != null && !cursor.Value.MiraPath.Equals(path)) {
                cursor = cursor.Next;
            }

            if (cursor?.Value.MiraPath.Equals(path) ?? false) {
                _cache.Remove(cursor);
                _cache.AddFirst(cursor);
                return cursor.Value;
            } else {
                MirabufLive mira = new MirabufLive(path);
                _cache.AddFirst(mira);
                while (_cache.Count > MAX_CACHE_SIZE) {
                    _cache.RemoveLast();
                }
                return mira;
            }
        }

        public static void Clear() {
            _cache.Clear();
        }
    }
}
