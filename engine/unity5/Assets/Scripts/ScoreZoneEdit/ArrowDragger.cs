using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script drags an arrow in only a single direction, but drags the parent gameobject in that direction as well
public class ArrowDragger : MonoBehaviour
{
    private Vector3 lastMousePos;

    public float OffsetFactor;

    public enum ConstrainAxis
    {
        x,
        y,
        z
    };

    public ConstrainAxis constrainAxis;
	
    void OnMouseDrag()
    {
        float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        Vector3 Position =
            Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));

        switch (constrainAxis)
        {
            case ConstrainAxis.x:
                Position.y = this.transform.position.y;
                Position.z = this.transform.position.z;
                Position.x -= OffsetFactor;
                break;
            case ConstrainAxis.y:
                Position.z = this.transform.position.z;
                Position.x = this.transform.position.x;
                Position.y -= OffsetFactor;
                break;
            case ConstrainAxis.z:
                Position.x = this.transform.position.x;
                Position.y = this.transform.position.y;
                Position.z -= OffsetFactor;
                break;
        }

        transform.parent.position = Position;

        // Vector3 delta = Input.mousePosition - lastMousePos;
        // Vector3 pos = transform.position;
        // pos.x += delta.x * 0.001f;
        // transform.position = pos;
        // lastMousePos = Input.mousePosition;
    }
}