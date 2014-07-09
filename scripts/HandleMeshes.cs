using UnityEngine;
using System.Collections;
using System.IO;

public class HandleMeshes : MonoBehaviour
{
		public static void loadBXDA (string filepath, Transform trans)
		{

				Debug.Log ("Load mesh: " + filepath);
				BXDAMesh mesh = new BXDAMesh ();
				mesh.ReadBXDA (filepath);

				int mCount = mesh.meshes.Count;

				Init.generateCubes (mCount, trans, trans.name + " Subpart");

				for (int j = 0; j < mCount; j++) {
						BXDAMesh.BXDASubMesh sub = mesh.meshes [j];
						GameObject parentCube = trans.gameObject;

						Vector3[] vertices = ArrayUtilities.WrapArray<Vector3> (
                delegate(double x, double y, double z) {
								return new Vector3 ((float)x, (float)y, (float)z);
						}, sub.verts);
						Vector3[] normals = ArrayUtilities.WrapArray<Vector3> (
                delegate(double x, double y, double z) {
								return new Vector3 ((float)x, (float)y, (float)z);
						}, sub.norms);
						Color32[] colors = ArrayUtilities.WrapArray<Color32> (
                delegate(byte r, byte g, byte b, byte a) {
								return new Color32 (r, g, b, a);
						}, sub.colors);
						Vector2[] uvs = ArrayUtilities.WrapArray<Vector2> (
                delegate(double x, double y) {
								return new Vector2 ((float)x, (float)y);
						}, sub.textureCoords);

						Mesh unityMesh = new Mesh ();

						//Debug.Log (vertices [2]);


						//Seems to be mostly useless since they are already added by default
						trans.GetChild (j).gameObject.AddComponent <MeshFilter> ();
						trans.GetChild (j).gameObject.AddComponent <MeshRenderer> ();
						trans.GetChild (j).GetComponent<MeshFilter> ().mesh = unityMesh;
						trans.GetChild (j).GetComponent<MeshRenderer> ().material = new Material (Shader.Find ("Diffuse"));
						unityMesh.vertices = vertices;
						unityMesh.triangles = sub.indicies;
						unityMesh.normals = normals;
						unityMesh.colors32 = colors;
						unityMesh.uv = uvs;
				}
		}

		public static void attachMeshColliders (Transform parent)
		{

				MeshCollider tmp;
				foreach (Transform child in parent) {
						foreach (Transform child2 in child) {
								child2.gameObject.AddComponent<MeshCollider> ();
								tmp = child2.gameObject.GetComponent<MeshCollider> ();
								tmp.convex = true;
						}
				}
		}

		public static void attachRigidBodies (Transform parent)
		{
				foreach (Transform child in parent) {
						child.gameObject.AddComponent<Rigidbody> ();
				}
		}


}
