using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

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

    public float requestedRotation = 0;
    private float requestedTranslation = 0;
    private BXDVector3 centerOfMass;

    public OGL_RigidNode()
    {
        myGUID = SelectManager.AllocateGUID(this);
    }

    public void destroy()
    {
        SelectManager.FreeGUID(myGUID);
        foreach (VBOMesh mesh in models)
        {
            mesh.destroy();
        }
    }


    public void loadMeshes(string path)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(path);
        this.centerOfMass = mesh.physics.centerOfMass;
        foreach (BXDAMesh.BXDASubMesh sub in mesh.meshes)
        {
            models.Add(new VBOMesh(sub));
        }
    }

    private bool initialPositions = false;
    float i = 0;
    public void compute()
    {
        i += 0.005f;

        #region INIT_POSITION
        if (!initialPositions && GetSkeletalJoint() != null)
        {
            initialPositions = true;
            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.CYLINDRICAL:
                    CylindricalJoint_Base cjb = (CylindricalJoint_Base) GetSkeletalJoint();
                    requestedRotation = cjb.currentAngularPosition;
                    requestedTranslation = cjb.currentLinearPosition;
                    break;
                case SkeletalJointType.ROTATIONAL:
                    RotationalJoint_Base rjb = (RotationalJoint_Base) GetSkeletalJoint();
                    requestedRotation = rjb.currentAngularPosition;
                    requestedTranslation = 0;
                    break;
                case SkeletalJointType.LINEAR:
                    LinearJoint_Base ljb = (LinearJoint_Base) GetSkeletalJoint();
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
                    basePoint = ((OGL_RigidNode) GetParent()).myTrans.Multiply(basePoint);
                    axis = ((OGL_RigidNode) GetParent()).myTrans.Rotate(axis);
                }
                if (animate)
                {
                    requestedRotation = (float) (Math.Sin(i) + 0.9f) * 1.2f *
                        (dof.hasAngularLimits() ? (dof.upperAngularLimit - dof.lowerAngularLimit) / 2.0f : 3.14f) +
                        (dof.hasAngularLimits() ? dof.lowerAngularLimit : 0);
                }
                requestedRotation = Math.Max(dof.lowerAngularLimit, Math.Min(dof.upperAngularLimit, requestedRotation));
                myTrans *= Matrix4.CreateTranslation(-dof.basePoint.ToTK());
                myTrans *= Matrix4.CreateFromAxisAngle(dof.rotationAxis.ToTK(), requestedRotation - dof.currentAngularPosition);
                myTrans *= Matrix4.CreateTranslation(dof.basePoint.ToTK());
            }
            foreach (LinearDOF dof in GetSkeletalJoint().GetLinearDOF())
            {
                if (animate)
                {
                    requestedTranslation = (float) (Math.Cos(i) + 0.9f) * 1.2f *
                        (dof.hasLowerLinearLimit() && dof.hasUpperLinearLimit() ? (dof.upperLinearLimit - dof.lowerLinearLimit) / 2.0f : 3.14f) +
                        (dof.hasLowerLinearLimit() ? dof.lowerLinearLimit : 0);
                }
                requestedTranslation = Math.Max(dof.lowerLinearLimit, Math.Min(dof.upperLinearLimit, requestedTranslation));
                myTrans *= Matrix4.CreateTranslation(dof.translationalAxis.ToTK() * (requestedTranslation - dof.currentLinearPosition));
            }
        }
        if (GetParent() != null)
        {
            myTrans = myTrans * ((OGL_RigidNode) GetParent()).myTrans;
        }
        foreach (RigidNode_Base child in children.Values)
        {
            ((OGL_RigidNode) child).compute();
        }
    }

    public bool animate = true;
    public HighlightState highlight = HighlightState.NOTHING;

    public void render(bool select = false)
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
                    GL.Uniform4(tintLocation, 1, new float[] { 1, 0, 1, 1 });
                }
                else if ((tmpHighlight & HighlightState.ACTIVE) == HighlightState.ACTIVE)
                {
                    GL.Uniform4(tintLocation, 1, new float[] { 1, 0, 0, 1 });
                }
                else if ((tmpHighlight & HighlightState.HOVERING) == HighlightState.HOVERING)
                {
                    GL.Uniform4(tintLocation, 1, new float[] { 0.75f, 1, 0.75f, 1 });
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

        if (GetSkeletalJoint() != null)
        {
            GL.PushMatrix();
            GL.MultMatrix(ref myTrans);

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
                        GL.Rotate(180.0f / 3.14f * (dof.lowerAngularLimit - requestedRotation), dof.rotationAxis.ToTK());

                        GL.Begin(PrimitiveType.Lines);
                        GL.Color3(0f, 1f, 1f);
                        GL.Vertex3(0, 0, 0);
                        GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
                        GL.End();
                        OGLDrawing.drawArc(dof.rotationAxis, direction, dof.lowerAngularLimit, dof.upperAngularLimit, crosshairLength);
                        GL.PopMatrix(); // Begin limit matrix

                        // Maxpos
                        GL.PushMatrix();
                        GL.Rotate(180.0f / 3.14f * (dof.upperAngularLimit - requestedRotation), dof.rotationAxis.ToTK());

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
                        OGLDrawing.drawArc(dof.rotationAxis, direction, 0, 6.28f, crosshairLength);
                    }
                }
                #endregion
                GL.PopMatrix();  // part -> COM-basepoint
            }
            #endregion

            #region LINEAR_SPEC
            foreach (LinearDOF dof in GetSkeletalJoint().GetLinearDOF())
            {
                float lower = (dof.hasLowerLinearLimit() ? dof.lowerLinearLimit : -crosshairLength) - requestedTranslation;
                float upper = (dof.hasUpperLinearLimit() ? dof.upperLinearLimit : crosshairLength) - requestedTranslation;

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

            GL.PopMatrix();  // World -> part matrix
        }

        // Revert Debug Settings
        GL.Enable(EnableCap.Lighting);
    }

    public override object GetModel()
    {
        return models;
    }
}
