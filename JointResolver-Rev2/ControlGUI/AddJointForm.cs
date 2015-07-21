using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class AddJointForm : Form
{

    public SkeletalJointType chooseType;

    public AddJointForm()
    {
        InitializeComponent();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
        if (comboboxChooseJoint.SelectedItem == comboboxChooseJoint.Items[0])
            chooseType = SkeletalJointType.ROTATIONAL;
        else if (comboboxChooseJoint.SelectedItem == comboboxChooseJoint.Items[1])
            chooseType = SkeletalJointType.LINEAR;
        else if (comboboxChooseJoint.SelectedItem == comboboxChooseJoint.Items[2])
            chooseType = SkeletalJointType.PLANAR;
        else if (comboboxChooseJoint.SelectedItem == comboboxChooseJoint.Items[3])
            chooseType = SkeletalJointType.CYLINDRICAL;
        else if (comboboxChooseJoint.SelectedItem == comboboxChooseJoint.Items[4])
            chooseType = SkeletalJointType.BALL;
        else throw new NotImplementedException("Selected joint type not found");

        Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

}
