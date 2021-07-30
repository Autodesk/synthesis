using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class Extensions {
    public static Vector2[] ToVector2Array(this IEnumerable<float> data) {
        Vector2[] vertices = new Vector2[data.Count() / 2];
        for (int i = 0; i < data.Count(); i += 2) {
            // TODO: Flip the X
            vertices[i / 2] = new Vector2(data.ElementAt(i), data.ElementAt(i + 1));
        }
        return vertices;
    }
    public static Vector3[] ToVector3Array(this IEnumerable<float> data, bool normalize = false) {
        Vector3[] vertices = new Vector3[data.Count() / 3];
        for (int i = 0; i < data.Count(); i += 3) {
            // TODO: Flip the X
            vertices[i / 3] = new Vector3(data.ElementAt(i) * -0.01f, data.ElementAt(i + 1) * 0.01f, data.ElementAt(i + 2) * 0.01f);
            if (normalize)
                vertices[i / 3] = vertices[i / 3].normalized;
        }
        return vertices;
    }
}
