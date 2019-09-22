using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.BUExtensions;
using BulletUnity;

public class StickyGamepiece : MonoBehaviour
{
    private BBallSocketConstraintEx constraint;
    private Vector3 point;

    public void SetPoint(Vector3 a)
    {
        constraint = gameObject.AddComponent<BBallSocketConstraintEx>();

        float x = a.x - transform.position.x;
        float y = a.y - transform.position.y;
        float z = a.z - transform.position.z;

        Vector3 g = new Vector3(x, y, z);

        constraint.PivotInA = gameObject.transform.InverseTransformVector(g).ToBullet();
        constraint.constraintType = BTypedConstraint.ConstraintType.constrainToPointInSpace;
        point = gameObject.transform.InverseTransformVector(g);
    }
}
