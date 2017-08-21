using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script drags an arrow in only a single direction, but drags the parent gameobject in that direction as well
public class ArrowDragger : MonoBehaviour
{
    private Vector3 m_lastMousePos;

    public float OffsetFactor;

    public enum ConstrainAxis
    {
        x,
        y,
        z
    };

    public ConstrainAxis constrainAxis;

    void Update()
    {
        // This code works fine but the positions get kinda screwed up, so fixes will have to be added for that (TODO)
       //  switch (constrainAxis)
       //  {
       //      case ConstrainAxis.x:
       //          // x -> y
       //          // y -> x
       //          // z -> z 
       //          this.transform.localScale = new Vector3(
       //              (1 / this.transform.parent.localScale.y) / 4,
       //              (1 / this.transform.parent.localScale.x) / 4,
       //              (1 / this.transform.parent.localScale.z) / 4);
       //          break;
       //      case ConstrainAxis.y:
       //          // x -> x
       //          // y -> y
       //          // z -> z
       //          this.transform.localScale = new Vector3(
       //              (1 / this.transform.parent.localScale.x) / 4,
       //              (1 / this.transform.parent.localScale.y) / 4,
       //              (1 / this.transform.parent.localScale.z) / 4);
       //          
       //          break;
       //      case ConstrainAxis.z:
       //          // x -> x
       //          // y -> z
       //          // z -> y
       //          this.transform.localScale = new Vector3(
       //              (1 / this.transform.parent.localScale.x) / 4,
       //              (1 / this.transform.parent.localScale.z) / 4,
       //              (1 / this.transform.parent.localScale.y) / 4);
       //          break;
       //  }
       //  
    }
	
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
                Position.x -= this.transform.parent.localScale.x * OffsetFactor;
                break;
            case ConstrainAxis.y:
                Position.z = this.transform.position.z;
                Position.x = this.transform.position.x;
                Position.y -= this.transform.parent.localScale.y * OffsetFactor;
                break;
            case ConstrainAxis.z:
                Position.x = this.transform.position.x;
                Position.y = this.transform.position.y;
                Position.z -= this.transform.parent.localScale.z * OffsetFactor;
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