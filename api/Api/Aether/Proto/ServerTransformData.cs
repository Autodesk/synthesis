using UnityEngine;

public partial class ServerTransformData {
    public static explicit operator ServerTransformData(Matrix4x4 m)
        => new ServerTransformData() {
            MatrixData = {
                m[0, 0], m[0, 1], m[0, 2], m[0, 3],
                m[1, 0], m[1, 1], m[1, 2], m[1, 3],
                m[2, 0], m[2, 1], m[2, 2], m[2, 3],
                m[3, 0], m[3, 1], m[3, 2], m[3, 3]
            }
        };

    public static explicit operator Matrix4x4(ServerTransformData transform)
        => new Matrix4x4(
            new Vector4(transform.MatrixData[0], transform.MatrixData[4], transform.MatrixData[8], transform.MatrixData[12]),
            new Vector4(transform.MatrixData[1], transform.MatrixData[5], transform.MatrixData[9], transform.MatrixData[13]),
            new Vector4(transform.MatrixData[2], transform.MatrixData[6], transform.MatrixData[10], transform.MatrixData[14]),
            new Vector4(transform.MatrixData[3], transform.MatrixData[7], transform.MatrixData[11], transform.MatrixData[15])
        );
}
