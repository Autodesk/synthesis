using System;
using System.Windows.Forms;
using System.Reflection;
using Inventor;


namespace RefKeys
{
    public partial class Form1 : Form
    {
        Inventor.Application mApp;

        //Member variables to keep the RefKey data
        private Byte[] oRefKey = new byte[]{};
        private Byte[] oContextData = new byte[]{};

        public Form1(Inventor.Application oApp)
        {
            InitializeComponent();

            mApp = oApp;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
             if (GetRefKeyFromSelectedFace()) 
             {
                SaveRefKey();
             }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
             if (LoadRefKey())
            {
                SelectFaceFromRefKey();
            }
        }


        //Get reference key and context data for a given face of a given given document
        //Document can be either a Part or an Assembly
         void GetFaceReferenceKey(Document oDoc, Face oFace, ref byte[] oRefKey, ref byte[] oContextData)
        {
            //Create a key context (required to obtain ref keys for BRep entities)
            int KeyContext = oDoc.ReferenceKeyManager.CreateKeyContext();

            //Get a reference key for the face
            oFace.GetReferenceKey(ref oRefKey, KeyContext);
            
            //Save KeyContext as a byte array for future use
            oDoc.ReferenceKeyManager.SaveContextToArray(KeyContext, ref oContextData);
        }

        //Retrieve a Face from its Reference Key and Context Data
        Face GetFaceFromReferenceKey(Document oDoc, ref  byte[] oRefKey, ref  byte[] oContextData) 
        {
            try
            {
                //Retrieve ContextKey from byte array
                int oKeyContext = oDoc.ReferenceKeyManager.LoadContextFromArray(ref oContextData);

                //Bind reference key to the Face object
                object MatchType;
                Face oFace = oDoc.ReferenceKeyManager.BindKeyToObject(ref oRefKey, oKeyContext, out MatchType) as Face;

                //Return result
                return oFace;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private bool GetRefKeyFromSelectedFace()
        {
            if (mApp.ActiveDocument == null)
            {
                System.Windows.Forms.MessageBox.Show("A document needs to be active", "RefKeys Error");
                return false;
            }

            if (mApp.ActiveDocument.SelectSet.Count == 1)
            {
                object obj = mApp.ActiveDocument.SelectSet[1];

                if (obj is Face) 
                {
                    //Get selected face
                    Face oFace = mApp.ActiveDocument.SelectSet[1] as Face;

                    GetFaceReferenceKey(mApp.ActiveDocument, oFace, ref oRefKey, ref oContextData);
                    return true;
                }
            }

            System.Windows.Forms.MessageBox.Show("A single Face needs to be selected", "RefKeys Error");
            return false;
        }

        private void SelectFaceFromRefKey()
        {
            if (mApp.ActiveDocument == null)
            {
                System.Windows.Forms.MessageBox.Show("A document needs to be active", "RefKeys Error");
                return;
            }

            Face oFace = GetFaceFromReferenceKey(mApp.ActiveDocument, ref oRefKey, ref oContextData);

            if (oFace != null) 
            {
                mApp.ActiveDocument.SelectSet.Clear();
                mApp.ActiveDocument.SelectSet.Select(oFace);
            }
        }

        //Save a reference key data on the disk
        private void SaveRefKey()
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();

            sfd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            sfd.FileName = "EntityRefkey";

            sfd.Filter = "RefKeyData (*.ref) |*.ref;";

            if( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileStream fs = System.IO.File.Create(sfd.FileName);
                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(fs);

                bw.Write(oContextData.Length);
                bw.Write(oContextData);

                bw.Write(oRefKey.Length);
                bw.Write(oRefKey);

                bw.Close();
                fs.Close();
            }
        }

        //Load a reference key data from the disk
        private bool LoadRefKey()
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();

            ofd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            ofd.Filter = "RefKeyData (*.ref) |*.ref;";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(ofd.FileName);
                System.IO.BinaryReader br = new System.IO.BinaryReader(fs);

                int count = 0;

                count = br.ReadInt32();
                oContextData = br.ReadBytes(count);

                count = br.ReadInt32();
                oRefKey = br.ReadBytes(count);

                br.Close();
                fs.Close();

                return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void button3_Click(object sender, EventArgs e)
        {
            if (GetRefKeyFromSelectedEntity()) 
            {
                SaveRefKey();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (LoadRefKey())
            {
                SelectEntityFromRefKey();
            }
        }
        
        private bool GetReferenceKeyFromObject(Document oDoc, object obj, ref byte[] oRefKey, ref byte[] oContextData)
        {
            try
            {
                int KeyContext = oDoc.ReferenceKeyManager.CreateKeyContext();

                object[] Params = new object[2];

                Params[0] = oRefKey;
                Params[1] = KeyContext;

                // Set up the parameter modifiers.
                ParameterModifier[] mods = new ParameterModifier[2]{new ParameterModifier(1), new ParameterModifier(1)};

                // Set the first parameter to be allowed to be modified.
                mods[0][0] = true; 

                obj.GetType().InvokeMember("GetReferenceKey", System.Reflection.BindingFlags.InvokeMethod, null, obj, Params, mods, null, null);

                oRefKey = (byte[])Params[0]; 

                oDoc.ReferenceKeyManager.SaveContextToArray(KeyContext, ref oContextData);

                return true;
            }
            catch (Exception ex)
            {
                oRefKey = null;
                oContextData = null;
                return false;
            }
        }

        object GetObjectFromReferenceKey(Document oDoc, ref  byte[] oRefKey, ref  byte[] oContextData, out object MatchType)
        {
            try
            {
                int oKeyContext = oDoc.ReferenceKeyManager.LoadContextFromArray(ref oContextData);

                object obj = oDoc.ReferenceKeyManager.BindKeyToObject(ref oRefKey, oKeyContext, out MatchType);

                return obj;
            }
            catch (Exception ex)
            {
                MatchType = null;
                return null;
            }
        }

        private bool GetRefKeyFromSelectedEntity()
        {
            if (mApp.ActiveDocument == null)
            {
                System.Windows.Forms.MessageBox.Show("A document needs to be active", "RefKeys Error");
                return false;
            }

            if (mApp.ActiveDocument.SelectSet.Count == 1)
            {
                object obj = mApp.ActiveDocument.SelectSet[1];

                if(GetReferenceKeyFromObject(mApp.ActiveDocument, obj, ref oRefKey, ref oContextData))
                {
                    return true;
                }

                System.Windows.Forms.MessageBox.Show("Selected entity doesn't support Reference Keys", "RefKeys Error");
                return false;
            }

            System.Windows.Forms.MessageBox.Show("A single Entity needs to be selected", "RefKeys Error");
            return false;
        }

        private void SelectEntityFromRefKey()
        {
            if (mApp.ActiveDocument == null)
            {
                System.Windows.Forms.MessageBox.Show("A document needs to be active", "RefKeys Error");
                return;
            }

            object MatchType;
            object obj = GetObjectFromReferenceKey(mApp.ActiveDocument, ref oRefKey, ref oContextData, out MatchType);

            if (obj != null)
            {
                mApp.ActiveDocument.SelectSet.Clear();
                mApp.ActiveDocument.SelectSet.Select(obj);
            }
        }

       
    }
}
