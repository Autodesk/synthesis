using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using UGraphics = UnityEngine.Graphics;

namespace Synthesis.Graphics {
    public class GridRenderer: MonoBehaviour {

        public Material Mat;
        
        private GraphicsBuffer _buff;
        private NativeArray<float> _vertData;
        
        // private void Start() {
        //     _vertData = new NativeArray<float>(new float[] { -0.5f, 0.5f }, Allocator.Persistent);

        //     _buff = new GraphicsBuffer(GraphicsBuffer.Target.Vertex, 2, 4);
        //     _buff.SetData(_vertData);
        // }

        // private void OnDestroy() {
        //     _buff.Dispose();
        //     _vertData.Dispose();
        //     _buff = null;
        // }

        // private void Update() {
        //     RenderParams rp = new RenderParams(Mat);
        //     rp.worldBounds = new Bounds(Vector3.zero, Vector3.one * 100);
        //     rp.matProps = new MaterialPropertyBlock();
        //     rp.matProps.SetBuffer("_Points", _buff);
        //     UGraphics.RenderPrimitives(rp, MeshTopology.Lines, 2, 1);
        // }
    }
}
