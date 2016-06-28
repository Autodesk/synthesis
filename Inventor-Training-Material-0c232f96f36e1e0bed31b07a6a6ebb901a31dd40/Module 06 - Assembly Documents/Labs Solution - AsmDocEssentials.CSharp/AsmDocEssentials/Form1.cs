using System;
using Inventor;


namespace AsmDocEssentials
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private Inventor.Application mApp = null;
        private DigitTextBox mAngleBox; 

        public Form1(Inventor.Application oApp)
        {
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            mAngleBox = new DigitTextBox();
            mAngleBox.Name = "AngleBox";
            mAngleBox.Location = new System.Drawing.Point(80, 197);
            mAngleBox.Size = new System.Drawing.Size(48, 30);
            mAngleBox.TabIndex = 3;
            mAngleBox.Text = "";

            this.Controls.Add(mAngleBox);

            mApp = oApp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VectorControl1.SetApp = mApp;
            VectorControl2.SetApp = mApp;
        }

        //Create new assembly document
        private void Button1_Click(object sender, EventArgs e)
        {
            AssemblyDocument oAsm = mApp.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject, 
                mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kAssemblyDocumentObject, 
                    SystemOfMeasureEnum.kDefaultSystemOfMeasure,
                    DraftingStandardEnum.kDefault_DraftingStandard, null),
                true) as AssemblyDocument;
        }

        //Add an occurrence, prompts user to select an existing Inventor Part or Assembly
        private void Button3_Click(object sender, EventArgs e)
        {

            if (((mApp.ActiveDocument != null)))
            {

                if ((mApp.ActiveDocument.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject))
                {

                    AssemblyDocument oAsm = mApp.ActiveDocument as AssemblyDocument;

                    FileDialog oDLG = null;
                    mApp.CreateFileDialog(out oDLG);

                    //oDLG.FileName = "C:\Temp\"
                    oDLG.Filter = "Inventor Files (*.iam;*.ipt)|*.iam;*.ipt";
                    oDLG.DialogTitle = "Insert occurrence";

                    oDLG.ShowOpen();

                    if ((!string.IsNullOrEmpty(oDLG.FileName)))
                    {

                        Matrix pos = mApp.TransientGeometry.CreateMatrix();
                        var oNewOcc = oAsm.ComponentDefinition.Occurrences.Add(oDLG.FileName, pos);


                        mApp.ActiveView.Update();

                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error");
                    return;

                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error");
                return;

            }
        }

        //Transform an occurrence. Occurrence needs to be selected first through the UI
        private void Button2_Click(object sender, EventArgs e)
        {
            {

                if (((mApp.ActiveDocument != null)))
                {

                    if ((mApp.ActiveDocument.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject))
                    {

                        if ((mApp.ActiveDocument.SelectSet.Count == 1))
                        {

                            if (((mApp.ActiveDocument.SelectSet[1]) is ComponentOccurrence))
                            {

                                ComponentOccurrence oCompOccurrence = mApp.ActiveDocument.SelectSet[1] as ComponentOccurrence;

                                Matrix oTransfo = mApp.TransientGeometry.CreateMatrix();

                                if ((mAngleBox.Text.Length == 0))
                                {
                                    mAngleBox.Text = "0";
                                }

                                double angle = System.Double.Parse(mAngleBox.Text);

                                Vector trans = VectorControl1.Vector;
                                Vector axis = VectorControl2.Vector;

                                if ((axis.Length == 0))
                                {
                                    System.Windows.Forms.MessageBox.Show("Rotation Axis cannot be null", "Error");
                                    return;
                                }

                                oTransfo.SetToRotation(angle * Math.Atan(1) * 4 / 180.0, axis, oCompOccurrence.MassProperties.CenterOfMass);

                                Vector oFinalTx = oTransfo.Translation;
                                oFinalTx.AddVector(trans);

                                oTransfo.SetTranslation(oFinalTx, false);

                                Matrix oNewTransfo = oCompOccurrence.Transformation;

                                oNewTransfo.TransformBy(oTransfo);

                                oCompOccurrence.Transformation = oNewTransfo;

                                mApp.ActiveView.Update();
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show("Not an occurrence...", "Error");
                                return;

                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("A single occurrence must be selected...", "Error");
                            return;

                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error");
                        return;

                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error");
                    return;

                }
            }
        }

        #region "Lab Demo - Constraint"

        void createPart1()
        {
            // create a new part

            PartDocument oDoc = (PartDocument)mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject);
            PartComponentDefinition oDef = oDoc.ComponentDefinition;

            TransientGeometry oTG = mApp.TransientGeometry;

            // create sketch elements
            PlanarSketch oSketch = oDef.Sketches.Add(oDef.WorkPlanes[3]);
            SketchCircle oCircle = oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(0, 0), 1);

            Profile oProfile = oSketch.Profiles.AddForSolid();

            // create a cylinder feature
            ExtrudeDefinition oExtrudDef = oDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kJoinOperation);
            oExtrudDef.SetDistanceExtent(5, PartFeatureExtentDirectionEnum.kPositiveExtentDirection);
            ExtrudeFeature oExtrudeF = oDef.Features.ExtrudeFeatures.Add(oExtrudDef);

            //add an attribute to cylinder face         
            Face oFace = oExtrudeF.SideFaces[1];

            AttributeSet oAttSet = default(AttributeSet);
            Inventor.Attribute oAtt = null;
            oAttSet = oFace.AttributeSets.Add("demoAttset");
            oAtt = oAttSet.Add("demoAtt", ValueTypeEnum.kStringType, "namedEdge");
			  if(System.IO.File.Exists("c:\temp\test1.ipt") ) 
             {
			  System.IO.File.Delete("c:\temp\test1.ipt"); 
			  }

            oDoc.SaveAs("c:\\temp\\test1.ipt", false);
             
        }

        private void createPart2()
        {
            // create a new part
            PartDocument oDoc = (PartDocument)mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject);
            PartComponentDefinition oDef = oDoc.ComponentDefinition;

            TransientGeometry oTG = mApp.TransientGeometry;

            // create sketch elements
            PlanarSketch oSketch = oDef.Sketches.Add(oDef.WorkPlanes[3]);
            oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(-5, -5), oTG.CreatePoint2d(5, 5));

            SketchPoint oSketchPt = oSketch.SketchPoints.Add(oTG.CreatePoint2d(0, 0));

            Profile oProfile = oSketch.Profiles.AddForSolid();
            // create a plate with a hole feature
            ExtrudeDefinition oExtrudDef = oDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kJoinOperation);
            oExtrudDef.SetDistanceExtent(1, PartFeatureExtentDirectionEnum.kPositiveExtentDirection);
            ExtrudeFeature oExtrudeF = oDef.Features.ExtrudeFeatures.Add(oExtrudDef);

            // Create an object collection for the hole center points. 
            ObjectCollection oHoleCenters = default(ObjectCollection);
            oHoleCenters = mApp.TransientObjects.CreateObjectCollection();

            oHoleCenters.Add(oSketchPt);

            // create hole feature
            HolePlacementDefinition oHPdef = (HolePlacementDefinition)oDef.Features.HoleFeatures.CreateSketchPlacementDefinition(oHoleCenters);

            HoleFeature oHoleF = oDef.Features.HoleFeatures.AddDrilledByThroughAllExtent(oHPdef, "2",PartFeatureExtentDirectionEnum.kNegativeExtentDirection );

            Face oFace = oHoleF.SideFaces[1];
            AttributeSet oAttSet = default(AttributeSet);
            Inventor.Attribute oAtt = null;
            oAttSet = oFace.AttributeSets.Add("demoAttset");
            oAtt = oAttSet.Add("demoAtt", ValueTypeEnum.kStringType, "namedEdge");
            if(System.IO.File.Exists("c:\temp\test2.ipt") ) 
             {
			  System.IO.File.Delete("c:\temp\test2.ipt"); 
			  }


            oDoc.SaveAs("c:\\temp\\test2.ipt", false);

        }

        private void insertPartsAndMateEdges()
        {

            // create an assembly
            AssemblyDocument oAssDoc = (AssemblyDocument)mApp.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject);
            AssemblyComponentDefinition oAssDef = oAssDoc.ComponentDefinition;

            Matrix oM = mApp.TransientGeometry.CreateMatrix();

            //place the two parts
            ComponentOccurrence oOcc1 = oAssDef.Occurrences.Add("c:\\temp\\test1.ipt", oM);
            ComponentOccurrence oOcc2 = oAssDef.Occurrences.Add("c:\\temp\\test2.ipt", oM);

            // find the two faces to mate
            PartDocument oDoc1 = (PartDocument)oOcc1.Definition.Document;
            ObjectCollection oObjCollection = oDoc1.AttributeManager.FindObjects("demoAttset", "demoAtt");

            Face  oFace1 = null ;
            if (oObjCollection[1] is Face)
            {
                oFace1 = (Face)oObjCollection[1];
            }

            PartDocument oDoc2 = (PartDocument)oOcc2.Definition.Document;
            oObjCollection = oDoc2.AttributeManager.FindObjects("demoAttset", "demoAtt");

            Face oFace2 = null;
            if (oObjCollection[1] is Face)
            {
                oFace2 = (Face)oObjCollection[1];
            }

            Object tempObj;
            //create the proxy objects for the two faces
           
            oOcc1.CreateGeometryProxy(oFace1, out tempObj);
            FaceProxy oAsmProxyFace1 = (FaceProxy)tempObj; 

            oOcc2.CreateGeometryProxy(oFace2, out tempObj);
            FaceProxy oAsmProxyFace2 = (FaceProxy)tempObj; 

            // add the mate constraint
            oAssDef.Constraints.AddMateConstraint(oAsmProxyFace1, oAsmProxyFace2, 0);

        }
        private void button4_Click(object sender, EventArgs e)
        {

           // create part1
            createPart1();
            // create part2
            createPart2();
           // create assembly, place the two parts and mate two faces
            insertPartsAndMateEdges();
        }
        #endregion

        
    }
}

