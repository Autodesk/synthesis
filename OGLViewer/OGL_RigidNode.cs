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
    public class OGL_RigidNode : RigidNode_Base
    {
        public enum HighlightState : byte
        {
            NOTHING = 0,
            ACTIVE = 1,
            HOVERING = 2,
            ACTIVE_HOVERING = 3
        }

        private readonly UInt32 myGUID;

        private Matrix4 myTrans = new Matrix4();
        private List<VBOMesh> models = new List<VBOMesh>();
        private List<VBOMesh> colliders = new List<VBOMesh>();

        public float requestedRotation = 0;
        private float requestedTranslation = 0;
        public BXDVector3 centerOfMass;

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
        public int meshTriangleCount
        {
            get;
            private set;
        }
        public int colliderTriangleCount
        {
            get;
            private set;
        }

        public OGL_RigidNode()
        {
            myGUID = SelectManager.AllocateGUID(this);
        }

        public OGL_RigidNode(RigidNode_Base baseData)
        {
            myGUID = SelectManager.AllocateGUID(this);
            modelFullID = baseData.modelFullID;
            modelFileName = baseData.modelFileName;

            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> child in baseData.children)
            {
                AddChild(child.Key, new OGL_RigidNode(child.Value));
            }
        }

        public void destroy()
        {
            SelectManager.FreeGUID(myGUID);
            foreach (VBOMesh mesh in models)
            {
                mesh.destroy();
            }
        }

        public void loadMeshes(BXDAMesh mesh)
        {
            this.centerOfMass = mesh.physics.centerOfMass;
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

        private bool initialPositions = false;
        float i = 0;
        public void compute(bool moveJoints)
        {
            if (moveJoints) i += 0.005f;
            else i = 0.0f;

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
                    if (animate)
                    {
                        requestedRotation = (float)(Math.Sin(i) + 0.9f) * 1.2f *
                            (dof.hasAngularLimits() ? (dof.upperLimit - dof.lowerLimit) / 2.0f : 3.14f) +
                            (dof.hasAngularLimits() ? dof.lowerLimit : 0);
                    }
                    requestedRotation = Math.Max(dof.lowerLimit, Math.Min(dof.upperLimit, requestedRotation));
                    myTrans *= Matrix4.CreateTranslation(-dof.basePoint.ToTK());
                    myTrans *= Matrix4.CreateFromAxisAngle(dof.rotationAxis.ToTK(), requestedRotation - dof.currentPosition);
                    myTrans *= Matrix4.CreateTranslation(dof.basePoint.ToTK());
                }
                foreach (LinearDOF dof in GetSkeletalJoint().GetLinearDOF())
                {
                    if (animate)
                    {
                        requestedTranslation = (float)(Math.Cos(i) + 0.9f) * 1.2f *
                            (dof.hasLowerLinearLimit() && dof.hasUpperLinearLimit() ? (dof.upperLimit - dof.lowerLimit) / 2.0f : 3.14f) +
                            (dof.hasLowerLinearLimit() ? dof.lowerLimit : 0);
                    }
                    requestedTranslation = Math.Max(dof.lowerLimit, Math.Min(dof.upperLimit, requestedTranslation));
                    myTrans *= Matrix4.CreateTranslation(dof.translationalAxis.ToTK() * (requestedTranslation - dof.currentPosition));
                }
            }
            if (GetParent() != null)
            {
                myTrans = myTrans * ((OGL_RigidNode)GetParent()).myTrans;
            }
            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> pair in children)
            {
                OGL_RigidNode child = ((OGL_RigidNode) pair.Value);
                child.compute(moveJoints);
            }
        }

        public bool animate = true;
        public HighlightState highlight = HighlightState.NOTHING;

        public void render(bool select = false, uint highlightColor = 0xFFFF0000, uint tintColor = 0xFFBBFFBB)
        {
            int tintLocation = 0;
            HighlightState tmpHighlight = highlight;

            GL.PushMatrix();
            GL.MultMatrix(ref myTrans);

            if (!select)
            {
                GL.Enable(EnableCap.Lighting);
                GL.UseProgram(ShaderLoader.PartShader);
                if (tmpHighlight > 0)
                {
                    tintLocation = GL.GetUniformLocation(ShaderLoader.PartShader, "tintColor");
                    if ((tmpHighlight & HighlightState.ACTIVE_HOVERING) == HighlightState.ACTIVE_HOVERING)
                    {
                        uint activeHoverColor = highlightColor & tintColor;

                        GL.Uniform4(tintLocation, 1, new float[] { (float) (activeHoverColor >> 16 & 0xFF) / 255f, 
                                                                   (float) (activeHoverColor >> 8 & 0xFF) / 255f, 
                                                                   (float) (activeHoverColor & 0xFF) / 255f,  
                                                                   (float) (activeHoverColor >> 24 & 0xFF) / 255f });
                    }
                    else if ((tmpHighlight & HighlightState.ACTIVE) == HighlightState.ACTIVE)
                    {
                        GL.Uniform4(tintLocation, 1, new float[] { (float) (highlightColor >> 16 & 0xFF) / 255f, 
                                                                   (float) (highlightColor >> 8 & 0xFF) / 255f, 
                                                                   (float) (highlightColor & 0xFF) / 255f,  
                                                                   (float) (highlightColor >> 24 & 0xFF) / 255f });
                    }
                    else if ((tmpHighlight & HighlightState.HOVERING) == HighlightState.HOVERING)
                    {
                        GL.Uniform4(tintLocation, 1, new float[] { (float) (tintColor >> 16 & 0xFF) / 255f, 
                                                                   (float) (tintColor >> 8 & 0xFF) / 255f, 
                                                                   (float) (tintColor & 0xFF) / 255f,  
                                                                   (float) (tintColor >> 24 & 0xFF) / 255f });
                    }
                }
            }
            else
            {
                GL.Disable(EnableCap.Lighting);
                GL.UseProgram(0);
                GL.Color4(SelectManager.GUIDToColor(myGUID));
            }
            foreach (VBOMesh mesh in models)
            {
                mesh.draw(!select);
            }
            if (!select)
            {
                if (tmpHighlight > 0)
                {
                    GL.Uniform4(tintLocation, 1, new float[] { 1, 1, 1, 1 });
                }
            }
            GL.UseProgram(0);
            GL.PopMatrix();
        }

        public void renderDebug()
        {
            // Debug Settings
            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);
            GL.LineWidth(2f);

            GL.PushMatrix();
            GL.MultMatrix(ref myTrans);

            GL.PushAttrib(AttribMask.AllAttribBits);
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                float lerp = 0;
                foreach (VBOMesh mesh in colliders)
                {
                    GL.Color4(0f, lerp, 1f - lerp, 1f);
                    lerp += (1f / colliders.Count);
                    mesh.draw();
                }
            }
            GL.PopAttrib();

            if (GetSkeletalJoint() != null)
            {
                float crosshairLength = 100;

                bool hasLinearDOF = GetSkeletalJoint().GetLinearDOF().GetEnumerator().MoveNext();


                #region ROTATIONAL_SPEC
                foreach (AngularDOF dof in GetSkeletalJoint().GetAngularDOF())
                {
                    BXDVector3 dirCOM = centerOfMass.Copy().Subtract(dof.basePoint);
                    float offset = BXDVector3.DotProduct(dirCOM, dof.rotationAxis);
                    BXDVector3 baseCOM = dof.basePoint.Copy();

                    if (BXDVector3.CrossProduct(dirCOM, dof.rotationAxis).Magnitude() < 1E-10)    // COM is on the axis. (>.>)  Pick randomlyish.
                    {
                        dirCOM = new BXDVector3(.123213, 123213, 0.82134); // Certain to be random.
                    }
                    dirCOM.Multiply(1f / dirCOM.Magnitude());
                    baseCOM.Add(dof.rotationAxis.Copy().Multiply(offset / dof.rotationAxis.Magnitude()));

                    BXDVector3 direction = BXDVector3.CrossProduct(dirCOM, dof.rotationAxis);
                    direction = BXDVector3.CrossProduct(direction, dof.rotationAxis);
                    if (BXDVector3.DotProduct(dirCOM, direction) < 0)
                    {
                        direction.Multiply(-1f);
                    }


                    GL.PushMatrix();
                    GL.Translate(baseCOM.x, baseCOM.y, baseCOM.z);

                    if (!hasLinearDOF) // Linear limits show the axis anyways, and clipping is UGLY
                    {
                        // Rotational Axis
                        GL.Begin(PrimitiveType.Lines);
                        GL.Color3(1f, 0f, 0f);
                        GL.Vertex3(-dof.rotationAxis.x * crosshairLength, -dof.rotationAxis.y * crosshairLength, -dof.rotationAxis.z * crosshairLength);
                        GL.Vertex3(dof.rotationAxis.x * crosshairLength, dof.rotationAxis.y * crosshairLength, dof.rotationAxis.z * crosshairLength);
                        GL.End();
                    }

                    // Current
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(1f, 0f, 1f);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
                    GL.End();
                    #region ROTATIONAL_LIMITS
                    {
                        if (dof.hasAngularLimits())
                        {
                            // Minpos
                            GL.PushMatrix();
                            GL.Rotate(180.0f / 3.14f * (dof.lowerLimit - requestedRotation), dof.rotationAxis.ToTK());

                            GL.Begin(PrimitiveType.Lines);
                            GL.Color3(0f, 1f, 1f);
                            GL.Vertex3(0, 0, 0);
                            GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
                            GL.End();
                            OGLDrawing.drawArc(dof.rotationAxis, direction, dof.lowerLimit, dof.upperLimit, crosshairLength, Color4.Cyan, Color4.Green);
                            GL.PopMatrix(); // Begin limit matrix

                            // Maxpos
                            GL.PushMatrix();
                            GL.Rotate(180.0f / 3.14f * (dof.upperLimit - requestedRotation), dof.rotationAxis.ToTK());

                            GL.Begin(PrimitiveType.Lines);
                            GL.Color3(0f, 1f, 0f);
                            GL.Vertex3(0, 0, 0);
                            GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
                            GL.End();

                            GL.PopMatrix(); // End limit matrix
                        }
                        else
                        {
                            // Full arc!
                            OGLDrawing.drawArc(dof.rotationAxis, direction, 0, 6.28f, crosshairLength, Color4.Cyan, Color4.Green);
                        }
                    }
                    #endregion
                    GL.PopMatrix();  // part -> COM-basepoint
                }
                #endregion

                #region LINEAR_SPEC
                foreach (LinearDOF dof in GetSkeletalJoint().GetLinearDOF())
                {
                    float lower = (dof.hasLowerLinearLimit() ? dof.lowerLimit : -crosshairLength) - requestedTranslation;
                    float upper = (dof.hasUpperLinearLimit() ? dof.upperLimit : crosshairLength) - requestedTranslation;

                    BXDVector3 dirCOM = centerOfMass.Copy().Subtract(dof.basePoint);
                    float offset = BXDVector3.DotProduct(dirCOM, dof.translationalAxis);
                    BXDVector3 baseCOM = dof.basePoint.Copy();

                    if (BXDVector3.CrossProduct(dirCOM, dof.translationalAxis).Magnitude() < 1E-10)    // COM is on the axis. (>.>)  Pick randomlyish.
                    {
                        dirCOM = new BXDVector3(.123213, 123213, 0.82134); // Certain to be random.
                    }
                    dirCOM.Multiply(1f / dirCOM.Magnitude());
                    baseCOM.Add(dof.translationalAxis.Copy().Multiply(offset / dof.translationalAxis.Magnitude()));

                    BXDVector3 direction = BXDVector3.CrossProduct(dirCOM, dof.translationalAxis);
                    direction = BXDVector3.CrossProduct(direction, dof.translationalAxis);
                    if (BXDVector3.DotProduct(dirCOM, direction) < 0)
                    {
                        direction.Multiply(-1f);
                    }


                    GL.PushMatrix();
                    GL.Translate(baseCOM.x, baseCOM.y, baseCOM.z);

                    #region LIMITS
                    BXDVector3 otherDirection = BXDVector3.CrossProduct(dof.translationalAxis, direction);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(0f, 1f, 0f);
                    GL.Vertex3(dof.translationalAxis.ToTK() * lower);
                    GL.Vertex3(dof.translationalAxis.ToTK() * upper);
                    GL.End();
                    if (dof.hasLowerLinearLimit())
                    {
                        GL.PushMatrix();
                        GL.Translate(dof.translationalAxis.ToTK() * lower);
                        OGLDrawing.drawCrossHair(dof.translationalAxis, direction, crosshairLength);
                        GL.PopMatrix();
                    }
                    if (dof.hasUpperLinearLimit())
                    {
                        GL.PushMatrix();
                        GL.Translate(dof.translationalAxis.ToTK() * upper);
                        OGLDrawing.drawCrossHair(dof.translationalAxis, direction, crosshairLength);
                        GL.PopMatrix();
                    }
                    #endregion
                    GL.PopMatrix();  // part -> COM-basepoint
                }
                #endregion
            }

            GL.PopMatrix();  // World -> part matrix

            // Revert Debug Settings
            GL.Enable(EnableCap.Lighting);
        }

        public override object GetModel()
        {
            return models;
        }

    }
}
