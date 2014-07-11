using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Inventor;


public partial class DriveChooser : Form
{
    public DriveChooser()
    {
        InitializeComponent();
    }

    private JointDriverType[] typeOptions;
    private SkeletalJoint_Base joint;
    private WheelPosition position;

    public void ShowDialog(SkeletalJoint_Base joint)
    {
        this.joint = joint;
        typeOptions = JointDriver.GetAllowedDrivers(joint);
        cmbJointDriver.Items.Clear();
        foreach (JointDriverType type in typeOptions)
        {
            cmbJointDriver.Items.Add(Enum.GetName(typeof(JointDriverType), type).Replace('_', ' ').ToLowerInvariant());
        }
        cmbJointDriver.SelectedIndex = 0;
        if (joint.cDriver != null)
        {
            cmbJointDriver.SelectedIndex = Array.IndexOf(typeOptions, joint.cDriver.GetDriveType());
            cmbJointDriver_SelectedIndexChanged(null, null);
            txtPortA.Value = joint.cDriver.portA;
            txtPortB.Value = joint.cDriver.portB;
            txtLowLimit.Value = (decimal) joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal) joint.cDriver.upperLimit;
        }
        ShowDialog();
    }

    private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
    {
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        lblPort.Text = JointDriver.GetPortType(cType) + " Port" + (JointDriver.HasTwoPorts(cType) ? "s" : "");
        txtPortB.Visible = JointDriver.HasTwoPorts(cType);
        txtPortA.Maximum = txtPortB.Maximum = JointDriver.GetPortMax(cType);
        groupBox1.Visible = JointDriver.IsMotor(cType);
        if (JointDriver.IsMotor(cType) == true)
        {
            this.Height = 360;
            btnSave.Location = new System.Drawing.Point(13, 280);
            btnSave.Visible = true;
        }
        else if (JointDriver.IsMotor(cType) == false)
        {
            this.Height = 300;
            btnSave.Location = new System.Drawing.Point(13, 220);
            btnSave.Visible = true;
        }
    }

    /// <summary>
    /// Takes the vertices one at a time and gets the distance in the plane of the wheel, orthoganal to the rotation axis.  It does not find the actual
    /// radius of the entire wheel, it finds the largest radius of a part in the plane of the wheel.  It treats it as if all the components of a wheel
    /// are in the same position.
    /// </summary>
    /// <param name="component">
    /// Which part to find the furthest vertex of.
    /// </param>
    /// <param name="centerToEdgeVector">
    /// A unit vector normal to the axis of rotation.  Any of the infinite number will work.
    /// </param>
    /// <returns>
    /// The distance of the furthest vector of the occurrence and all of its suboccurrences.
    /// </returns>
    private double FindMaxRadius(ComponentOccurrence component, Vector rotationAxis, double currentMaxRadius, out ComponentOccurrence treadPart)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application) System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];
        double newRadius;
        Vector vertex = ((Inventor.Application) System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Vector projectedVector = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        treadPart = null;
        double maxRadius = 0;
        

        Console.WriteLine("Finding radius of " + component.Name + ".");

        foreach (ComponentOccurrence sub in component.SubOccurrences)
        {
            newRadius = FindMaxRadius(sub, rotationAxis, maxRadius, out component);

            if (newRadius > currentMaxRadius)
            {
                currentMaxRadius = newRadius;

                treadPart = component;
            }
        }

        foreach (SurfaceBody surface in component.Definition.SurfaceBodies)
        {
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);

            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                vertex.X = verticeCoords[i];
                vertex.Y = verticeCoords[i + 1];
                vertex.Z = verticeCoords[i + 2];

                projectedVector = rotationAxis.CrossProduct(vertex);

                newRadius = Math.Sqrt(Math.Pow(projectedVector.X, 2) + Math.Pow(projectedVector.Y, 2) + Math.Pow(projectedVector.Z, 2));

                if (newRadius > currentMaxRadius)
                {
                    currentMaxRadius = newRadius;

                    treadPart = component;
                }
            }
        }

        return currentMaxRadius;
    }

    /// <summary>
    /// Finds the width of the provided part of the wheel.
    /// </summary>
    /// <param name="wheelTread">
    /// The part of the wheel that actually touches the ground.
    /// </param>
    /// <param name="rotationAxis">
    /// The rotation normal for the rotation axis.
    /// </param>
    /// <returns>
    /// The width of the part in centimeters.
    /// </returns>
    public void FindWheelWidthCenter(ComponentOccurrence wheelTread, Vector rotationAxis, out double maxWidth, out Vector center)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];
        double newWidth;
        Vector vertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Vector projectedVector = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Inventor.Point sideVertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint(0, 0, 0);
        double minWidth = 0.0;
        maxWidth = 0;
        center = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(0, 0, 0);

        foreach (SurfaceBody surface in wheelTread.Definition.SurfaceBodies)
        {
            //TODO: use extending box or finding projection on plane.
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);
            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                center.X += verticeCoords[i];
                center.Y += verticeCoords[i + 1];
                center.Z += verticeCoords[i + 2];

                vertex.X = verticeCoords[i] - sideVertex.X;
                vertex.Y = verticeCoords[i + 1] - sideVertex.Y;
                vertex.Z = verticeCoords[i + 2] - sideVertex.Z;

                newWidth = rotationAxis.DotProduct(vertex);

                //Stores the distance to the point. 
                if (newWidth > maxWidth)
                {
                    maxWidth = newWidth;
                }
                //Changes the starting point when detecting distance for later vertices.
                if (newWidth < minWidth)
                {
                    sideVertex.X = vertex.X;
                    sideVertex.Y = vertex.Y;
                    sideVertex.Z = vertex.Z;

                    minWidth = newWidth;
                }

                //These two statements result in an end where the starting point is on one side of the wheel, and the distance that is being
                //      calculated and stored is for a vertex on the other side of the wheel.
            }

            //May cause issue if there are multiple surfaces.

            center.X = center.X / vertexCount;
            center.Y = center.Y / vertexCount;
            center.Z = center.Z / vertexCount;
        }

       
    }

    /// <summary>
    /// Saves all the data from the DriveChooser frame to be used elsewhere in the program.  Also begins calculation of wheel radius.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave_Click(object sender, EventArgs e)
    {
        WheelDriverMeta wheelDriver;
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        double maxRadius = 0;
        Vector rotationAxis = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        ComponentOccurrence treadPart = null;
        Matrix asmToPart = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateMatrix();
        Matrix transformedVector = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateMatrix();
        Inventor.Point origin;
        Vector partXAxis;
        Vector partYAxis;
        Vector partZAxis;
        Vector asmXAxis = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(1, 0, 0);
        Vector asmYAxis = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(0, 1, 0);
        Vector asmZAxis = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(0, 0, 1);
        Vector center;
        Inventor.Point wheelNode = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        double maxWidth;

        joint.cDriver = new JointDriver(cType);

        joint.cDriver.portA = (int) txtPortA.Value;
        joint.cDriver.portB = (int) txtPortB.Value;
        joint.cDriver.lowerLimit = (float) txtLowLimit.Value;
        joint.cDriver.upperLimit = (float) txtHighLimit.Value;

        //Only need to store wheel driver if run by motor and is a wheel.
        if (JointDriver.IsMotor(cType) && position != WheelPosition.NO_WHEEL)
        {
            wheelDriver = new WheelDriverMeta();
            wheelDriver.position = this.position;

            if (joint is RotationalJoint)
            {
                foreach (ComponentOccurrence component in ((RotationalJoint)joint).GetWrapped().childGroup.occurrences)
                {
                    //Takes the part axes and the assembly axes and creates a transformation from one to the other.
                    component.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

                    asmToPart.SetToAlignCoordinateSystems(origin, partXAxis, partYAxis, partZAxis, origin, asmXAxis, asmYAxis, asmZAxis);

                    transformedVector.Cell[1, 1] = ((RotationalJoint)joint).axis.x;
                    transformedVector.Cell[2, 1] = ((RotationalJoint)joint).axis.y;
                    transformedVector.Cell[3, 1] = ((RotationalJoint)joint).axis.z;

                    Console.Write("Changing vector from " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1]);

                    transformedVector.TransformBy(asmToPart);

                    rotationAxis.X = transformedVector.Cell[1, 1];
                    rotationAxis.Y = transformedVector.Cell[2, 1];
                    rotationAxis.Z = transformedVector.Cell[3, 1];

                    Console.Write(" to " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1] + ".\n");

                    maxRadius = FindMaxRadius(component, rotationAxis, 0.0, out treadPart);
                }

                wheelDriver.radius = (float)maxRadius;
                FindWheelWidthCenter(treadPart, rotationAxis, out maxWidth, out center);

                wheelDriver.width = (float)maxWidth;

                //Vector testVector = treadPart.Transformation.SetTranslation();
                treadPart.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

                wheelNode.X = treadPart.Transformation.Translation.X;
                wheelNode.Y = treadPart.Transformation.Translation.Y;
                wheelNode.Z = treadPart.Transformation.Translation.Z;

                asmToPart.SetToAlignCoordinateSystems(origin, asmXAxis, asmYAxis, asmZAxis, origin, partXAxis, partYAxis, partZAxis);

                center.TransformBy(asmToPart);

                wheelDriver.centerX = (float)(center.X + wheelNode.X);
                wheelDriver.centerY = (float)(center.Y + wheelNode.Y);
                wheelDriver.centerZ = (float)(center.Z + wheelNode.Z);
            }

            joint.cDriver.AddInfo(wheelDriver);
        }

        Hide();
    }

    private void cmbWheelPosition_SelectedIndexChanged(object sender, EventArgs e)
    {
        position = (WheelPosition) cmbWheelPosition.SelectedIndex;
    }

    private void txtPortA_ValueChanged(object sender, EventArgs e)
    {

    }

    private void txtPortB_ValueChanged(object sender, EventArgs e)
    {

    }

    private void txtLowLimit_ValueChanged(object sender, EventArgs e)
    {

    }

    private void lblPort_Click(object sender, EventArgs e)
    {

    }

    private void grpDriveOptions_Enter(object sender, EventArgs e)
    {

    }

    private void groupBox1_Enter(object sender, EventArgs e)
    {

    }
}