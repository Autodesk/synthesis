using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    private TransMatrix myTrans = new TransMatrix();
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

    private void ensureRotationalLimits(float low, float high)
    {
        if (animate)
        {
            requestedRotation *= (high - low) / 1.75f / 6.28f;
            requestedRotation += (high + low) / 2.0f;
            requestedRotation = Math.Min(requestedRotation, high);
            requestedRotation = Math.Max(requestedRotation, low);
        }
    }

    private bool initialPositions = false;
    private BXDVector3 activeBasePoint, activeAxis;
    // A*B*C = C, then B, then A
    float i = 0;
    public void compute()
    {
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
        if (animate)
        {
            i += 0.01f;
            requestedTranslation = (float) Math.Sin(i) * 100.0f;
            requestedRotation = (float) Math.Sin(i) * 6.28f;
        }
        myTrans.identity();
        if (GetSkeletalJoint() != null)
        {
            float modelTranslation = 0, modelRotation = 0;
            BXDVector3 activeBasePoint = null, activeAxis = null;
            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:
                    RotationalJoint_Base rjb = (RotationalJoint_Base) GetSkeletalJoint();
                    activeBasePoint = rjb.basePoint;
                    activeAxis = rjb.axis;
                    requestedTranslation = 0;
                    modelRotation = rjb.currentAngularPosition;
                    if (rjb.hasAngularLimit)
                        ensureRotationalLimits(rjb.angularLimitLow, rjb.angularLimitHigh);
                    break;
                case SkeletalJointType.LINEAR:
                    LinearJoint_Base ljb = (LinearJoint_Base) GetSkeletalJoint();
                    activeBasePoint = ljb.basePoint;
                    activeAxis = ljb.axis;
                    requestedRotation = 0;
                    modelTranslation = ljb.currentLinearPosition;
                    if (ljb.hasUpperLimit)
                        requestedTranslation = Math.Min(requestedTranslation, ljb.linearLimitHigh);
                    if (ljb.hasLowerLimit)
                        requestedTranslation = Math.Max(requestedTranslation, ljb.linearLimitLow);
                    break;
                case SkeletalJointType.CYLINDRICAL:
                    CylindricalJoint_Base cjb = (CylindricalJoint_Base) GetSkeletalJoint();
                    activeBasePoint = cjb.basePoint;
                    activeAxis = cjb.axis;
                    modelRotation = cjb.currentAngularPosition;
                    modelTranslation = cjb.currentLinearPosition;
                    if (cjb.hasLinearEndLimit)
                        requestedTranslation = Math.Min(requestedTranslation, cjb.linearLimitEnd);
                    if (cjb.hasLinearStartLimit)
                        requestedTranslation = Math.Max(requestedTranslation, cjb.linearLimitStart);
                    if (cjb.hasAngularLimit)
                        ensureRotationalLimits(cjb.angularLimitLow, cjb.angularLimitHigh);
                    break;
            }

            this.activeAxis = activeAxis;
            this.activeBasePoint = activeBasePoint;
            if (GetParent() != null)
            {
                activeBasePoint = ((OGL_RigidNode) GetParent()).myTrans.multiply(activeBasePoint);
                activeAxis = ((OGL_RigidNode) GetParent()).myTrans.rotate(activeAxis);
            }
            TransMatrix mat = new TransMatrix();

            mat.identity().setTranslation(activeBasePoint.x, activeBasePoint.y, activeBasePoint.z);
            myTrans.multiply(mat);
            mat.identity().setRotation(activeAxis.x, activeAxis.y, activeAxis.z, requestedRotation - modelRotation);
            mat.setTranslation(activeAxis.x * (requestedTranslation - modelTranslation), activeAxis.y * (requestedTranslation - modelTranslation), activeAxis.z * (requestedTranslation - modelTranslation));
            myTrans.multiply(mat);
            mat.identity().setTranslation(-activeBasePoint.x, -activeBasePoint.y, -activeBasePoint.z);
            myTrans.multiply(mat);
        }
        if (GetParent() != null)
        {
            myTrans.multiply(((OGL_RigidNode) GetParent()).myTrans);
        }
        foreach (RigidNode_Base child in children.Values)
        {
            ((OGL_RigidNode) child).compute();
        }
    }

    public bool animate = false;
    public HighlightState highlight = HighlightState.NOTHING;

    public void render(bool select = false)
    {
        int tintLocation = 0;
        HighlightState tmpHighlight = highlight;

        GL.PushMatrix();
        GL.MultMatrix(myTrans.toBuffer());

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

        if (GetSkeletalJoint() != null && activeBasePoint != null && activeAxis != null)
        {

            GL.PushMatrix();
            GL.MultMatrix(myTrans.toBuffer());

            float crosshairLength = 100;


            #region LIMITS_DEBUG
            {
                BXDVector3 dirCOM = centerOfMass.Copy().Subtract(activeBasePoint);
                float offset = BXDVector3.DotProduct(dirCOM, activeAxis);
                BXDVector3 baseCOM = activeBasePoint.Copy();

                if (BXDVector3.CrossProduct(dirCOM, activeAxis).Magnitude() < 1E-10)    // COM is on the axis. (>.>)  Pick randomlyish.
                {
                    dirCOM = new BXDVector3(.123213, 123213, 0.82134); // Certain to be random.
                }
                dirCOM.Multiply(1f / dirCOM.Magnitude());
                baseCOM.Add(activeAxis.Copy().Multiply(offset / activeAxis.Magnitude()));

                BXDVector3 direction = BXDVector3.CrossProduct(dirCOM, activeAxis);
                direction = BXDVector3.CrossProduct(direction, activeAxis);
                if (BXDVector3.DotProduct(dirCOM, direction) < 0)
                {
                    direction.Multiply(-1f);
                }


                GL.PushMatrix();
                GL.Translate(baseCOM.x, baseCOM.y, baseCOM.z);
                #region ROTATIONAL_LIMIT_DEBUG
                if (GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL || GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL)
                {
                    if (GetSkeletalJoint().GetJointType() != SkeletalJointType.CYLINDRICAL) // Linear limits show the axis anyways.
                    {
                        // Rotational Axis
                        GL.Begin(PrimitiveType.Lines);
                        GL.Color3(1f, 0f, 0f);
                        GL.Vertex3(-activeAxis.x * crosshairLength, -activeAxis.y * crosshairLength, -activeAxis.z * crosshairLength);
                        GL.Vertex3(activeAxis.x * crosshairLength, activeAxis.y * crosshairLength, activeAxis.z * crosshairLength);
                        GL.End();
                    }

                    // Current
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(1f, 0f, 1f);
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
                    GL.End();

                    {
                        bool hasAngularLimits = false;
                        float minAngle = 0, maxAngle = 0, modelRotation = 0;
                        switch (GetSkeletalJoint().GetJointType())
                        {
                            case SkeletalJointType.ROTATIONAL:
                                RotationalJoint_Base rjb = (RotationalJoint_Base) GetSkeletalJoint();
                                hasAngularLimits = rjb.hasAngularLimit;
                                minAngle = rjb.angularLimitLow;
                                maxAngle = rjb.angularLimitHigh;
                                modelRotation = rjb.currentAngularPosition;
                                break;
                            case SkeletalJointType.CYLINDRICAL:
                                CylindricalJoint_Base cjb = (CylindricalJoint_Base) GetSkeletalJoint();
                                hasAngularLimits = cjb.hasAngularLimit;
                                minAngle = cjb.angularLimitLow;
                                maxAngle = cjb.angularLimitHigh;
                                modelRotation = cjb.currentAngularPosition;
                                break;
                        }
                        if (hasAngularLimits)
                        {
                            // Minpos
                            GL.PushMatrix();
                            GL.Rotate(180.0f / 3.14f * (minAngle - requestedRotation), activeAxis.x, activeAxis.y, activeAxis.z);

                            GL.Begin(PrimitiveType.Lines);
                            GL.Color3(0f, 1f, 1f);
                            GL.Vertex3(0, 0, 0);
                            GL.Vertex3(direction.x * crosshairLength, direction.y * crosshairLength, direction.z * crosshairLength);
                            GL.End();
                            GL.Begin(PrimitiveType.LineStrip);
                            // Arcthing
                            TransMatrix stepMatrix = new TransMatrix().identity().setRotation(activeAxis.x, activeAxis.y, activeAxis.z, (maxAngle - minAngle) / 100.0f);
                            BXDVector3 tempVec = direction.Copy();
                            for (float f = 0; f < 1.0f; f += 0.01f)
                            {
                                GL.Color3(0, 1f, 1f - f);
                                GL.Vertex3(tempVec.x * crosshairLength, tempVec.y * crosshairLength, tempVec.z * crosshairLength);
                                tempVec = stepMatrix.multiply(tempVec);
                            }
                            GL.End();
                            GL.PopMatrix(); // Begin limit matrix

                            // Maxpos
                            GL.PushMatrix();
                            GL.Rotate(180.0f / 3.14f * (maxAngle - requestedRotation), activeAxis.x, activeAxis.y, activeAxis.z);

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
                            GL.Begin(PrimitiveType.LineLoop);
                            TransMatrix stepMatrix = new TransMatrix().identity().setRotation(activeAxis.x, activeAxis.y, activeAxis.z, (2.0f * (float) Math.PI) / 100.0f);
                            BXDVector3 tempVec = direction.Copy();
                            for (float f = 0; f < 1.0f; f += 0.01f)
                            {
                                GL.Color3(0, 1f, 1f - f);
                                GL.Vertex3(tempVec.x * crosshairLength, tempVec.y * crosshairLength, tempVec.z * crosshairLength);
                                tempVec = stepMatrix.multiply(tempVec);
                            }
                            GL.End();
                        }
                    }
                }
                #endregion
                #region LINEAR_LIMIT_DEBUG
                if (GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL || GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR)
                {
                    bool hasLower = false, hasUpper = false;
                    float lower = -crosshairLength, upper = crosshairLength;
                    switch (GetSkeletalJoint().GetJointType())
                    {
                        case SkeletalJointType.CYLINDRICAL:
                            CylindricalJoint_Base cjb = (CylindricalJoint_Base) GetSkeletalJoint();
                            hasLower = cjb.hasLinearStartLimit;
                            hasUpper = cjb.hasLinearEndLimit;
                            if (hasLower)
                                lower = cjb.linearLimitStart - cjb.currentLinearPosition;
                            if (hasUpper)
                                upper = cjb.linearLimitEnd - cjb.currentLinearPosition;
                            break;
                        case SkeletalJointType.LINEAR:
                            LinearJoint_Base ljb = (LinearJoint_Base) GetSkeletalJoint();
                            hasLower = ljb.hasLowerLimit;
                            hasUpper = ljb.hasUpperLimit;
                            if (hasLower)
                                lower = ljb.linearLimitLow - ljb.currentLinearPosition;
                            if (hasUpper)
                                upper = ljb.linearLimitHigh - ljb.currentLinearPosition;
                            break;
                    }
                    BXDVector3 otherDirection = BXDVector3.CrossProduct(activeAxis, direction);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(0f, 1f, 0f);
                    GL.Vertex3(activeAxis.x * lower, activeAxis.y * lower, activeAxis.z * lower);
                    GL.Vertex3(activeAxis.x * upper, activeAxis.y * upper, activeAxis.z * upper);
                    GL.End();
                    if (hasLower)
                    {
                        GL.PushMatrix();
                        GL.Translate(activeAxis.x * lower, activeAxis.y * lower, activeAxis.z * lower);
                        GL.Begin(PrimitiveType.Lines);
                        foreach (BXDVector3 seg in new BXDVector3[] { direction, otherDirection })
                        {
                            GL.Vertex3(-seg.x * crosshairLength, -seg.y * crosshairLength, -seg.z * -crosshairLength);
                            GL.Vertex3(seg.x * crosshairLength, seg.y * crosshairLength, seg.z * -crosshairLength);
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
                    if (hasUpper)
                    {
                        GL.PushMatrix();
                        GL.Translate(activeAxis.x * upper, activeAxis.y * upper, activeAxis.z * upper);
                        GL.Begin(PrimitiveType.Lines);
                        foreach (BXDVector3 seg in new BXDVector3[] { direction, otherDirection })
                        {
                            GL.Vertex3(-seg.x * crosshairLength, -seg.y * crosshairLength, -seg.z * -crosshairLength);
                            GL.Vertex3(seg.x * crosshairLength, seg.y * crosshairLength, seg.z * -crosshairLength);
                        }
                        GL.End();
                        GL.PopMatrix();
                    }
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
