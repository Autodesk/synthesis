using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Simulator
{
    /**
     * This class is responsible for handling robots, fields, and will act as the middle man between
     * elements of the simulator and all other components (DriverPractice, Controller, etc.)
     */
    public class SimulatorHandler
    {
        public event EventHandler<ProtoField> FieldSpawned;

        private SimulatorHandler() { }

        #region Load Functions

        public void LoadField(ProtoField field, Transform parent = null, Vector3? spawnposition = null, Quaternion? spawnRotation = null)
        {
            GameObject fieldObj = new GameObject(field.FieldName);

            for (int i = 0; i < field.Nodes.Count; i++)
            {
                LoadNode(field.Nodes[i], fieldObj.transform);
            }

            fieldObj.transform.position = spawnposition ?? Vector3.zero;
            fieldObj.transform.rotation = spawnRotation ?? Quaternion.Euler(Vector3.zero);
            if (parent != null) fieldObj.transform.parent = parent;
        }

        public void LoadNode(ProtoNode node, Transform parent = null, Vector3? spawnposition = null, Quaternion? spawnRotation = null)
        {
            GameObject nodeObj = new GameObject(node.Name);

            for (int i = 0; i < node.Bodies.Count; i++)
            {
                LoadBody(node.Bodies[i], nodeObj.transform);
            }

            nodeObj.transform.position = spawnposition ?? Vector3.zero;
            nodeObj.transform.rotation = spawnRotation ?? Quaternion.Euler(Vector3.zero);
            if (parent != null) nodeObj.transform.parent = parent;
        }

        public void LoadBody(ProtoObject obj, Transform parent = null, Vector3? spawnposition = null, Quaternion? spawnRotation = null)
        {
            Vector3[] verts = new Vector3[obj.Verts.Count];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = new Vector3(obj.Verts[i].X, obj.Verts[i].Y, obj.Verts[i].Z); // - position;
            }
            Vector2[] uvs = new Vector2[obj.Uv.Count];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2(obj.Uv[i].X, obj.Uv[i].Y);
            }
            int[] tris = new int[obj.Tris.Count];
            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = obj.Tris[i];
            }
            System.Array.Reverse(tris);

            Mesh m = new Mesh();
            m.vertices = verts;
            m.triangles = tris;
            m.uv = uvs;
            m.RecalculateBounds();
            m.RecalculateNormals();

            GameObject gameObj = new GameObject("_b");
            MeshFilter filter = gameObj.AddComponent<MeshFilter>();
            filter.mesh = m;
            MeshRenderer renderer = gameObj.AddComponent<MeshRenderer>();
            renderer.material = ObjectLedger.Instance.spawnMat;
            gameObj.transform.position = spawnposition ?? Vector3.zero;
            gameObj.transform.rotation = spawnRotation ?? Quaternion.Euler(Vector3.zero);
            if (parent != null) gameObj.transform.parent = parent;
        }

        #endregion

        private static SimulatorHandler instance;
        public static SimulatorHandler Instance {
            get {
                if (instance == null) instance = new SimulatorHandler();
                return instance;
            }
        }
    }
}