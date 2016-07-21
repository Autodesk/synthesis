using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Simulation_RD
{
    class BulletDebugDrawer : IDebugDraw
    {
        private Random r = new Random();
        public DebugDrawModes DebugMode
        {
            get
            {
                // ALL the draw modes
                return ((IEnumerable<DebugDrawModes>)Enum.GetValues(typeof(DebugDrawModes))).Aggregate((a, b) => a | b);
            }
            set
            {
                ;
            }
        }
        
        public void Draw3dText(ref Vector3 location, string textString)
        {
        }

        public void DrawAabb(ref Vector3 min, ref Vector3 max, OpenTK.Graphics.Color4 color)
        {
            //GL.Begin(PrimitiveType.Lines);
            //GL.Color3(0, 0, 0);
            //Vector3 A, B, C, D, E, F;

            //min -= new Vector3(0.1f, 0.1f, 0.1f);
            //max += new Vector3(0.1f, 0.1f, 0.1f);

            //A = new Vector3(max.X, min.Y, max.Z);
            //B = new Vector3(max.X, min.Y, max.Z);
            //C = new Vector3(min.X, min.Y, max.Z);
            //D = new Vector3(min.X, max.Y, min.Z);
            //E = new Vector3(max.X, max.Y, min.Z);
            //F = new Vector3(min.X, max.Y, max.Z);

            ////Top
            //GL.Vertex3(min);
            //GL.Vertex3(A);
            //GL.Vertex3(B);
            //GL.Vertex3(C);
            ////Left
            //GL.Vertex3(min);
            //GL.Vertex3(C);
            //GL.Vertex3(F);
            //GL.Vertex3(D);
            ////Front
            //GL.Vertex3(B);
            //GL.Vertex3(C);
            //GL.Vertex3(F);
            //GL.Vertex3(max);
            ////Right
            //GL.Vertex3(A);
            //GL.Vertex3(B);
            //GL.Vertex3(max);
            //GL.Vertex3(E);
            ////Back
            //GL.Vertex3(min);
            //GL.Vertex3(A);
            //GL.Vertex3(E);
            //GL.Vertex3(D);
            ////Bot
            //GL.Vertex3(max);
            //GL.Vertex3(E);
            //GL.Vertex3(D);
            //GL.Vertex3(F);
            //GL.End();

            ////Console.WriteLine("Drew AABB");
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, OpenTK.Graphics.Color4 color, bool drawSect)
        {
        }
        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, OpenTK.Graphics.Color4 color, bool drawSect, float stepDegrees)
        {
        }

        public void DrawBox(ref Vector3 min, ref Vector3 max, OpenTK.Graphics.Color4 color)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1.0f, 0, 0);
            Vector3 A, B, C, D, E, F;
            A = new Vector3(max.X, min.Y, max.Z);
            B = new Vector3(max.X, min.Y, max.Z);
            C = new Vector3(min.X, min.Y, max.Z);
            D = new Vector3(min.X, max.Y, min.Z);
            E = new Vector3(max.X, max.Y, min.Z);
            F = new Vector3(min.X, max.Y, max.Z);
            //Top
            GL.Vertex3(min);
            GL.Vertex3(A);
            GL.Vertex3(B);
            GL.Vertex3(C);
            //Left
            GL.Vertex3(min);
            GL.Vertex3(C);
            GL.Vertex3(F);
            GL.Vertex3(D);
            //Front
            GL.Vertex3(B);
            GL.Vertex3(C);
            GL.Vertex3(F);
            GL.Vertex3(max);
            //Right
            GL.Vertex3(A);
            GL.Vertex3(B);
            GL.Vertex3(max);
            GL.Vertex3(E);
            //Back
            GL.Vertex3(min);
            GL.Vertex3(A);
            GL.Vertex3(E);
            GL.Vertex3(D);
            //Bot
            GL.Vertex3(max);
            GL.Vertex3(E);
            GL.Vertex3(D);
            GL.Vertex3(F);
            GL.End();
            //Console.WriteLine("Drew Box");
        }

        public void DrawBox(ref Vector3 min, ref Vector3 max, ref Matrix4 trans, OpenTK.Graphics.Color4 color)
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(1.0f, 0, 0);
            Vector3 A, B, C, D, E, F;
            A = Vector3.Transform(new Vector3(max.X, min.Y, min.Z), trans);
            B = Vector3.Transform(new Vector3(max.X, min.Y, max.Z), trans);
            C = Vector3.Transform(new Vector3(min.X, min.Y, max.Z), trans);
            D = Vector3.Transform(new Vector3(min.X, max.Y, min.Z), trans);
            E = Vector3.Transform(new Vector3(max.X, max.Y, min.Z), trans);
            F = Vector3.Transform(new Vector3(min.X, max.Y, max.Z), trans);
            max = Vector3.Transform(max, trans);
            min = Vector3.Transform(min, trans);
            
            //Top
            GL.Vertex3(min);
            GL.Vertex3(A);
            GL.Vertex3(B);
            GL.Vertex3(C);
            //Left
            GL.Vertex3(min);
            GL.Vertex3(C);
            GL.Vertex3(F);
            GL.Vertex3(D);
            //Front
            GL.Vertex3(B);
            GL.Vertex3(C);
            GL.Vertex3(F);
            GL.Vertex3(max);
            //Right
            GL.Vertex3(A);
            GL.Vertex3(B);
            GL.Vertex3(max);
            GL.Vertex3(E);
            //Back
            GL.Vertex3(min);
            GL.Vertex3(A);
            GL.Vertex3(E);
            GL.Vertex3(D);
            //Bot
            GL.Vertex3(max);
            GL.Vertex3(E);
            GL.Vertex3(D);
            GL.Vertex3(F);
            GL.End();

            //Console.WriteLine("Drew Box");
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix4 transform, OpenTK.Graphics.Color4 color)
        {
        }
        public void DrawCone(float radius, float height, int upAxis, ref Matrix4 transform, OpenTK.Graphics.Color4 color)
        {
        }
        public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, OpenTK.Graphics.Color4 color)
        {
        }
        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix4 transform, OpenTK.Graphics.Color4 color)
        {
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, OpenTK.Graphics.Color4 color)
        {
            GL.Color3(1.0f, 1.0f, 0.0f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(from);
            GL.Vertex3(to);
            GL.End();
            //Console.WriteLine("Drew Line");
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, OpenTK.Graphics.Color4 fromColor, OpenTK.Graphics.Color4 toColor)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(0, 0, 255);
            GL.Vertex3(from);
            GL.Color3(toColor.R, toColor.G, toColor.B);
            GL.Vertex3(to);
            GL.End();
            //Console.WriteLine("Drew Line");
        }
        
        public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix4 transform, OpenTK.Graphics.Color4 color)
        {
        }

        public void DrawSphere(ref Vector3 p, float radius, OpenTK.Graphics.Color4 color)
        {
            Random r = new Random();
            Func<Vector3> rv = () => new Vector3((float)r.NextDouble() * radius, (float)r.NextDouble() * radius, (float)r.NextDouble() * radius);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(255, 0, 0);
            for (int i = 0; i < 30; i++)
                GL.Vertex3(rv());
            GL.End();

            //Console.WriteLine("Drew Sphere");
        }

        public void DrawSphere(float radius, ref Matrix4 transform, OpenTK.Graphics.Color4 color)
        {
            Random r = new Random();
            Func<Vector3> rv = () => new Vector3((float)r.NextDouble() * radius, (float)r.NextDouble() * radius, (float)r.NextDouble() * radius);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(255, 0, 0);
            for (int i = 0; i < 30; i++)
                GL.Vertex3(rv() * transform.ExtractScale() + transform.ExtractTranslation());
            GL.End();

            //Console.WriteLine("Drew Sphere");
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, OpenTK.Graphics.Color4 color)
        {
            Random r = new Random();
            Func<Vector3> rv = () => new Vector3((float)r.NextDouble() * radius, (float)r.NextDouble() * radius, (float)r.NextDouble() * radius);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(255, 0, 0);
            for (int i = 0; i < 30; i++)
                GL.Vertex3(rv());
            GL.End();

            Console.WriteLine("Drew Sphere");
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, OpenTK.Graphics.Color4 color, float stepDegrees)
        {
            Random r = new Random();
            Func<Vector3> rv = () => new Vector3((float)r.NextDouble() * radius, (float)r.NextDouble() * radius, (float)r.NextDouble() * radius);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(255, 0, 0);
            for (int i = 0; i < 30; i++)
                GL.Vertex3(rv());
            GL.End();

            //Console.WriteLine("Drew Sphere");
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, OpenTK.Graphics.Color4 color, float stepDegrees, bool drawCenter)
        {
            Random r = new Random();
            Func<Vector3> rv = () => new Vector3((float)r.NextDouble() * radius, (float)r.NextDouble() * radius, (float)r.NextDouble() * radius);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(255, 0, 0);
            for (int i = 0; i < 30; i++)
                GL.Vertex3(rv());
            GL.End();

            Console.WriteLine("Drew Sphere");
        }

        public void DrawTransform(ref Matrix4 transform, float orthoLen)
        {
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, OpenTK.Graphics.Color4 color, float alpha)
        {
            float radius = 5;
            Vector3 center = (v0 + v1 + v2) / 3;
            Random r = new Random();
            Func<Vector3> rv = () => new Vector3((float)r.NextDouble() * radius, (float)r.NextDouble() * radius, (float)r.NextDouble() * radius);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(255, 0, 0);
            for (int i = 0; i < 30; i++)
                GL.Vertex3(rv() + center);
            GL.End();

            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(0, 255, 0);
            GL.Vertex3(v0);
            GL.Vertex3(v1);
            GL.Vertex3(v2);
            GL.End();

            //Console.WriteLine("Drew Triangle");
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed3, ref Vector3 __unnamed4, ref Vector3 __unnamed5, OpenTK.Graphics.Color4 color, float alpha)
        {
            //GL.Begin(PrimitiveType.Triangles);
            //GL.Color3(0, 255, 0);
            //GL.Vertex3(v0);
            //GL.Vertex3(v1);
            //GL.Vertex3(v2);
            //GL.End();

            //Console.WriteLine("Drew Triangle 2");
        }

        public void FlushLines()
        {
        }

        public void ReportErrorWarning(string warningString)
        {
            Console.WriteLine(warningString);
        }
    }
}
