using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.Network
{
    /// <summary>
    /// This class solves the jittery movement effect of objects synchronized over the network
    /// by smoothing out the movments of their associated meshes.
    /// </summary>
    public class NetworkMesh : MonoBehaviour
    {
        /// <summary>
        /// The rate of acceleration for making apadtive corrections.
        /// A high value means that the mesh will react quickly to larget offsets
        /// with the physical robot's position.
        /// </summary>
        private const float AdaptiveLinearAcceleration = 100f;

        /// <summary>
        /// The damping factor for making adaptive corrections.
        /// A higher value means corrections will be slow and smooth as the difference
        /// between the mesh's position and physical robot's position becomes small.
        /// </summary>
        private const float AdaptiveLinearDampingFactor = 10f;

        /// <summary>
        /// The maximum correction speed for the mesh when making adaptive corrections.
        /// Limiting this value prevents adaptive correction from overshooting the target.
        /// </summary>
        private const float AdaptiveMaxLinearSpeed = 15f;

        /// <summary>
        /// The maximum linear dapming for the mesh when making adaptive corrections.
        /// Limiting this value prevents the mesh from never fully reaching its target.
        /// </summary>
        private const float AdaptiveMaxLinearDamping = 30f;

        /// <summary>
        /// The factor controlling how quickly the velocity of the mesh reacts to
        /// linear velocity changes of the target.
        /// A high value indicates that the mesh will react quickly to changes in the
        /// target velocity.
        /// </summary>
        private const float LinearVelocityCorrectionScalar = 100f;

        /// <summary>
        /// The factor controlling how quickly the rotation of the mesh reacts to
        /// rotational changes of the target.
        /// A high value indicates that the mesh will rotate quickly to correct any
        /// offset with the target's rotation.
        /// </summary>
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
        /// Initializes the NetworkMesh.
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
            matchedLinearVelocity = Vector3.Lerp(matchedLinearVelocity, TargetLinearVelocity, Time.deltaTime * LinearVelocityCorrectionScalar);

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
}
