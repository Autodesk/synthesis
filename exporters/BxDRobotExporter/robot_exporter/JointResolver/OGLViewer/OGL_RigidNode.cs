using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OGLViewer
{

    /// <summary>
    /// A rigid node with mesh data that can be rendered in the viewer
    /// </summary>
    public class OGL_RigidNode : RigidNode_Base
    {

        /// <summary>
        /// GUID used to determine highlighted nodes
        /// </summary>
        private readonly UInt32 myGUID;

        /// <summary>
        /// The part's transformation matrix
        /// </summary>
        private Matrix4 myTrans = new Matrix4();

        public BXDAMesh baseMesh;

        /// <summary>
        /// The node's visual submeshes
        /// </summary>
        private List<VBOMesh> models = new List<VBOMesh>();

        /// <summary>
        /// The node's collision submeshes
        /// </summary>
        private List<VBOMesh> colliders = new List<VBOMesh>();

        /// <summary>
        /// The requested rotation of the part around a rotational joint
        /// </summary>
        public float requestedRotation = 0;

        /// <summary>
        /// The requested movement of the part around a linear joint
        /// </summary>
        private float requestedTranslation = 0;

        /// <summary>
        /// The part's center of mass (Shown upon selection)
        /// </summary>
        public BXDVector3 centerOfMass;

        /// <summary>
        /// Reset the part to its initial position
        /// </summary>
        private bool initialPositions = false;

        /// <summary>
        /// The amount the part should be moved from its starting position
        /// </summary>
        private float timeStep = 0;

        /// <summary>
        /// The highlight state of the node
        /// </summary>
        public HighlightState highlight = HighlightState.NOTHING;

        /// <summary>
        /// Get the number of collision submeshes on the part
        /// </summary>
        public int colliderCount
        {
            get
            {
                return colliders.Count;
            }
            private set
            {
            }
        }

        /// <summary>
        /// Get the number of visual submeshes on the part
        /// </summary>
        public int meshCount
        {
            get
            {
                return models.Count;
            }
            private set
            {
            }
        }

        /// <summary>
        /// The total number of triangles in the part's visual submeshes
        /// </summary>
        public int meshTriangleCount
        {
            get;
            private set;
        }

        /// <summary>
        /// The total number of triangles in the part's collision submeshes
        /// </summary>
        public int colliderTriangleCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Create a blank OGL_RigidNode with a unique GUID
        /// </summary>
        public OGL_RigidNode(Guid guid)
            : base(guid)
        {
            myGUID = SelectManager.AllocateGUID(this);
        }

        /// <summary>
        /// Create a new OGL_RigidNode from existing data
        /// </summary>
        /// <remarks>
        /// This is used primarily for converting another subclass of RigidNode_Base to an OGL_RigidNode. 
        /// For conversion from a RigidNode_Base, casting will suffice.
        /// </remarks>
        /// <param name="baseData">The rigid node containing existing model data</param>
        public OGL_RigidNode(RigidNode_Base baseData)
            : base(baseData.GUID)
        {
            myGUID = SelectManager.AllocateGUID(this);
            ModelFullID = baseData.ModelFullID;
            ModelFileName = baseData.ModelFileName;

            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> child in baseData.Children)
            {
                AddChild(child.Key, new OGL_RigidNode(child.Value));
            }
        }

        /// <summary>
        /// Free resources used by OpenGL
        /// </summary>
        public void destroy()
        {
            SelectManager.FreeGUID(myGUID);
            foreach (VBOMesh mesh in models)
            {
                mesh.destroy();
            }
        }

        public void GetWheelInfo(out float radius, out float width, out BXDVector3 center)
        {
            radius = 0;
            width = 0;
            center = new BXDVector3();

            if (GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null &&
                GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL)
            {
                float[] dists = new float[3]; //[0] = x, [1] = y, [2] = z

                //Let's assume that it's aligned on some axis. If it isn't, we have other problems
                foreach (VBOMesh mesh in models)
                {
                    double[] verts = mesh.subMesh.verts;

                    Vector3[] vertices = new Vector3[verts.Length / 3];

                    for (int i = 0; i < verts.Length; i += 3)
                    {
                        vertices[i / 3] = new Vector3((float) verts[i], (float) verts[i + 1], (float) verts[i + 2]);
                    }

                    Vector3 sum = new Vector3();

                    foreach (Vector3 vert in vertices)
                    {
                        sum += vert;
                    }

                    center = new BXDVector3(sum.X / vertices.Length, sum.Y / vertices.Length, sum.Z / vertices.Length);
                }

                foreach (VBOMesh mesh in models)
                {
                    double[] verts = mesh.subMesh.verts;

                    for (int i = 0; i < verts.Length; i += 3)
                    {
                        dists[0] = (float) Math.Max(dists[0], Math.Abs(center.x - verts[i]));
                        dists[1] = (float) Math.Max(dists[1], Math.Abs(center.y - verts[i + 1]));
                        dists[2] = (float) Math.Max(dists[2], Math.Abs(center.z - verts[i + 2]));
                    }
                }

                Array.Sort(dists);

                width = dists[0];
                radius = (dists[1] + dists[2]) / 2f;
            }

            Console.WriteLine(String.Format("Found center to be <{0} {1} {2}>", center.x, center.y, center.z));
            Console.WriteLine(String.Format("Found radius to be {0}", radius));
            Console.WriteLine(String.Format("Found width to be {0}", width));
        }

        /// <summary>
        /// Load the data from a BXDAMesh into the node
        /// </summary>
        /// <param name="mesh">The mesh to load from</param>
        public void loadMeshes(BXDAMesh mesh)
        {
            this.centerOfMass = mesh.physics.centerOfMass;
            baseMesh = mesh;

            meshTriangleCount = 0;
            foreach (BXDAMesh.BXDASubMesh sub in mesh.meshes)
            {
                models.Add(new VBOMesh(sub));
                foreach (BXDAMesh.BXDASurface surf in sub.surfaces)
                {
                    meshTriangleCount += surf.indicies.Length / 3;
                }
            }

            colliderTriangleCount = 0;
            foreach (BXDAMesh.BXDASubMesh sub in mesh.colliders)
            {
                colliders.Add(new VBOMesh(sub));
                foreach (BXDAMesh.BXDASurface surf in sub.surfaces)
                {
                    colliderTriangleCount += surf.indicies.Length / 3;
                }
            }
        }

        /// <summary>
        /// Compute the positions and rotations of this node and its children
        /// </summary>
        /// <param name="moveJoints">Whether or not to move the node on its joints</param>
        public void compute(bool moveJoints)
        {
            if (moveJoints) timeStep += 0.005f;
            else timeStep = 0.0f;

            #region INIT_POSITION
            if (!initialPositions && GetSkeletalJoint() != null)
            {
                initialPositions = true;

                switch (GetSkeletalJoint().GetJointType())
                {
                    case SkeletalJointType.CYLINDRICAL:
                        CylindricalJoint_Base cjb = (CylindricalJoint_Base)GetSkeletalJoint();
                        requestedRotation = cjb.currentAngularPosition;
                        requestedTranslation = cjb.currentLinearPosition;
                        break;
                    case SkeletalJointType.ROTATIONAL:
                        RotationalJoint_Base rjb = (RotationalJoint_Base)GetSkeletalJoint();
                        requestedRotation = rjb.currentAngularPosition;
                        requestedTranslation = 0;
                        break;
                    case SkeletalJointType.LINEAR:
                        LinearJoint_Base ljb = (LinearJoint_Base)GetSkeletalJoint();
                        requestedRotation = 0;
                        requestedTranslation = ljb.currentLinearPosition;
                        break;
                }
            }
            #endregion

            myTrans = Matrix4.Identity;
            if (GetSkeletalJoint() != null)
            {
                foreach (AngularDOF dof in GetSkeletalJoint().GetAngularDOF())
                {
                    BXDVector3 axis = dof.rotationAxis;
                    BXDVector3 basePoint = dof.basePoint;
                    if (GetParent() != null)
                    {
                        basePoint = ((OGL_RigidNode)GetParent()).myTrans.Multiply(basePoint);
                        axis = ((OGL_RigidNode)GetParent()).myTrans.Rotate(axis);
                    }

                    requestedRotation = (float)(Math.Sin(timeStep) + 0.9f) * 1.2f *
                                            (dof.hasAngularLimits() ?
                                                (dof.upperLimit - dof.lowerLimit) / 2.0f : 3.14f) +
                                            (dof.hasAngularLimits() ?
                                                dof.lowerLimit : 0);

                    requestedRotation = Math.Max(dof.lowerLimit, Math.Min(dof.upperLimit, requestedRotation));
                    myTrans *= Matrix4.CreateTranslation(-dof.basePoint.ToTK());
                    myTrans *= Matrix4.CreateFromAxisAngle(dof.rotationAxis.ToTK(), requestedRotation - dof.currentPosition);
                    myTrans *= Matrix4.CreateTranslation(dof.basePoint.ToTK());
                }
                foreach (LinearDOF dof in GetSkeletalJoint().GetLinearDOF())
                {
                    requestedTranslation = (float)(Math.Cos(timeStep) + 0.9f) * 1.2f *
                                                (dof.hasLowerLinearLimit() && dof.hasUpperLinearLimit() ?
                                                    (dof.upperLimit - dof.lowerLimit) / 2.0f : 3.14f) +
                                                (dof.hasLowerLinearLimit() ?
                                                    dof.lowerLimit : 0);

                    requestedTranslation = Math.Max(dof.lowerLimit, Math.Min(dof.upperLimit, requestedTranslation));
                    myTrans *= Matrix4.CreateTranslation(dof.translationalAxis.ToTK() * (requestedTranslation - dof.currentPosition));
                }
            }

            if (GetParent() != null)
            {
                myTrans = myTrans * ((OGL_RigidNode)GetParent()).myTrans;
            }

            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in Children)
            {
                OGL_RigidNode child = ((OGL_RigidNode)pair.Value);
                child.compute(moveJoints);
            }
        }

        #region OUTDATED
        ///// <summary>
        ///// Render the node and tint it based on any highlights it may have
        ///// </summary>
        ///// <param name="select">Whether or not the node is being rendered for highlight selection</param>
        ///// <param name="highlightColor">The color to highlight the node if it is active</param>
        ///// <param name="tintColor">The color to tint the node on hover</param>
        //public void render(bool select = false, uint highlightColor = 0xFFFF0000, uint tintColor = 0xFFBBFFBB)
        //{
        //    int tintLocation = 0;
        //    HighlightState tmpHighlight = highlight;

        //    GL.PushMatrix();
        //    {
        //        GL.MultMatrix(ref myTrans);

        //        if (!select)
        //        {
        //            GL.Enable(EnableCap.Lighting);
        //            GL.UseProgram(ShaderLoader.PartShader);
        //            if (tmpHighlight > 0)
        //            {
        //                tintLocation = GL.GetUniformLocation(ShaderLoader.PartShader, "tintColor");

        //                if ((tmpHighlight & HighlightState.ACTIVE_HOVERING) == HighlightState.ACTIVE_HOVERING)
        //                {
        //                    uint activeHoverColor = highlightColor & tintColor;
        //                    byte a = (byte)(activeHoverColor >> 24);
        //                    byte r = (byte)((((activeHoverColor >> 16) & 0xFF) + 0xFF) / 2);
        //                    byte g = (byte)((((activeHoverColor >> 8) & 0xFF) + 0xFF) / 2);
        //                    byte b = (byte)(((activeHoverColor & 0xFF) + 0xFF) / 2);

        //                    GL.Uniform4(tintLocation, 1, new float[] { (float) r / 255f, 
        //                                                           (float) g / 255f, 
        //                                                           (float) b / 255f,  
        //                                                           (float) a / 255f });
        //                }
        //                else if ((tmpHighlight & HighlightState.ACTIVE) == HighlightState.ACTIVE)
        //                {
        //                    GL.Uniform4(tintLocation, 1, new float[] { (float) (highlightColor >> 16 & 0xFF) / 255f, 
        //                                                           (float) (highlightColor >> 8 & 0xFF) / 255f, 
        //                                                           (float) (highlightColor & 0xFF) / 255f,  
        //                                                           (float) (highlightColor >> 24 & 0xFF) / 255f });
        //                }
        //                else if ((tmpHighlight & HighlightState.HOVERING) == HighlightState.HOVERING)
        //                {
        //                    GL.Uniform4(tintLocation, 1, new float[] { (float) (tintColor >> 16 & 0xFF) / 255f, 
        //                                                           (float) (tintColor >> 8 & 0xFF) / 255f, 
        //                                                           (float) (tintColor & 0xFF) / 255f,  
        //                                                           (float) (tintColor >> 24 & 0xFF) / 255f });
        //                }
        //            }
        //        }
        //        else
        //        {
        //            GL.Disable(EnableCap.Lighting);
        //            GL.UseProgram(0);
        //            GL.Color4(SelectManager.GUIDToColor(myGUID)); //Render it the color assigned to it by the GUID manager
        //        }

        //        foreach (VBOMesh mesh in models)
        //        {
        //            mesh.draw(!select);
        //        }

        //        if (!select)
        //        {
        //            if (tmpHighlight > 0)
        //            {
        //                GL.Uniform4(tintLocation, 1, new float[] { 1, 1, 1, 1 });
        //            }
        //        }

        //        GL.UseProgram(0);
        //    }
        //    GL.PopMatrix();
        //}

        ///// <summary>
        ///// Render the node's center of mass and limits of motion along the joint it is connected to (If any)
        ///// </summary>
        //public void renderDebug(bool drawAxes)
        //{
        //    // Debug Settings
        //    GL.UseProgram(0);
        //    GL.Disable(EnableCap.Lighting);
        //    GL.LineWidth(2f);

        //    GL.PushMatrix();
        //    {
        //        GL.MultMatrix(ref myTrans);

        //        GL.PushAttrib(AttribMask.AllAttribBits);
        //        {
        //            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        //            float lerp = 0;
        //            foreach (VBOMesh mesh in colliders)
        //            {
        //                GL.Color4(0f, lerp, 1f - lerp, 1f);
        //                lerp += (1f / colliders.Count);
        //                mesh.draw();
        //            }
        //        }
        //        GL.PopAttrib();

        //        if (GetSkeletalJoint() != null && drawAxes)
        //        {
        //            float crosshairLength = 100;

        //            bool hasLinearDOF = GetSkeletalJoint().GetLinearDOF().GetEnumerator().MoveNext();

        //            #region ROTATIONAL_SPEC
        //            foreach (AngularDOF dof in GetSkeletalJoint().GetAngularDOF())
        //            {
        //                BXDVector3 dirCOM = centerOfMass.Copy().Subtract(dof.basePoint);
        //                float offset = BXDVector3.DotProduct(dirCOM, dof.rotationAxis);
        //                BXDVector3 baseCOM = dof.basePoint.Copy();

        //                if (BXDVector3.CrossProduct(dirCOM, dof.rotationAxis).Magnitude() < 1E-10)    // COM is on the axis. (>.>)  Pick randomlyish.
        //                {
        //                    dirCOM = new BXDVector3(.123213, 123213, 0.82134); // Certain to be random.
        //                }

        //                dirCOM.Multiply(1f / dirCOM.Magnitude());
        //                baseCOM.Add(dof.rotationAxis.Copy().Multiply(offset / dof.rotationAxis.Magnitude()));

        //                BXDVector3 direction = BXDVector3.CrossProduct(dirCOM, dof.rotationAxis);
        //                direction = BXDVector3.CrossProduct(direction, dof.rotationAxis);
        //                if (BXDVector3.DotProduct(dirCOM, direction) < 0)
        //                {
        //                    direction.Multiply(-1f);
        //                }

        //                GL.PushMatrix();
        //                {
        //                    GL.Translate(baseCOM.x, baseCOM.y, baseCOM.z);

        //                    if (!hasLinearDOF) // Linear limits show the axis anyways, and clipping is UGLY
        //                    {
        //                        // Rotational Axis
        //                        GL.Begin(PrimitiveType.Lines);
        //                        {
        //                            GL.Color3(1f, 0f, 0f);
        //                            GL.Vertex3(-dof.rotationAxis.x * crosshairLength, -dof.rotationAxis.y * crosshairLength, 
        //                                       -dof.rotationAxis.z * crosshairLength);
        //                            GL.Vertex3(dof.rotationAxis.x * crosshairLength, dof.rotationAxis.y * crosshairLength, 
        //                                       dof.rotationAxis.z * crosshairLength);
        //                        }
        //                        GL.End();
        //                    }

        //                    // Current
        //                    GL.Begin(PrimitiveType.Lines);
        //                    {
        //                        GL.Color3(1f, 0f, 1f);
        //                        GL.Vertex3(0, 0, 0);
        //                        GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
        //                    }
        //                    GL.End();

        //                    #region ROTATIONAL_LIMITS
        //                    if (dof.hasAngularLimits())
        //                    {
        //                        // Minpos
        //                        GL.PushMatrix();
        //                        {
        //                            GL.Rotate(180.0f / 3.14f * (dof.lowerLimit - requestedRotation), dof.rotationAxis.ToTK());

        //                            GL.Begin(PrimitiveType.Lines);
        //                            {
        //                                GL.Color3(0f, 1f, 1f);
        //                                GL.Vertex3(0, 0, 0);
        //                                GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
        //                            }
        //                            GL.End();

        //                            OGLDrawing.drawArc(dof.rotationAxis, direction, dof.lowerLimit, dof.upperLimit, crosshairLength,
        //                                               Color4.Cyan, Color4.Green);
        //                        }
        //                        GL.PopMatrix(); // Begin limit matrix

        //                        // Maxpos
        //                        GL.PushMatrix();
        //                        {
        //                            GL.Rotate(180.0f / 3.14f * (dof.upperLimit - requestedRotation), dof.rotationAxis.ToTK());

        //                            GL.Begin(PrimitiveType.Lines);
        //                            {
        //                                GL.Color3(0f, 1f, 0f);
        //                                GL.Vertex3(0, 0, 0);
        //                                GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
        //                            }
        //                            GL.End();
        //                        }
        //                        GL.PopMatrix(); // End limit matrix
        //                    }
        //                    else
        //                    {
        //                        // Full arc!
        //                        OGLDrawing.drawArc(dof.rotationAxis, direction, 0, 6.28f, crosshairLength, Color4.Cyan, Color4.Green);
        //                    }
        //                    #endregion
        //                }
        //                GL.PopMatrix();  // part -> COM-basepoint
        //            }
        //            #endregion

        //            #region LINEAR_SPEC
        //            foreach (LinearDOF dof in GetSkeletalJoint().GetLinearDOF())
        //            {
        //                float lower = (dof.hasLowerLinearLimit() ? dof.lowerLimit : -crosshairLength) - requestedTranslation;
        //                float upper = (dof.hasUpperLinearLimit() ? dof.upperLimit : crosshairLength) - requestedTranslation;

        //                BXDVector3 dirCOM = centerOfMass.Copy().Subtract(dof.basePoint);
        //                float offset = BXDVector3.DotProduct(dirCOM, dof.translationalAxis);
        //                BXDVector3 baseCOM = dof.basePoint.Copy();

        //                if (BXDVector3.CrossProduct(dirCOM, dof.translationalAxis).Magnitude() < 1E-10)    // COM is on the axis. (>.>)  Pick randomlyish.
        //                {
        //                    dirCOM = new BXDVector3(.123213, 123213, 0.82134); // Certain to be random.
        //                }

        //                dirCOM.Multiply(1f / dirCOM.Magnitude());
        //                baseCOM.Add(dof.translationalAxis.Copy().Multiply(offset / dof.translationalAxis.Magnitude()));

        //                BXDVector3 direction = BXDVector3.CrossProduct(dirCOM, dof.translationalAxis);
        //                direction = BXDVector3.CrossProduct(direction, dof.translationalAxis);
        //                if (BXDVector3.DotProduct(dirCOM, direction) < 0)
        //                {
        //                    direction.Multiply(-1f);
        //                }

        //                GL.PushMatrix();
        //                {
        //                    GL.Translate(baseCOM.x, baseCOM.y, baseCOM.z);

        //                    #region LIMITS
        //                    BXDVector3 otherDirection = BXDVector3.CrossProduct(dof.translationalAxis, direction);

        //                    GL.Begin(PrimitiveType.Lines);
        //                    {
        //                        GL.Color3(0f, 1f, 0f);
        //                        GL.Vertex3(dof.translationalAxis.ToTK() * lower);
        //                        GL.Vertex3(dof.translationalAxis.ToTK() * upper);
        //                    }
        //                    GL.End();

        //                    if (dof.hasLowerLinearLimit())
        //                    {
        //                        GL.PushMatrix();
        //                        {
        //                            GL.Translate(dof.translationalAxis.ToTK() * lower);
        //                            OGLDrawing.drawCrossHair(dof.translationalAxis, direction, crosshairLength);
        //                        }
        //                        GL.PopMatrix();
        //                    }

        //                    if (dof.hasUpperLinearLimit())
        //                    {
        //                        GL.PushMatrix();
        //                        {
        //                            GL.Translate(dof.translationalAxis.ToTK() * upper);
        //                            OGLDrawing.drawCrossHair(dof.translationalAxis, direction, crosshairLength);
        //                        }
        //                        GL.PopMatrix();
        //                    }
        //                    #endregion
        //                }
        //                GL.PopMatrix();  // part -> COM-basepoint
        //            }
        //            #endregion
        //        }
        //    }
        //    GL.PopMatrix();  // World -> part matrix

        //    // Revert Debug Settings
        //    GL.Enable(EnableCap.Lighting);
        //} 
        #endregion

        /// <summary>
        /// Get the visual representation of the model
        /// </summary>
        /// <returns>The model's VBO meshes</returns>
        public override object GetModel()
        {
            return models;
        }

        /// <summary>
        /// The enum representing the highlight state of the node
        /// </summary>
        public enum HighlightState : byte
        {
            NOTHING = 0,
            ACTIVE = 1,
            HOVERING = 2,
            ACTIVE_HOVERING = 3
        }

    }
}
