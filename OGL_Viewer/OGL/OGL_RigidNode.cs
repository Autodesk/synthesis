using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

public class OGL_RigidNode : RigidNode_Base
{
    private TransMatrix myTrans = new TransMatrix();
    private List<VBOMesh> models = new List<VBOMesh>();

    public float requestedRotation = 0;
    private float requestedTranslation = 0;

    public void loadMeshes(string path)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(path);
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

    BXDVector3 baseV = null, axis = null;
    // A*B*C = C, then B, then A
    float i = 0;
    public void compute()
    {
        if (hs && animate)
        {
            i += 0.01f;
            requestedTranslation = (float) Math.Sin(i) * 100.0f;
            requestedRotation = (float) Math.Sin(i) * 6.28f;
        }
        myTrans.identity();
        if (GetSkeletalJoint() != null)
        {
            float modelTranslation = 0, modelRotation = 0;
            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:
                    RotationalJoint_Base rjb = (RotationalJoint_Base) GetSkeletalJoint();
                    baseV = rjb.basePoint;
                    axis = rjb.axis;
                    requestedTranslation = 0;
                    modelRotation = rjb.currentAngularPosition;
                    if (rjb.hasAngularLimit)
                        ensureRotationalLimits(rjb.angularLimitLow, rjb.angularLimitHigh);
                    break;
                case SkeletalJointType.LINEAR:
                    LinearJoint_Base ljb = (LinearJoint_Base) GetSkeletalJoint();
                    baseV = ljb.basePoint;
                    axis = ljb.axis;
                    requestedRotation = 0;
                    modelTranslation = ljb.currentLinearPosition;
                    if (ljb.hasUpperLimit)
                        requestedTranslation = Math.Min(requestedTranslation, ljb.linearLimitHigh);
                    if (ljb.hasLowerLimit)
                        requestedTranslation = Math.Max(requestedTranslation, ljb.linearLimitLow);
                    break;
                case SkeletalJointType.CYLINDRICAL:
                    CylindricalJoint_Base cjb = (CylindricalJoint_Base) GetSkeletalJoint();
                    baseV = cjb.basePoint;
                    axis = cjb.axis;
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
            if (GetParent() != null)
            {
                baseV = ((OGL_RigidNode) GetParent()).myTrans.multiply(baseV);
                axis = ((OGL_RigidNode) GetParent()).myTrans.rotate(axis);
            }
            TransMatrix mat = new TransMatrix();

            mat.identity().setTranslation(baseV.x, baseV.y, baseV.z);
            myTrans.multiply(mat);
            mat.identity().setRotation(axis.x, axis.y, axis.z, requestedRotation - modelRotation);
            mat.setTranslation(axis.x * (requestedTranslation - modelTranslation), axis.y * (requestedTranslation - modelTranslation), axis.z * (requestedTranslation - modelTranslation));
            myTrans.multiply(mat);
            mat.identity().setTranslation(-baseV.x, -baseV.y, -baseV.z);
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

    bool hs;
    public bool animate = true;
    public void render()
    {
        if (hs)
        {
            Gl.glColorMask(true, false, false, true);
        }
        else
        {
            Gl.glColorMask(true, true, true, true);
        }
        Gl.glPushMatrix();
        Gl.glMultMatrixf(myTrans.toBuffer());
        foreach (VBOMesh mesh in models)
        {
            mesh.draw();
        }
        Gl.glPopMatrix();

        if (axis != null && hs)
        {
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glLineWidth(2f);
            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3f(1f, 0f, 0f);
            float len = 100;
            Gl.glVertex3f(baseV.x - axis.x * len, baseV.y - axis.y * len, baseV.z - axis.z * len);
            Gl.glVertex3f(baseV.x + axis.x * len, baseV.y + axis.y * len, baseV.z + axis.z * len);
            Gl.glEnd();
            Gl.glEnable(Gl.GL_LIGHTING);
        }
    }

    public void highlight(bool flag)
    {
        this.hs = flag;
    }

    public override object GetModel()
    {
        return models;
    }
}
