using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace EditorsLibrary
{
    public class JointEditorPane : UserControl
    {
        public delegate void JointEditorEvent(RigidNode_Base node);

        private ListView lstJoints;
        private ColumnHeader item_chType;
        private ColumnHeader item_chParent;
        private ColumnHeader item_chChild;
        private ColumnHeader item_chDrive;
        private ColumnHeader item_chWheel;
        private ColumnHeader item_chSensors;
        private Button listSensorsButton;

        public event JointEditorEvent SelectedJoint;

        private DriveChooser driveChooserDialog = new DriveChooser();

        public JointEditorPane()
        {
            InitializeComponent();
            this.DoLayout(null, null);
        }

        private void InitializeComponent()
        {
            this.lstJoints = new System.Windows.Forms.ListView();
            this.item_chType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chParent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chChild = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chDrive = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chWheel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.item_chSensors = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listSensorsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstJoints
            // 
            this.lstJoints.AutoArrange = false;
            this.lstJoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.item_chType,
            this.item_chParent,
            this.item_chChild,
            this.item_chDrive,
            this.item_chWheel,
            this.item_chSensors});
            this.lstJoints.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstJoints.FullRowSelect = true;
            this.lstJoints.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstJoints.Location = new System.Drawing.Point(0, 0);
            this.lstJoints.MultiSelect = false;
            this.lstJoints.Name = "lstJoints";
            this.lstJoints.ShowGroups = false;
            this.lstJoints.Size = new System.Drawing.Size(800, 548);
            this.lstJoints.TabIndex = 3;
            this.lstJoints.UseCompatibleStateImageBehavior = false;
            this.lstJoints.View = System.Windows.Forms.View.Details;
            this.lstJoints.SelectedIndexChanged += new System.EventHandler(this.lstJoints_SelectedIndexChanged);
            this.lstJoints.DoubleClick += new System.EventHandler(this.lstJoints_DoubleClick);
            // 
            // item_chType
            // 
            this.item_chType.Text = "Joint Type";
            this.item_chType.Width = 100;
            // 
            // item_chParent
            // 
            this.item_chParent.Text = "Fixed";
            this.item_chParent.Width = 80;
            // 
            // item_chChild
            // 
            this.item_chChild.Text = "Child";
            this.item_chChild.Width = 80;
            // 
            // item_chDrive
            // 
            this.item_chDrive.Text = "Driver";
            this.item_chDrive.Width = 160;
            // 
            // item_chWheel
            // 
            this.item_chWheel.Text = "Wheel Type";
            this.item_chWheel.Width = 100;
            // 
            // item_chSensors
            // 
            this.item_chSensors.Text = "Sensor Count";
            this.item_chSensors.Width = 95;
            // 
            // listSensorsButton
            // 
            this.listSensorsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.listSensorsButton.Location = new System.Drawing.Point(678, 554);
            this.listSensorsButton.Name = "listSensorsButton";
            this.listSensorsButton.Size = new System.Drawing.Size(119, 43);
            this.listSensorsButton.TabIndex = 8;
            this.listSensorsButton.Text = "List Sensors";
            this.listSensorsButton.UseVisualStyleBackColor = true;
            this.listSensorsButton.Click += new System.EventHandler(this.listSensors_Click);
            // 
            // JointEditorPane
            // 
            this.Controls.Add(this.lstJoints);
            this.Controls.Add(this.listSensorsButton);
            this.Name = "JointEditorPane";
            this.Size = new System.Drawing.Size(800, 600);
            this.SizeChanged += new System.EventHandler(this.DoLayout);
            this.ResumeLayout(false);

        }

        private void DoLayout(object sender, EventArgs e)
        {
            float[] scales = new float[this.lstJoints.Columns.Count];


            item_chType.Width = this.lstJoints.Width / 8;
            item_chParent.Width = this.lstJoints.Width / 8;
            item_chChild.Width = this.lstJoints.Width / 8;
            item_chDrive.Width = this.lstJoints.Width / 3;
            item_chWheel.Width = this.lstJoints.Width / 8;
            item_chSensors.Width = this.lstJoints.Width / 8;
            
            /*
            for (int i = 0; i < scales.Length; i++)
            {
                scales[i] = this.lstJoints.Columns[i].Width / (float)this.lstJoints.Width;
            }
            for (int i = 0; i < scales.Length; i++)
            {
                this.lstJoints.Columns[i].Width = (int)(scales[i] * this.lstJoints.Width);
            }*/
            this.lstJoints.Width = this.Width;
            this.lstJoints.Height = this.Height - this.listSensorsButton.Height - 6;
            this.listSensorsButton.Top = this.lstJoints.Bottom + 3;
            this.listSensorsButton.Left = this.lstJoints.Right - this.listSensorsButton.Width;
        }

        private void listSensors_Click(object sender, EventArgs e)
        {
            if (lstJoints.SelectedItems.Count > 0 && lstJoints.SelectedItems[0].Tag is RigidNode_Base)
            {
                SensorListForm listForm = new SensorListForm(((RigidNode_Base)lstJoints.SelectedItems[0].Tag).GetSkeletalJoint());
                listForm.ShowDialog();
                this.UpdateJointList();
            }
        }

        private void lstJoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode_Base)
            {
                if (SelectedJoint != null)
                    SelectedJoint((RigidNode_Base)lstJoints.SelectedItems[0].Tag);
            }
            else
            {
                if (SelectedJoint != null)
                    SelectedJoint(null);
            }
        }

        private void lstJoints_DoubleClick(object sender, EventArgs e)
        {
            if (lstJoints.SelectedItems.Count == 1 && lstJoints.SelectedItems[0].Tag is RigidNode_Base)
            {
                SkeletalJoint_Base joint = ((RigidNode_Base)lstJoints.SelectedItems[0].Tag).GetSkeletalJoint();
                driveChooserDialog.ShowDialog(joint, (RigidNode_Base)lstJoints.SelectedItems[0].Tag);
                UpdateJointList();
            }
        }

        private List<RigidNode_Base> nodeList = null;

        public void SetSkeleton(RigidNode_Base root)
        {
            nodeList = root.ListAllNodes();
            UpdateJointList();
        }

        private void UpdateJointList()
        {

            if (nodeList == null)
                return;
            lstJoints.Items.Clear();
            foreach (RigidNode_Base node in nodeList)
            {
                if (node.GetSkeletalJoint() != null)
                {
                    SkeletalJoint_Base joint = node.GetSkeletalJoint();
                    if (joint != null)
                    {
                        WheelDriverMeta wheelData = null;
                        if (joint.cDriver != null)
                        {
                            wheelData = joint.cDriver.GetInfo<WheelDriverMeta>();
                        }

                        System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(new string[] { 
                    Enum.GetName(typeof(SkeletalJointType),joint.GetJointType()).ToLowerInvariant(),
                        node.GetParent().modelFileName,
                        node.modelFileName, joint.cDriver!=null ? joint.cDriver.ToString() : "No driver",
                        wheelData!=null ? wheelData.GetTypeString() : "No Wheel",
                        joint.attachedSensors.Count.ToString()});
                        item.Tag = node;
                        lstJoints.Items.Add(item);
                    }
                }
            }
        }
    }
}