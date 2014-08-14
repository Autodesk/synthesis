using System;
public class TransMatrix : Matrix
{

    public TransMatrix()
        : base(4, 4)
    {
        identity();
    }

    public TransMatrix(Matrix m)
        : base(4, 4)
    {
        copy(m);
    }

    public TransMatrix copy(Matrix m)
    {
        for (int i = 0; i < m.getRows(); i++)
            for (int q = 0; q < m.getCols(); q++)
                set(i, q, m.get(i, q));
        return this;
    }

    public TransMatrix setRotation(float x, float y, float z, float radians)
    {
        float c = (float) Math.Cos(radians), s = (float) Math.Sin(radians);
        set(0, 0, c + (x * x * (1 - c)));
        set(0, 1, x * y * (1 - c) - z * s);
        set(0, 2, x * z * (1 - c) + y * s);

        set(1, 0, y * x * (1 - c) + z * s);
        set(1, 1, c + (y * y * (1 - c)));
        set(1, 2, y * z * (1 - c) - x * s);

        set(2, 0, z * x * (1 - c) - y * s);
        set(2, 1, z * y * (1 - c) + x * s);
        set(2, 2, c + (z * z * (1 - c)));
        return this;
    }

    public TransMatrix identity()
    {
        set(0, 0, 1);
        set(1, 0, 0);
        set(2, 0, 0);
        set(3, 0, 0);

        set(0, 1, 0);
        set(1, 1, 1);
        set(2, 1, 0);
        set(3, 1, 0);

        set(0, 2, 0);
        set(1, 2, 0);
        set(2, 2, 1);
        set(3, 2, 0);

        set(0, 3, 0);
        set(1, 3, 0);
        set(2, 3, 0);
        set(3, 3, 1);
        return this;
    }

    public TransMatrix setXRotation(float radians)
    {
        return setRotation(1, 0, 0, radians);
    }

    public TransMatrix setYRotation(float yaw)
    {
        return setRotation(0, 1, 0, yaw);
    }

    public TransMatrix setZRotation(float radians)
    {
        return setRotation(0, 0, 1, radians);
    }

    public BXDVector3 multiply(BXDVector3 v)
    {
        float resX = get(0, 3) + (v.x * get(0, 0)) + (v.y * get(0, 1))
                + (v.z * get(0, 2));
        float resY = get(1, 3) + (v.x * get(1, 0)) + (v.y * get(1, 1))
                + (v.z * get(1, 2));
        float resZ = get(2, 3) + (v.x * get(2, 0)) + (v.y * get(2, 1))
                + (v.z * get(2, 2));
        return new BXDVector3((float) resX, (float) resY, (float) resZ);
    }


    public TransMatrix setReverseSystemTranslation(BXDVector3 origin,
            BXDVector3 xVec, BXDVector3 yVec, BXDVector3 zVec)
    {
        set(0, 0, xVec.x);
        set(0, 1, xVec.y);
        set(0, 2, xVec.z);

        set(1, 0, yVec.x);
        set(1, 1, yVec.y);
        set(1, 2, yVec.z);

        set(2, 0, zVec.x);
        set(2, 1, zVec.y);
        set(2, 2, zVec.z);

        setTranslation(origin.x, origin.y, origin.z);
        return this;
    }

    public TransMatrix setTranslation(float x, float y, float z)
    {
        set(0, 3, x);
        set(1, 3, y);
        set(2, 3, z);
        set(3, 3, 1);
        return this;
    }

    public TransMatrix multiply(TransMatrix b)
    {
        Matrix m = Matrix.multiply(this, b);
        for (int i = 0; i < m.getCols(); i++)
        {
            Array.Copy(m.data[i], 0, data[i], 0, 4);
        }
        return this;
    }

    public BXDVector3 rotate(BXDVector3 norm)
    {
        BXDVector3 ret = new BXDVector3(0, 0, 0);
        ret.x = get(0) * norm.x + get(1) * norm.y + get(2) * norm.z;
        ret.y = get(4) * norm.x + get(5) * norm.y + get(6) * norm.z;
        ret.z = get(8) * norm.x + get(9) * norm.y + get(10) * norm.z;
        return ret;
    }

