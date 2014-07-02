using UnityEngine;
using System.Collections;

public class Init : MonoBehaviour {
	
	void generateCubes(int amt)
	{
		GameObject tmp;
		for (int i = 0; i < amt; i++) 
		{
			tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tmp.transform.parent = this.transform;
			tmp.transform.position = new Vector3(0,0,0);
			tmp.name = "part" + i;
		}
	}

	void Start () {
		string[] filepaths = {"C:\\Users\\t_defap\\Documents\\part2_1.bxda","C:\\Users\\t_defap\\Documents\\movement_1.bxda"};
		generateCubes (filepaths.Length);
		for (int i = 0; i < filepaths.Length; i++) 
		{
			HandleMeshes.loadBXDA (filepaths[i],transform.GetChild(i));
		}
		HandleMeshes.attachMeshColliders (this.transform);
		HandleJoints.loadBXDJ ("C:/Users/t_defap/Documents/skele_andy_mark_chassis/skele_andy_mark_chassis/skeleton.bxdj", transform.GetChild(0), transform.GetChild(1));
		HandleMeshes.attachRigidBodies (this.transform);
	}

	void Update () {
	
	}
}
