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
    /// Takes the vertices one at a time and gets the distance in the plane of the wheel, orthoganal to the rotation axis.  The largest
    /// distance is kept.  All the vertex points are with the origin of the piece, not of the assembly.  As such, if a wheel is split by
    /// 1000 feet, it will consider them to be side by side when finding the radius.  This may cause issues later.
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
            //TODO: use extending box or finding projection on plane.
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
    public double FindWheelWidth(ComponentOccurrence wheelTread, Vector rotationAxis)
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
        double maxWidth = 0;
        Inventor.Point sideVertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint(0, 0, 0);
        double minWidth = 0.0;

        foreach (SurfaceBody surface in wheelTread.Definition.SurfaceBodies)
        {
            //TODO: use extending box or finding projection on plane.
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);

            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                vertex.X = verticeCoords[i] - sideVertex.X;
                vertex.Y = verticeCoords[i + 1] - sideVertex.Y;
                vertex.Z = verticeCoords[i + 2] - sideVertex.Z;

                newWidth = rotationAxis.DotProduct(vertex);

                if (newWidth > maxWidth)
                {
                    maxWidth = newWidth;
                }
                if (newWidth < minWidth)
                {
                    sideVertex.X = vertex.X;
                    sideVertex.Y = vertex.Y;
                    sideVertex.Z = vertex.Z;

                    minWidth = newWidth;
                }
            }
        }

        return maxWidth;
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
        joint.cDriver = new JointDriver(cType);
        joint.cDriver.portA = (int) txtPortA.Value;
        joint.cDriver.portB = (int) txtPortB.Value;
        joint.cDriver.lowerLimit = (float) txtLowLimit.Value;
        joint.cDriver.upperLimit = (float) txtHighLimit.Value;
        double maxRadius = 0;
        double maxWidth = 0;
        Vector rotationAxis = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        ComponentOccurrence treadPart = null;

        //Needed to make another vector so that I could use the Vector.CrossProduct function.  This guarentees an output vector normal to 
        //the rotation axis, which is what I need.

        //Only need to store wheel driver if run by motor and is a wheel.
        if (JointDriver.IsMotor(cType) && position != WheelPosition.NO_WHEEL)
        {
            wheelDriver = new WheelDriverMeta();
            wheelDriver.position = this.position;

            if (joint is RotationalJoint)
            {
                foreach (ComponentOccurrence component in ((RotationalJoint) joint).GetWrapped().childGroup.occurrences)
                {
                    rotationAxis.X = ((RotationalJoint) joint).axis.x;
                    rotationAxis.Y = ((RotationalJoint) joint).axis.y;
                    rotationAxis.Z = ((RotationalJoint) joint).axis.z;

                    maxRadius = FindMaxRadius(component, rotationAxis, 0.0, out treadPart);
                }

                wheelDriver.radius = (float)maxRadius;
                wheelDriver.width = (float)FindWheelWidth(treadPart, rotationAxis);
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