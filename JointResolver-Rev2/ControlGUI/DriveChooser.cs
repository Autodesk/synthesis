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
    /// distance is kept.
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
    private double FindMaxRadius(ComponentOccurrence component, UnitVector centerToEdgeVector)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application) System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];
        double maxRadius = 0;
        double newRadius;
        Vector vertex = ((Inventor.Application) System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();

        Console.WriteLine("Finding radius of " + component.Name + ".");

        foreach (SurfaceBody surface in component.Definition.SurfaceBodies)
        {
            //TODO: use extending box or finding projection on plane.
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);

            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                vertex.X = verticeCoords[i];
                vertex.Y = verticeCoords[i + 1];
                vertex.Z = verticeCoords[i + 2];

                newRadius = centerToEdgeVector.DotProduct(vertex.AsUnitVector()) * Math.Sqrt(Math.Pow(vertex.X, 2) + Math.Pow(vertex.Y, 2) + Math.Pow(vertex.Z, 2));

                if (newRadius > maxRadius)
                {
                    maxRadius = newRadius;
                }
            }
        }

        foreach (ComponentOccurrence sub in component.SubOccurrences)
        {
            newRadius = FindMaxRadius(sub, centerToEdgeVector);

            if (newRadius > maxRadius)
            {
                maxRadius = newRadius;
            }
        }

        return maxRadius;
    }

    //TODO: Modify to handle sub-suboccurrences.
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
        UnitVector randomVector = ((Inventor.Application) System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateUnitVector();
        UnitVector rotationAxis = ((Inventor.Application) System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateUnitVector();

        //Needed to make another vector so that I could use the Vector.CrossProduct function.  This guarentees an output vector normal to 
        //the rotation axis, which is what I need.
        randomVector.X = .707;
        randomVector.Y = 0;
        randomVector.Z = .707;

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

                    maxRadius = FindMaxRadius(component, randomVector.CrossProduct(rotationAxis));
                }

                wheelDriver.radius = (float) maxRadius;
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