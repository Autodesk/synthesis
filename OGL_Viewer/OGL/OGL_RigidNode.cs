using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

public class OGL_RigidNode : RigidNode_Base
{
    private TransMatrix myTrans = new TransMatrix();
    private List<VBOMesh> models = new List<VBOMesh>();

    public float requestedRotation = 0;
    private float requestedTranslation = 0;
    private BXDVector3 centerOfMass;

    public void loadMeshes(string path)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(path);
        highlight = path.EndsWith("node_4.bxda");
        this.centerOfMass = mesh.physics.centerOfMass;
        foreach (BXDAMesh.BXDASubMesh sub in mesh.meshes)
        {
            models.Add(new VBOMesh(sub));
        }
    }

    private void ensureRotationalLimits(float low, float high)
    {
        if (low > high)
        {
            float temp = high;
            high = low;
            low = temp;
        }
        if (animate)
        {
            requestedRotation *= (high - low) / 1.75f / 6.28f;
            requestedRotation += (high + low) / 2.0f;
            requestedRotation = Math.Min(requestedRotation, high);
            requestedRotation = Math.Max(requestedRotation, low);
        }
    }

    private BXDVector3 activeBasePoint, activeAxis;
    // A*B*C = C, then B, then A
    float i = 0;
    public void compute()
    {
        if (highlight && animate)
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
    public bool highlight = false;

    public void render()
    {
        GL.PushMatrix();
        GL.MultMatrix(myTrans.toBuffer());
        GL.UseProgram(ShaderLoader.PartShader);
        int tintLocation = 0;
        bool tmpHighlight = highlight;
        if (tmpHighlight)
        {
            tintLocation = GL.GetUniformLocation(ShaderLoader.PartShader, "tintColor");
            GL.Uniform4(tintLocation, 1, new float[] { 1, 0, 0, 1 });
        }
        foreach (VBOMesh mesh in models)
        {
            mesh.draw();
        }
        if (tmpHighlight)
        {
            GL.Uniform4(tintLocation, 1, new float[] { 1, 1, 1, 1 });
        }
        GL.UseProgram(0);
        GL.PopMatrix();

        if (highlight)
        {
            renderDebug();
        }
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

            float len = 100;

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(1f, 0f, 0f);
            GL.Vertex3(activeBasePoint.x - activeAxis.x * len, activeBasePoint.y - activeAxis.y * len, activeBasePoint.z - activeAxis.z * len);
            GL.Vertex3(activeBasePoint.x + activeAxis.x * len, activeBasePoint.y + activeAxis.y * len, activeBasePoint.z + activeAxis.z * len);
            GL.End();


            BXDVector3 dirCOM = centerOfMass.Copy().Subtract(activeBasePoint);
            float offset = BXDVector3.DotProduct(dirCOM, activeAxis);
            dirCOM.Multiply(1f / dirCOM.Magnitude());
            BXDVector3 baseCOM = activeBasePoint.Copy().Add(activeAxis.Copy().Multiply(offset / activeAxis.Magnitude()));
            #region ROTATIONAL_LIMIT_DEBUG
            if (GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL || GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL)
            {
                BXDVector3 direction = BXDVector3.CrossProduct(dirCOM, activeAxis);
                direction = BXDVector3.CrossProduct(direction, activeAxis);
                if (BXDVector3.DotProduct(dirCOM, direction) < 0)
                {
                    direction.Multiply(-1f);
                }

                GL.PushMatrix();
                GL.Translate(baseCOM.x, baseCOM.y, baseCOM.z);

                // Current
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(1f, 0f, 1f);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(direction.x * len, direction.y * len, direction.z * len);
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
                        GL.Vertex3(direction.x * len, direction.y * len, direction.z * len);
                        GL.End();
                        GL.Begin(PrimitiveType.LineStrip);
                        // Arcthing
                        TransMatrix stepMatrix = new TransMatrix().identity().setRotation(activeAxis.x, activeAxis.y, activeAxis.z, (maxAngle - minAngle) / 100.0f);
                        BXDVector3 tempVec = direction.Copy();
                        for (float f = 0; f < 1.0f; f += 0.01f)
                        {
                            GL.Color3(0, 1f, 1f - f);
                            GL.Vertex3(tempVec.x * len, tempVec.y * len, tempVec.z * len);
                            tempVec = stepMatrix.multiply(tempVec);
                        }
                        GL.End();
                        GL.PopMatrix(); // Begin limit matrix

                        // Maxpos
                        GL.PushMatrix();
                        GL.Rotate(180.0f / 3.14f * (maxAngle - requestedRotation), activeAxis.x, activeAxis.y, activeAxis.z);

                        GL.Begin(BeginMode.Lines);
                        GL.Color3(0f, 1f, 0f);
                        GL.Vertex3(0, 0, 0);
                        GL.Vertex3(direction.x * len, direction.y * len, direction.z * len);
                        GL.End();

                        GL.PopMatrix(); // End limit matrix
                    }
                }
                GL.PopMatrix();  // part -> COM-basepoint
            }
            #endregion
            #region LINEAR_LIMIT_DEBUG
            if (GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL || GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR)
            {

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
