using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{
    public static class GeometryUtilEx
    {
        public static void GetPlaneEquationsFromVertices(AlignedVector3Array vertices, List<Vector4> planeEquationsOut)
        {
            int numVertices = vertices.Count;

            for (int i = 0; i < numVertices; i++)
            {
                Vector3 n1 = vertices[i];

                for (int j = i + 1; j < numVertices; j++)
                {
                    Vector3 n2 = vertices[j];

                    for (int k = j + 1; k < numVertices; k++)
                    {
                        Vector3 n3 = vertices[k];

                        Vector3 planeEquationV3;
                        Vector3 edge0 = n2 - n1;
                        Vector3 edge1 = n3 - n1;

                        float normalSign = 1f;

                        for (int ww = 0; ww < 2; ww++)
                        {
                            planeEquationV3 = normalSign * edge0.Cross(edge1);

                            if (planeEquationV3.LengthSquared > 0.0001f)
                            {
                                planeEquationV3.Normalize();

                                if (NotExist(planeEquationV3, planeEquationsOut))
                                {
                                    Vector4 planeEquation = new Vector4(planeEquationV3, -planeEquationV3.Dot(n1));

                                    AlignedVector3Array array = new AlignedVector3Array();

                                    foreach (Vector4 v in planeEquationsOut)
                                        array.Add(new Vector3(v.X, v.Y, v.Z));

                                    if (GeometryUtil.AreVerticesBehindPlane(planeEquationV3, array, 0.01f)/*AreVerticesBehindPlane(planeEquation, planeEquationsOut, 0.01f)*/)
                                    {
                                        planeEquationsOut.Add(planeEquation);
                                    }
                                }
                            }

                            normalSign = -1f;
                        }
                    }
                }
            }
        }

        private static bool NotExist(Vector3 planeEquation, List<Vector4> planeEquations)
        {
            int numBrushes = planeEquations.Count;

            for (int i = 0; i < numBrushes; i++)
            {
                Vector4 n1 = planeEquations[i];
                Vector3 n1V3 = new Vector3(n1.X, n1.Y, n1.Z);

                if (planeEquation.Dot(n1V3) > 0.999f)
                    return false;
            }
            
            return true;
        }

        private static bool AreVerticesBehindPlane(Vector4 planeNormal, List<Vector4> vertices, float margin)
        {
            int numVertices = vertices.Count;

            for (int i = 0; i < numVertices; i++)
            {
                Vector4 n1 = vertices[i];
                Vector3 n1V3 = new Vector3(n1.X, n1.Y, n1.Z);
                Vector3 planeNormalV3 = new Vector3(planeNormal.X, planeNormal.Y, planeNormal.Z);

                float dist = planeNormalV3.Dot(n1V3) + planeNormal.W - margin;

                if (dist > 0f)
                    return false;
            }

            return true;
        }
    }
}
