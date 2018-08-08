using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class NetworkMesh : MonoBehaviour
{
    private const float AdaptiveLinearAcceleration = 100f;
    private const float AdaptiveLinearDampingFactor = 10f;
    private const float AdaptiveMaxLinearSpeed = 15f;
    private const float AdaptiveMaxLinearDamping = 30f;

    private const float VelocityCorrectionScalar = 100f;
    private const float RotationCorrectionScalar = 10f;

    private Vector3 matchedLinearVelocity;
    private Vector3 adaptiveLinearVelocity;

    /// <summary>
    /// The target linear velocity of the <see cref="NetworkMesh"/>.
    /// </summary>
    public Vector3 TargetLinearVelocity { get; set; }

    /// <summary>
    /// The mesh object associated with this instance.
    /// </summary>
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
        matchedLinearVelocity = Vector3.Lerp(matchedLinearVelocity, TargetLinearVelocity, Time.deltaTime * VelocityCorrectionScalar);

        MeshObject.transform.position += matchedLinearVelocity * Time.deltaTime;

        Vector3 offset = transform.position - MeshObject.transform.position;

        adaptiveLinearVelocity += offset * AdaptiveLinearAcceleration * Time.deltaTime;
        adaptiveLinearVelocity /= 1 + Math.Min((AdaptiveLinearDampingFactor / offset.magnitude), AdaptiveMaxLinearDamping) * Time.deltaTime;

        if (adaptiveLinearVelocity.magnitude > AdaptiveMaxLinearSpeed)
            adaptiveLinearVelocity = adaptiveLinearVelocity.normalized * AdaptiveMaxLinearSpeed;

        MeshObject.transform.position += adaptiveLinearVelocity * Time.deltaTime;

        MeshObject.transform.rotation = Quaternion.Lerp(MeshObject.transform.rotation, transform.rotation, Time.deltaTime * RotationCorrectionScalar);
    }

    /// <summary>
    /// Destroys the associated mesh object when this instance is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        Destroy(MeshObject);
    }
}
