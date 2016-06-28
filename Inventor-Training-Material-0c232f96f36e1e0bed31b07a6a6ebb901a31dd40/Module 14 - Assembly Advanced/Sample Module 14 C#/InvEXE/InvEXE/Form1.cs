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
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Reflection;

namespace InvEXE
{
    public partial class Form1 : Form
    {
        private Inventor.Application _InvApplication;
        Macros _macros;
        public Form1()
        {
            InitializeComponent();
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

        private void ComboBoxMacros_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

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
    }

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
        /// Add iMate definitions using AddMateiMateDefinition and AddInsertiMateDefinition.
        /// </summary>
        /// <remarks></remarks>

        public void CreateiMateDefinition()
        {
         // Create a new part document, using the default part template.
         PartDocument oPartDoc  = (PartDocument)_InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject));

         // Set a reference to the component definition.
         PartComponentDefinition oCompDef = default(PartComponentDefinition);
         oCompDef = oPartDoc.ComponentDefinition;

         // Create a new sketch on the X-Y work plane.
         PlanarSketch oSketch = default(PlanarSketch);
         oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes[3]);

         // Set a reference to the transient geometry object.
         TransientGeometry oTransGeom = default(TransientGeometry);
         oTransGeom = _InvApplication.TransientGeometry;

         // Draw a 4cm x 3cm rectangle with the corner at (0,0)
         SketchEntitiesEnumerator oRectangleLines = default(SketchEntitiesEnumerator);
         oRectangleLines = oSketch.SketchLines.AddAsTwoPointRectangle(oTransGeom.CreatePoint2d(0, 0), oTransGeom.CreatePoint2d(4, 3));

         // Create a profile.
         Profile oProfile = default(Profile);
         oProfile = oSketch.Profiles.AddForSolid();

         // Create a base extrusion 1cm thick.
         ExtrudeDefinition oExtrudeDef = default(ExtrudeDefinition);
         oExtrudeDef = oCompDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kNewBodyOperation);
         oExtrudeDef.SetDistanceExtent(1, PartFeatureExtentDirectionEnum.kNegativeExtentDirection);
         ExtrudeFeature oExtrude1 = default(ExtrudeFeature);
         oExtrude1 = oCompDef.Features.ExtrudeFeatures.Add(oExtrudeDef);

         // Get the top face of the extrusion to use for creating the new sketch.
         Face oFrontFace = default(Face);
         oFrontFace = oExtrude1.StartFaces[1];

         // Create a new sketch on this face, but use the method that allows you to
         // control the orientation and orgin of the new sketch.
         oSketch = oCompDef.Sketches.AddWithOrientation(oFrontFace, oCompDef.WorkAxes[1], true, true, oCompDef.WorkPoints[1]);

         // Create a sketch circle with the center at (2, 1.5).
         SketchCircle oCircle = default(SketchCircle);
         oCircle = oSketch.SketchCircles.AddByCenterRadius(oTransGeom.CreatePoint2d(2, 1.5), 0.5);

         // Create a profile.
         oProfile = oSketch.Profiles.AddForSolid();

         // Create the second extrude (a hole).
         ExtrudeFeature oExtrude2 = default(ExtrudeFeature);
         oExtrude2 = oCompDef.Features.ExtrudeFeatures.AddByThroughAllExtent(oProfile, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, PartFeatureOperationEnum.kCutOperation);

         // Create a mate iMateDefinition on a side face of the first extrude.
         MateiMateDefinition oMateiMateDefinition = default(MateiMateDefinition);
         oMateiMateDefinition = oCompDef.iMateDefinitions.AddMateiMateDefinition(oExtrude1.SideFaces[1], 0,InferredTypeEnum.kNoInference  ,null  , "MateA");

         // Create a match list of names to use for the next iMateDefinition.
         string[] strMatchList = new string[3];
         strMatchList[0] = "InsertA";
         strMatchList[1] = "InsertB";
         strMatchList[2] = "InsertC";

         // Create an insert iMateDefinition on the cylindrical face of the second extrude.
         InsertiMateDefinition oInsertiMateDefinition = default(InsertiMateDefinition);
         oInsertiMateDefinition = oCompDef.iMateDefinitions.AddInsertiMateDefinition(oExtrude2.SideFaces[1], false, 0,null , "InsertA", strMatchList);
        }

        public void iMateResultCreation()
        {
         // Get the component definition of the currently open assembly.
         // This will fail if an assembly document is not open.
         AssemblyDocument oAssDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
         AssemblyComponentDefinition oAsmCompDef  = oAssDoc.ComponentDefinition;

         // Create a new matrix object.  It will be initialized to an identity matrix.
         Matrix oMatrix = default(Matrix);
         oMatrix = _InvApplication.TransientGeometry.CreateMatrix();

         // Place the first occurrence.
         ComponentOccurrence oOcc1 = default(ComponentOccurrence);
         oOcc1 = oAsmCompDef.Occurrences.Add("C:\\Temp\\iMatePart.ipt", oMatrix);

         // Place the second occurrence, but adjust the matrix slightly so they're
         // not right on top of each other.
         oMatrix.Cell[1, 4] = 10;
         ComponentOccurrence oOcc2 = default(ComponentOccurrence);
         oOcc2 = oAsmCompDef.Occurrences.Add("C:\\Temp\\iMatePart.ipt", oMatrix);

         // Look through the iMateDefinitions defined for the first occurrence
         // and find the one named "iMate:1".  This loop demonstrates using the
         // Count and Item properties of the iMateDefinitions object.
         int i = 0;
         iMateDefinition oiMateDef1 = null;  
         for (i = 1; i <= oOcc1.iMateDefinitions.Count; i++) {
          if (oOcc1.iMateDefinitions[i].Name == "iMate:1") {
           oiMateDef1 = oOcc1.iMateDefinitions[i];
           break;  
          }
         }

         if (oiMateDef1 == null) {
          MessageBox.Show ("An iMate definition named \"iMate:1\" does not exist in " + oOcc1.Name);
          return;
         }

         // Look through the iMateDefinitions defined for the second occurrence
         // and find the one named "iMate:1".  This loop demonstrates using the
         // For Each method of iterating through a collection.
         iMateDefinition oiMateDef2 = null; ;
         bool bFoundDefinition = false;
         foreach ( iMateDefinition oiMateDef in oOcc2.iMateDefinitions) {
             if (oiMateDef.Name == "iMate:1")
             {
           bFoundDefinition = true;
           oiMateDef2 = oiMateDef;
           break; // TODO: might not be correct. Was : Exit For
          }
         }

         if (!bFoundDefinition) {
             MessageBox.Show("An iMate definition named \"iMate:1\" does not exist in " + oOcc2.Name);
          return;
         }

         // Create an iMate result using the two definitions.
         iMateResult oiMateResult  = oAsmCompDef.iMateResults.AddByTwoiMates(oiMateDef1, oiMateDef2);

        }



        public void AssemblyFeature()
        {
            AssemblyDocument oAssDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oAsmDef = oAssDoc.ComponentDefinition;

            // Create a sketch on the XY workplane.
            PlanarSketch oSketch = default(PlanarSketch);
            oSketch = oAsmDef.Sketches.Add(oAsmDef.WorkPlanes[3]);

            TransientGeometry oTG = default(TransientGeometry);
            oTG = _InvApplication.TransientGeometry;

            // Draw a rectangle.
            oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(0.1, 0.1), oTG.CreatePoint2d(1, 0.5));
            // Create a profile
            Profile oProfile = default(Profile);
            oProfile = oSketch.Profiles.AddForSolid();

            // Create the extrusion.
            ExtrudeDefinition oExtrudeDef = default(ExtrudeDefinition);
            oExtrudeDef = oAsmDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kCutOperation);
            oExtrudeDef.SetDistanceExtent("2 cm", PartFeatureExtentDirectionEnum.kSymmetricExtentDirection);

            oAsmDef.Features.ExtrudeFeatures.Add(oExtrudeDef);

        }


        public void AddAssemblyBrowserFolder()
        {
        AssemblyDocument oDoc = (AssemblyDocument)_InvApplication.ActiveDocument;

         AssemblyComponentDefinition oDef =  oDoc.ComponentDefinition;

         BrowserPane oPane = default(BrowserPane);
         oPane = oDoc.BrowserPanes.ActivePane;

         // Create an object collection
         ObjectCollection oOccurrenceNodes = default(ObjectCollection);
         oOccurrenceNodes = _InvApplication.TransientObjects.CreateObjectCollection();
 
         foreach ( ComponentOccurrence oOcc in oDef.Occurrences) {
          // Get the node associated with this occurrence.
          BrowserNode oNode = default(BrowserNode);
          oNode = oPane.GetBrowserNodeFromObject(oOcc);

          // Add the node to the collection.
          oOccurrenceNodes.Add(oNode);
         }

         // Add the folder to the browser and specify the nodes to be grouped within it.
         BrowserFolder oFolder = default(BrowserFolder);
         oFolder = oPane.AddBrowserFolder("My Occurrences Folder", oOccurrenceNodes);

        }



        public void BomAccess()
        {
            AssemblyDocument oDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oDef = oDoc.ComponentDefinition;

            BOM oBOM = default(BOM);
            oBOM = oDef.BOM;

            oBOM.StructuredViewEnabled = true;

            BOMView oBomView =  oBOM.BOMViews["Structured"];

            int  rowIdx = 0;

            for (rowIdx = 1; rowIdx <= oBomView.BOMRows.Count; rowIdx++)
            {
                BOMRow oRow =  oBomView.BOMRows[rowIdx];

                Debug.Print("ItemNumber: " + oRow.ItemNumber + " TotalQuantity = " + oRow.TotalQuantity);

                ComponentDefinition oCompDef =  oRow.ComponentDefinitions[1];

                PropertySet oDesignPropSet = default(PropertySet);
                oDesignPropSet = oCompDef.Document.PropertySets("Design Tracking Properties");

            }

        }


        public void ExportBOM()
        {
            AssemblyDocument oDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oDef = oDoc.ComponentDefinition;

            BOM oBOM = oDef.BOM;

            // Set the structured view to 'all levels'
            oBOM.StructuredViewFirstLevelOnly = false;

            // Make sure that the structured view is enabled.
            oBOM.StructuredViewEnabled = true;

            // Set a reference to the "Structured" BOMView
            BOMView oStructuredBOMView = default(BOMView);
            oStructuredBOMView = oBOM.BOMViews["Structured"];

            // Export the BOM view to an Excel file
            oStructuredBOMView.Export("C:\\Temp\\BOM-StructuredAllLevels.xls", FileFormatEnum.kMicrosoftExcelFormat);

        }


        public void AddiAssemblyOccurrence()
        {
            AssemblyDocument oDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oDef = oDoc.ComponentDefinition;

            ComponentOccurrences oOccurrences   = oDef.Occurrences;

            TransientGeometry oTG   = _InvApplication.TransientGeometry;

            Matrix oPos = default(Matrix);
            oPos = oTG.CreateMatrix();

            ComponentOccurrence oNewOcc ;

            //Row specified either by a Long (row index), String (MemberName), or iAssemblyTableRow.
            oNewOcc = oOccurrences.AddiAssemblyMember("C:\\MyiAsm.iam", oPos, 1);
            oNewOcc = oOccurrences.AddiAssemblyMember("C:\\MyiAsm.iam", oPos, "MemberName");

        }



        public void DesignViewSample()
        {
            AssemblyDocument oDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oAsmDef = oDoc.ComponentDefinition;


            DesignViewRepresentations oDesignViewReps  = oAsmDef.RepresentationsManager.DesignViewRepresentations;

            // Use ComponentOccurrence functionality to set the state, (visibility, color, etc.).  
            // When the design view is created it will capture the current state of the assembly.

            DesignViewRepresentation oDesignViewRep = default(DesignViewRepresentation);
            oDesignViewRep = oDesignViewReps.Add("Test");

            // Activate the master design view.
            oDesignViewReps["Master"].Activate();

        }


        public void PositionalRepSample()
        {

            AssemblyDocument oAsmDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oAsmDef = oAsmDoc.ComponentDefinition;

            PositionalRepresentations oPositionalReps = default(PositionalRepresentations);
            oPositionalReps = oAsmDef.RepresentationsManager.PositionalRepresentations;

            // Create a new position representation.
            PositionalRepresentation oPosRep = default(PositionalRepresentation);
            oPosRep = oPositionalReps.Add("New Test");

            // Get a constraint and override it's value.
            AssemblyConstraint oConstraint = default(AssemblyConstraint);
            oConstraint = oAsmDoc.ComponentDefinition.Constraints[1];
            oPosRep.SetConstraintValueOverride(oConstraint, "1 in");

        }


        public void LevelOfDetail()
        {
            AssemblyDocument oAsmDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oAsmDef = oAsmDoc.ComponentDefinition;

            LevelOfDetailRepresentations oLODReps = default(LevelOfDetailRepresentations);
            oLODReps = oAsmDef.RepresentationsManager.LevelOfDetailRepresentations;

            // Create a new level of detail.
            LevelOfDetailRepresentation oLODRep = default(LevelOfDetailRepresentation);
            oLODRep = oLODReps.Add("My LOD");

            // Suppress an occurrence.
            oAsmDef.Occurrences[1].Suppress();

            // Save the document, which is really saving the LOD.
            _InvApplication.ActiveDocument.Save();

            // Activate the master LOD.
            oAsmDef.RepresentationsManager.LevelOfDetailRepresentations["Master"].Activate();


        }


        public void BOMfromLoD()
        {

            AssemblyDocument oAsmDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oAsmDef = oAsmDoc.ComponentDefinition;

            AssemblyDocument oAsmDocMasterLOD = (AssemblyDocument)_InvApplication.Documents.Open(oAsmDoc.File.FullFileName, false);

            //Obtains BOM only from Master LOD
            BOM oBOM = default(BOM);
            oBOM = oAsmDocMasterLOD.ComponentDefinition.BOM;

            //From here you can operate on the BOM object...
            //Following lines of code are examples only

            oBOM.StructuredViewFirstLevelOnly = true;
            oBOM.StructuredViewEnabled = true;

        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample 
        ////
        //// Use: Copy & Replace assembly references (1st Level refs only)
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void ReplaceReference()
        {
          AssemblyDocument oAsmDoc = (AssemblyDocument)_InvApplication.ActiveDocument;
            AssemblyComponentDefinition oAsmDef = oAsmDoc.ComponentDefinition;

         Document oNewRefDoc = default(Document);
         string filename = null;


         foreach (FileDescriptor oFileDesc in oAsmDoc.File.ReferencedFileDescriptors)
         {
          oNewRefDoc = oFileDesc.ReferencedFile.AvailableDocuments(1);

          filename = "C:\\Temp\\Copy-" + oNewRefDoc.DisplayName;

          oNewRefDoc.SaveAs(filename, true);

          oFileDesc.ReplaceReference(filename);

         }

         filename = "C:\\Temp\\Copy-" + oAsmDoc.DisplayName;
         oAsmDoc.SaveAs(filename, true);

         oAsmDoc.Close(true);

        }

        #region "Apprentice demo"


        Inventor.ApprenticeServerComponent mApprenticeApp = new ApprenticeServerComponent();
        private void SaveWithApprentice()
        {
            string NewFolder = "C:\\Temp\\";
            string AsmFullFilename = "C:\\Program Files\\Autodesk\\Inventor 2013\\Samples\\Models\\Tube & Pipe\\Tank\\Tank.iam";

            Inventor.ApprenticeServerDocument oApprenticeDoc = mApprenticeApp.Open(AsmFullFilename);

            SaveRec(ref NewFolder, ref oApprenticeDoc);
            mApprenticeApp.FileSaveAs.ExecuteSaveCopyAs();
            oApprenticeDoc.Close();

        }

        private string newFileName(string FullFileName)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(FullFileName);
            return fileInfo.Name.Substring(0, fileInfo.Name.Length - 4) + "_new" + fileInfo.Extension;
        }


        private void SaveRec(ref string NewFolder, ref ApprenticeServerDocument oApprenticeDoc)
        {
            string NewFullFilename = NewFolder + newFileName(oApprenticeDoc.FullFileName);

            try
            {
                mApprenticeApp.FileSaveAs.AddFileToSave(oApprenticeDoc, NewFullFilename);
                ApprenticeServerDocument oRefDoc;
                for (int i = 1; i <= oApprenticeDoc.ReferencedDocuments.Count;i++ )
                {
                    oRefDoc = oApprenticeDoc.ReferencedDocuments[i];
                    SaveRec(ref NewFolder, ref oRefDoc);
                }

            }
            catch
            {
                //Content Center Parts will fail
            }
        }

        #endregion


    }
}
