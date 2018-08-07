using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NetworkMesh : MonoBehaviour
{
    private const float LinearAcceleration = 10000f;
    private const float LinearDamping = 1000f;

    private const float RotationCorrectionScalar = 10f;

    private Vector3 linearVelocity;

    public GameObject MeshObject { get; private set; }

    /// <summary>
    /// Initializes the NetworkMesh
    /// </summary>
    private void Start()
    {
        MeshObject = new GameObject(transform.name + "_network_mesh");
        MeshObject.transform.position = transform.position;
        MeshObject.transform.rotation = transform.rotation;

        Transform[] childTransforms = new Transform[transform.childCount];

        for (int i = 0; i < childTransforms.Length; i++)
            childTransforms[i] = transform.GetChild(i);

        for (int i = 0; i < childTransforms.Length; i++)
            childTransforms[i].parent = MeshObject.transform;
    }

    /// <summary>
    /// Updates the transform of the visible mesh.
    /// </summary>
    private void Update()
    {
        linearVelocity += (transform.position - MeshObject.transform.position) * LinearAcceleration * Time.deltaTime;
        linearVelocity /= 1f + LinearDamping * Time.deltaTime;
        MeshObject.transform.position += linearVelocity * Time.deltaTime;

        MeshObject.transform.rotation = Quaternion.Lerp(MeshObject.transform.rotation, transform.rotation, Time.deltaTime * RotationCorrectionScalar);
    }

    private void OnDestroy()
    {
        Destroy(MeshObject);
    }
}
