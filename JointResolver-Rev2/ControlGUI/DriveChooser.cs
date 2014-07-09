using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            txtLowLimit.Value = (decimal)joint.cDriver.lowerLimit;
            txtHighLimit.Value = (decimal)joint.cDriver.upperLimit;
        }
        ShowDialog();
    }

    private void cmbJointDriver_SelectedIndexChanged(object sender, EventArgs e)
    {
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        lblPort.Text = JointDriver.GetPortType(cType) + " Port" + (JointDriver.HasTwoPorts(cType) ? "s" : "");
        txtPortB.Visible = JointDriver.HasTwoPorts(cType);
        if (txtPortB.Visible == false)
        {
            txtPortB.Visible = JointDriver.HasTwoPortsCheck(cType);
        }
        txtPortA.Maximum = txtPortB.Maximum = JointDriver.GetPortMax(cType);
        groupBox1.Visible = JointDriver.IsMotor(cType);
        if (JointDriver.IsMotor(cType) == true)
        {
            this.Height = 360;
            btnSave.Location = new System.Drawing.Point (13, 280);
            btnSave.Visible = true;
        } 
        else if (JointDriver.IsMotor(cType) == false)
        {
            this.Height = 300;
            btnSave.Location = new System.Drawing.Point (13, 220);
            btnSave.Visible = true;
        }
    }

    /// <summary>
    /// Creates a set of vertices for a component and all of its sub components.  A bounding box is then created to contain all of the 
    /// vertices.  Currently, the Y dimension is taken as the wheel's diameter.  Later, the rotation normal will be used to find the
    /// correct dimension of the radius.  Could find the two dimensions that are closest in distance, as the wheel is round.
    /// </summary>
    /// <param name="component">
    /// The component to be encapsulated by the returned box.
    /// </param>
    /// <param name="wheelBox">
    /// The reference in which to store the created box.
    /// </param>
    /// <returns>
    /// The expanded box to include the most recently added component.
    /// </returns>
    private Box ContainInBox(ComponentOccurrence component, Box wheelBox)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];

        Console.WriteLine("Containing " + component.Name + " in as Box.");

        foreach (SurfaceBody surface in component.Definition.SurfaceBodies)
        {
            //TODO: use extending box or finding projection on plane.
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);
            
            
            for (int i = 0; i < verticeCoords.Length; i+=3)
            {
                tmp.X = verticeCoords[i];
                tmp.Y = verticeCoords[i+1];
                tmp.Z = verticeCoords[i+2];

                if (wheelBox == null)
                {
                    Inventor.Application inv = (Inventor.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
                    wheelBox = inv.TransientGeometry.CreateBox();
                    wheelBox.MinPoint = tmp.Copy();
                    wheelBox.MaxPoint = tmp.Copy();
                }
                else
                {
                    if (i == verticeCoords.Length - 2)
                    {
                        int test = 5;
                    }
                    wheelBox.Extend(tmp);
                }
            }

        }

        foreach (ComponentOccurrence sub in component.SubOccurrences)
        {
            wheelBox = this.ContainInBox(sub, wheelBox);
        }

        return wheelBox;
    }


    /// <summary>
    /// Saves all the data from the DriveChooser frame to be used elsewhere in the program.  Also begins calculation of wheel radius.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void btnSave_Click(object sender, EventArgs e)
    {
        WheelDriverMeta wheelDriver;
        Box wheelBox = null;
        JointDriverType cType = typeOptions[cmbJointDriver.SelectedIndex];
        joint.cDriver = new JointDriver(cType);
        joint.cDriver.portA = (int)txtPortA.Value;
        joint.cDriver.portB = (int)txtPortB.Value;
        joint.cDriver.lowerLimit = (float)txtLowLimit.Value;
        joint.cDriver.upperLimit = (float)txtHighLimit.Value;

        //Only need to store wheel driver if run by motor and is a wheel.
        if (cType == JointDriverType.MOTOR && position != WheelPosition.NO_WHEEL)
        {
            wheelDriver = new WheelDriverMeta();
            wheelDriver.position = this.position;

            if (joint is RotationalJoint)
            {

                //Enter the rabbit hole.
                foreach (ComponentOccurrence component in ((RotationalJoint)joint).GetWrapped().childGroup.occurrences)
                {
                    wheelBox = this.ContainInBox(component, wheelBox);    
                }


                wheelDriver.radius = ((float)(wheelBox.MaxPoint.X - wheelBox.MinPoint.X)) / 2;
            }

            joint.cDriver.AddInfo(wheelDriver);
        }

        Hide();
    }

    private void cmbWheelPosition_SelectedIndexChanged(object sender, EventArgs e)
    {
         position = (WheelPosition)cmbWheelPosition.SelectedIndex;
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