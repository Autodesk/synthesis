using UnityEngine;

public class ConnectionPointBillboard : MonoBehaviour {
    void LateUpdate() {
        transform.RotateAround(transform.position, transform.up,
            CalcSignedCentralAngle(transform.forward,
                Vector3.Normalize(Camera.main.transform.position - transform.position), transform.up) *
                Mathf.Rad2Deg);
    }

    private float CalcSignedCentralAngle(
        Vector3 dir1, Vector3 dir2, Vector3 normal) // calculates signed angle projected to a plane
        => Mathf.Atan2(Vector3.Dot(Vector3.Cross(dir1, dir2), normal), Vector3.Dot(dir1, dir2));
}
