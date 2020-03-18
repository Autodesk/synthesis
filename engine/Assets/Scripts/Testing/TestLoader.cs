using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Simulator;
using Google.Protobuf.Collections;
using Google.Protobuf;
using System.IO;
using Google.Protobuf.WellKnownTypes;

public class TestLoader : MonoBehaviour
{
    public GameObject obj;
    public GameObject loadedInto;
    public Material m;

    public void Save()
    {
        Mesh m = obj.GetComponent<MeshFilter>().mesh;

        ProtoObject objFile = new ProtoObject();
        objFile.Position = ToProtoVector3(obj.transform.position);

        for (int i = 0; i < m.vertexCount; i++)
        {
            objFile.Verts.Add(ToProtoVector3(m.vertices[i]));
        }
        for (int i = 0; i < m.uv.Length; i++)
        {
            objFile.Uv.Add(ToProtoVector2(m.uv[i]));
        }
        for (int i = 0; i < m.triangles.Length; i++)
        {
            objFile.Tris.Add(m.triangles[i]);
        }

        FileStream fs = new FileStream("SavedObject.syn", FileMode.Create);
        objFile.WriteTo(fs);

        fs.Flush();
        fs.Close();
    }

    public void Load()
    {
        loadedInto = new GameObject();

        FileStream fs = new FileStream("SavedObject.syn", FileMode.Open);
        ProtoObject objFile = ProtoObject.Parser.ParseFrom(fs);
        fs.Close();

        Mesh newMesh = new Mesh();
        Vector3[] verts = new Vector3[objFile.Verts.Count];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = ToUnityVector3(objFile.Verts[i]);
        }
        Vector2[] uv = new Vector2[objFile.Uv.Count];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = ToUnityVector2(objFile.Uv[i]);
        }
        int[] tris = new int[objFile.Tris.Count];
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = objFile.Tris[i];
        }
        newMesh.vertices = verts;
        newMesh.triangles = tris;
        newMesh.uv = uv;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        loadedInto.transform.position = ToUnityVector3(objFile.Position);

        loadedInto.AddComponent<MeshFilter>();
        loadedInto.AddComponent<MeshRenderer>();
        loadedInto.GetComponent<MeshFilter>().mesh = newMesh;
        loadedInto.GetComponent<MeshRenderer>().material = m;
    }

    #region Conversions from Unity objects to Proto objects

    public Vector3 ToUnityVector3(ProtoVector3 vect)
    {
        return new Vector3(vect.X, vect.Y, vect.Z);
    }

    public ProtoVector3 ToProtoVector3(Vector3 vect)
    {
        return new ProtoVector3() { X = vect.x, Y = vect.y, Z = vect.z };
    }

    public Vector2 ToUnityVector2(ProtoVector2 vect)
    {
        return new Vector2(vect.X, vect.Y);
    }

    public ProtoVector2 ToProtoVector2(Vector2 vect)
    {
        return new ProtoVector2() { X = vect.x, Y = vect.y };
    }

    #endregion
}