    public float determinant()
    {
        return data[0][0]
                * (data[1][1]
                        * (data[2][2] * data[3][3] - data[3][2] * data[2][3])
                        - data[1][2]
                        * (data[2][1] * data[3][3] - data[3][1] * data[2][3]) + data[1][3]
                        * (data[2][1] * data[3][2] - data[3][1] * data[2][2]))
                - data[0][1]
                * (data[1][0]
                        * (data[2][2] * data[3][3] - data[3][2] * data[2][3])
                        - data[1][2]
                        * (data[2][0] * data[3][3] - data[3][0] * data[2][3]) + data[1][3]
                        * (data[2][0] * data[3][2] - data[3][0] * data[2][2]))
                + data[0][2]
                * (data[1][0]
                        * (data[2][1] * data[3][3] - data[3][1] * data[2][3])
                        - data[1][1]
                        * (data[2][0] * data[3][3] - data[3][0] * data[2][3]) + data[1][3]
                        * (data[2][0] * data[3][1] - data[3][0] * data[2][1]))
                - data[0][3]
                * (data[1][0]
                        * (data[2][1] * data[3][2] - data[3][1] * data[2][2])
                        - data[1][1]
                        * (data[2][0] * data[3][2] - data[3][0] * data[2][2]) + data[1][2]
                        * (data[2][0] * data[3][1] - data[3][0] * data[2][1]));
    }

