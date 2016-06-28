using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Inventor;
 
using System.Diagnostics;
using System.Reflection;

namespace InvEXE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Inventor.Application _InvApplication;
        Macros _macros;
        private void Form1_Load(object sender, EventArgs e)
        {

            try
            {
                _InvApplication = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
            }
            catch (Exception ex)
            {
                MessageBox.Show("make sure an Inventor instance is launched!");
                return;
            }

            // Add any initialization after the InitializeComponent() call.
            _macros = new Macros(_InvApplication);

            MemberInfo[] methods = _macros.GetType().GetMembers();

            foreach (MemberInfo member in methods)
            {
                if ((member.DeclaringType.Name == "Macros" & member.MemberType == MemberTypes.Method))
                {
                    ComboBoxMacros.Items.Add(member.Name);
                }
            }

            if (ComboBoxMacros.Items.Count > 0)
            {
                ComboBoxMacros.SelectedIndex = 0;
                button1.Enabled = true;
            }

 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                string memberName = ComboBoxMacros.SelectedItem.ToString();

                object[] @params = null;
                _macros.GetType().InvokeMember(memberName, BindingFlags.InvokeMethod, null, _macros, @params, null, null, null);
            }

            catch (Exception ex)
            {

                string Caption = ex.Message;
                MessageBoxButtons Buttons = MessageBoxButtons.OK;
                DialogResult Result = MessageBox.Show(ex.StackTrace, Caption, Buttons, MessageBoxIcon.Exclamation);

            } 
        }
    }

       //macro class
    public class Macros
    {

        Inventor.Application _InvApplication;

        public Macros(Inventor.Application oApp)
        {
            _InvApplication = oApp;
        }

        //Small helper function that prompts user for a file selection
        private string OpenFile(string StrFilter)
        {

            string filename = "";

            System.Windows.Forms.OpenFileDialog ofDlg = new System.Windows.Forms.OpenFileDialog();

            string user = System.Windows.Forms.SystemInformation.UserName;

            ofDlg.Title = "Open File";
            ofDlg.InitialDirectory = "C:\\Documents and Settings\\" + user + "\\Desktop\\";

            ofDlg.Filter = StrFilter;
            //Example: "Inventor files (*.ipt; *.iam; *.idw)|*.ipt;*.iam;*.idw"
            ofDlg.FilterIndex = 1;
            ofDlg.RestoreDirectory = true;

            if ((ofDlg.ShowDialog() == DialogResult.OK))
            {
                filename = ofDlg.FileName;
            }

            return filename;

        }

        /// <summary>
    /// create an attribute
    /// this assumes a part document with some features is opened.
    /// </summary>
    /// <remarks></remarks>

    public void AddAttribute()
    {
     // Get the selected edge.  This assumes an edge is already selected.
     Edge oEdge  = (Edge)_InvApplication.ActiveDocument.SelectSet[1];

     // Add an attribute set to the edge.  If you only need to "name" the
     // edge this is enough to find it later and you can skip the next step.
     AttributeSet oAttribSet = oEdge.AttributeSets.Add("BoltEdge");

     // Add an attribute to the set.  This can be any information you
     // want to associate with the edge.
     oAttribSet.Add("BoltRadius", ValueTypeEnum.kDoubleType, 0.5);

    }

        /// <summary>
        /// Query Attribute
        ///  this assumes a part document with some features is opened.
        /// and you have run  AddAttribute on an edge
        /// </summary>
        /// <remarks></remarks>

        public void QueryAttribute()
        {
         // Set a reference to the attribute manager of the active document.
         AttributeManager oAttribMgr = _InvApplication.ActiveDocument.AttributeManager;

         // Get the objects with a particular attribute attached.
         ObjectCollection oObjs  = oAttribMgr.FindObjects("BoltEdge", "BoltRadius", 0.5);

         // Get the objects that have an attribute set of a certain name.
         oObjs = oAttribMgr.FindObjects("BoltEdge");

         // Get the attribute sets with a certain name.
         AttributeSetsEnumerator oAttribSets   = oAttribMgr.FindAttributeSets("BoltEdge");

         // Get the attribute sets with a certain name using a wild card.
         oAttribSets = oAttribMgr.FindAttributeSets("Bolt*");

        }

        /// <summary>
        /// assumes a document is opened    ''' 
        /// </summary>
        /// <remarks></remarks>

        public void Add_Persistent_Transient_Attribute()
        {
            Document oDoc = _InvApplication.ActiveDocument;


            MessageBox.Show("add one attribute set named PersistentAttSet which is persistent" + "\n" + "add one attribute set named TransientAttSet which is transient");

            // add persistent attribute
            AttributeSet oPersistentAttSet = oDoc.AttributeSets.Add("PersistentAttSet");

            oPersistentAttSet.Add("PersistentAtt", ValueTypeEnum.kDoubleType, 0.5);


            //add transient attribute
            AttributeSet oTransientAttSet = oDoc.AttributeSets.AddTransient("TransientAttSet");

            oPersistentAttSet.Add("TransientAtt", ValueTypeEnum.kDoubleType, 1);


            MessageBox.Show("now try to find the attributes PersistentAttSet in the current document");


            // Set a reference to the attribute manager of the active document.
            AttributeManager oAttribMgr = oDoc.AttributeManager;

            AttributeSetsEnumerator oAttSets = oAttribMgr.FindAttributeSets("PersistentAttSet");

            if (oAttSets.Count > 0)
            {
                MessageBox.Show("find Persistent AttSet!");
            }
            else
            {
                MessageBox.Show("cannot find Persistent AttSet!");
            }

            MessageBox.Show("now try to find the attributes PersistentAttSet in the current document");

            oAttSets = oAttribMgr.FindAttributeSets("TransientAttSet");

            if (oAttSets.Count > 0)
            {
                MessageBox.Show("find Transient AttSet!");
            }
            else
            {
                MessageBox.Show("cannot find Transient AttSet!");
            }

            MessageBox.Show("Please save and close this document, open it again. Then click [OK] of this dialog");

            oDoc = _InvApplication.ActiveDocument;

            oAttribMgr = oDoc.AttributeManager;

            oAttSets = oAttribMgr.FindAttributeSets("PersistentAttSet");

            if (oAttSets.Count > 0)
            {
                MessageBox.Show("find Persistent AttSet!");
            }
            else
            {
                MessageBox.Show("cannot find Persistent AttSet!");
            }

            oAttSets = oAttribMgr.FindAttributeSets("TransientAttSet");

            if (oAttSets.Count > 0)
            {
                MessageBox.Show("find Transient AttSet!");
            }
            else
            {
                MessageBox.Show("cannot find Transient AttSet!");
            }
        }

    }
}
