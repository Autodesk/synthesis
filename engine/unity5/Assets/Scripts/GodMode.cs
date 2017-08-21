using Assets.Scripts.BUExtensions;
using Assets.Scripts.FSM;
using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// This script is used for controlling objects in a scene by dragging them around with the mouse.
    /// </summary>
    public class GodMode : MonoBehaviour
    {
        BBallSocketConstraintEx constraint;
        ActivationState initialState;
        float initialDamping;
        float rayDistance;

        /// <summary>
        /// Links this MonoBehaviour to the MainState.
        /// </summary>
        private void Awake()
        {
            StateMachine.Instance.Link<MainState>(this);
        }

        /// <summary>
        /// Updates constraint information for the active object, if applicable.
        /// </summary>
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && constraint == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
                BulletSharp.Math.Vector3 end = ray.GetPoint(100).ToBullet();

                ClosestRayResultCallback callback = new ClosestRayResultCallback(ref start, ref end);
                BPhysicsWorld.Get().world.RayTest(start, end, callback);

                rayDistance = (start - callback.HitPointWorld).Length;

                if (callback.CollisionObject != null)
                {
                    BRigidBody rigidBody = callback.CollisionObject.UserObject as BRigidBody;
                    initialState = rigidBody.GetCollisionObject().ActivationState;
                    rigidBody.GetCollisionObject().ActivationState = ActivationState.DisableDeactivation;
                    initialDamping = rigidBody.angularDamping;
                    rigidBody.angularDamping = 0.9f;

                    if (rigidBody != null && rigidBody.mass > 0f)
                    {
                        constraint = rigidBody.gameObject.AddComponent<BBallSocketConstraintEx>();
                        constraint.PivotInA = rigidBody.transform.InverseTransformPoint(callback.HitPointWorld.ToUnity()).ToBullet();
                        constraint.constraintType = BTypedConstraint.ConstraintType.constrainToPointInSpace;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0) && constraint != null)
            {
                constraint.thisRigidBody.GetCollisionObject().ActivationState = initialState;
                constraint.GetComponent<BRigidBody>().angularDamping = initialDamping;

                Destroy(constraint);
                constraint = null;
            }
            else if (constraint != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                constraint.PivotInB = ray.GetPoint(rayDistance).ToBullet();
            }
        }
    }
}