    public TransMatrix adjugate()
    {
        TransMatrix adj = new TransMatrix();
        adj.data[0][0] = (data[1][1] * data[2][2] * data[3][3])
                + (data[1][2] * data[2][3] * data[3][1])
                + (data[1][3] * data[2][1] * data[3][2])
                - (data[1][1] * data[2][3] * data[3][2])
                - (data[1][2] * data[2][1] * data[3][3])
                - (data[1][3] * data[2][2] * data[3][1]);
        adj.data[0][1] = (data[0][1] * data[2][3] * data[3][2])
                + (data[0][2] * data[2][1] * data[3][3])
                + (data[0][3] * data[2][2] * data[3][1])
                - (data[0][1] * data[2][2] * data[3][3])
                - (data[0][2] * data[2][3] * data[3][1])
                - (data[0][3] * data[2][1] * data[3][2]);
        adj.data[0][2] = (data[0][1] * data[1][2] * data[3][3])
                + (data[0][2] * data[1][3] * data[3][1])
                + (data[0][3] * data[1][1] * data[3][2])
                - (data[0][1] * data[1][3] * data[3][2])
                - (data[0][2] * data[1][1] * data[3][3])
                - (data[0][3] * data[1][2] * data[3][1]);
        adj.data[0][3] = (data[0][1] * data[1][3] * data[2][2])
                + (data[0][2] * data[1][1] * data[2][3])
                + (data[0][3] * data[1][2] * data[2][1])
                - (data[0][1] * data[1][2] * data[2][3])
                - (data[0][2] * data[1][3] * data[2][1])
                - (data[0][3] * data[1][1] * data[2][2]);

        adj.data[1][0] = (data[1][0] * data[2][3] * data[3][2])
                + (data[1][2] * data[2][0] * data[3][3])
                + (data[1][3] * data[2][2] * data[3][0])
                - (data[1][0] * data[2][2] * data[3][3])
                - (data[1][2] * data[2][3] * data[3][0])
                - (data[1][3] * data[2][0] * data[3][2]);
        adj.data[1][1] = (data[0][0] * data[2][2] * data[3][3])
                + (data[0][2] * data[2][3] * data[3][0])
                + (data[0][3] * data[2][0] * data[3][2])
                - (data[0][0] * data[2][3] * data[3][2])
                - (data[0][2] * data[2][0] * data[3][3])
                - (data[0][3] * data[2][2] * data[3][0]);
        adj.data[1][2] = (data[0][0] * data[1][3] * data[3][2])
                + (data[0][2] * data[1][0] * data[3][3])
                + (data[0][3] * data[1][2] * data[3][0])
                - (data[0][0] * data[1][2] * data[3][3])
                - (data[0][2] * data[1][0] * data[3][0])
                - (data[0][3] * data[1][0] * data[3][2]);
        adj.data[1][3] = (data[0][0] * data[1][2] * data[2][3])
                + (data[0][2] * data[1][3] * data[2][0])
                + (data[0][3] * data[1][0] * data[2][2])
                - (data[0][0] * data[1][3] * data[2][2])
                - (data[0][2] * data[1][0] * data[2][3])
                - (data[0][3] * data[1][2] * data[2][0]);

        adj.data[2][0] = (data[1][0] * data[2][1] * data[3][3])
                + (data[1][1] * data[2][3] * data[3][0])
                + (data[1][3] * data[2][0] * data[3][1])
                - (data[1][0] * data[2][3] * data[3][1])
                - (data[1][1] * data[2][0] * data[3][3])
                - (data[1][3] * data[2][1] * data[3][0]);
        adj.data[2][1] = (data[0][0] * data[2][3] * data[3][1])
                + (data[0][1] * data[2][0] * data[3][3])
                + (data[0][3] * data[2][1] * data[3][0])
                - (data[0][0] * data[2][1] * data[3][3])
                - (data[0][1] * data[2][3] * data[3][0])
                - (data[0][3] * data[2][0] * data[3][1]);
        adj.data[2][2] = (data[0][0] * data[1][1] * data[3][3])
                + (data[0][1] * data[1][3] * data[3][0])
                + (data[0][3] * data[1][0] * data[3][1])
                - (data[0][0] * data[1][3] * data[3][1])
                - (data[0][1] * data[1][0] * data[3][3])
                - (data[0][3] * data[1][1] * data[3][0]);
        adj.data[2][3] = (data[0][0] * data[1][3] * data[2][1])
                + (data[0][1] * data[1][0] * data[2][3])
                + (data[0][3] * data[1][1] * data[2][0])
                - (data[0][0] * data[1][1] * data[2][3])
                - (data[0][1] * data[1][3] * data[2][0])
                - (data[0][3] * data[1][0] * data[2][1]);

        adj.data[3][0] = (data[1][0] * data[2][2] * data[3][1])
                + (data[1][1] * data[2][0] * data[3][2])
                + (data[1][2] * data[2][1] * data[3][0])
                - (data[1][0] * data[2][1] * data[3][2])
                - (data[1][1] * data[2][2] * data[3][0])
                - (data[1][2] * data[2][0] * data[3][1]);
        adj.data[3][1] = (data[0][0] * data[2][1] * data[3][1])
                + (data[0][1] * data[2][2] * data[3][0])
                + (data[0][2] * data[2][0] * data[3][1])
                - (data[0][0] * data[2][2] * data[3][1])
                - (data[0][1] * data[2][0] * data[3][2])
                - (data[0][2] * data[2][1] * data[3][0]);
        adj.data[3][2] = (data[0][0] * data[1][2] * data[3][1])
                + (data[0][1] * data[1][0] * data[3][2])
                + (data[0][2] * data[1][1] * data[3][0])
                - (data[0][0] * data[1][1] * data[3][2])
                - (data[0][1] * data[1][2] * data[3][0])
                - (data[0][2] * data[1][0] * data[3][1]);
        adj.data[3][3] = (data[0][0] * data[1][1] * data[2][2])
                + (data[0][1] * data[1][2] * data[2][0])
                + (data[0][2] * data[1][0] * data[2][1])
                - (data[0][0] * data[1][2] * data[2][1])
                - (data[0][1] * data[1][0] * data[2][2])
                - (data[0][2] * data[1][1] * data[2][0]);
        return adj;
    }

    public TransMatrix inverse()
    {
        TransMatrix m = adjugate();
        m.multiply(1f / determinant());
        return m;
    }

    public float[] toBuffer()
    {
        float[] res = new float[16];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                res[(i * 4) + j] = data[j][i];
            }
        }
        return res;
    }

    public TransMatrix setScale(float x, float y, float z)
    {
        set(0, 0, x);
        set(1, 0, 0);
        set(2, 0, 0);

        set(0, 1, 0);
        set(1, 1, y);
        set(2, 1, 0);

        set(0, 2, 0);
        set(1, 2, 0);
        set(2, 2, z);

        set(0, 3, 0);
        set(1, 3, 0);
        set(2, 3, 0);
        set(3, 3, 1);
        return this;
    }
}