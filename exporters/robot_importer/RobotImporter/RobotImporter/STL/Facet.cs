using System;


public class Facet
{
    public readonly float[] Normal;
    public readonly float[] Point1;
    public readonly float[] Point2;
    public readonly float[] Point3;

    public Facet(float[] vertices)
    {
        if (vertices.Length != 12)
        {
            Normal = new float[3];
            Point1 = new float[3];
            Point2 = new float[3];
            Point3 = new float[3];
            throw (new Exception("ERROR: Improper Vertex Data"));
        }
        else
        {
            Normal = new float[] { vertices[0], vertices[1], vertices[2] };
            Point1 = new float[] { vertices[3], vertices[4], vertices[5] };
            Point2 = new float[] { vertices[6], vertices[7], vertices[8] };
            Point3 = new float[] { vertices[9], vertices[10], vertices[11] };

        }
    }
}
