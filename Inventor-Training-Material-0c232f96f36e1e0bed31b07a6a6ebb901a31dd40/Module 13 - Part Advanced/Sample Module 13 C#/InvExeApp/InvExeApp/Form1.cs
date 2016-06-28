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

namespace InvExeApp
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
            catch(Exception ex)
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


            //=======================================================
            //Service provided by Telerik (www.telerik.com)
            //Conversion powered by NRefactory.
            //Built and maintained by Todd Anglin and Telerik
            //=======================================================

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

        //*********** Declare here your public Sub routines ***********

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample
        ////
        //// Use: iFeature Insertion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void iFeatureInsertion()
	{

        PartDocument oPartDoc = (PartDocument)_InvApplication.ActiveDocument;

		PartComponentDefinition oCompDef;
        oCompDef = oPartDoc.ComponentDefinition;

		TransientGeometry oTG;
		oTG = _InvApplication.TransientGeometry;

		// Arbitrarily get the start face of the first extrusion.
		Face oFace;
		oFace = oCompDef.Features.ExtrudeFeatures[1].EndFaces[1];

		// Create the definition object for the specified ide file.


		string oideFile = OpenFile("(*.ide)|*.ide");
		// Create the definition object for the specified ide file.
		if ((string.IsNullOrEmpty(oideFile))) {
			MessageBox.Show("no ide file is  selected!");
			return;
		}
		iFeatureDefinition oiFeatDef;
		oiFeatDef = oCompDef.Features.iFeatures.CreateiFeatureDefinition(oideFile);

		// Set the iFeature input values.	
		iFeatureParameterInput oParamInput;

		foreach ( iFeatureInput oiFeatInput in oiFeatDef.iFeatureInputs) {

			switch (oiFeatInput.Name) {
				case "Sketch Plane":
					iFeatureSketchPlaneInput oPlaneInput;
                    oPlaneInput = (iFeatureSketchPlaneInput)oiFeatInput;
					oPlaneInput.PlaneInput = oFace;
					oPlaneInput.SetPosition(oTG.CreatePoint(5, 5, 0.1), oTG.CreateVector(1, 0, 0), 0);
                    break;
				case "Diameter":
                    oParamInput = (iFeatureParameterInput)oiFeatInput;
					oParamInput.Expression = ".7 in";
                    break;
				case "Height":
                    oParamInput = (iFeatureParameterInput)oiFeatInput;
					oParamInput.Expression = "10.0 in";
                    break;
				case "Radius":
                    oParamInput = (iFeatureParameterInput)oiFeatInput;
					oParamInput.Expression = ".5 in";
                    break;

			}
		}

		// Create the iFeature.
		oCompDef.Features.iFeatures.Add(oiFeatDef);

	}

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample
        ////
        //// Use: iFeature Table-Driven
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void PlaceTableDriveniFeature()
	{

		// Get the part document and component definition of the active document.
		PartDocument oPartDoc;
		oPartDoc = (PartDocument)_InvApplication.ActiveDocument;

		PartComponentDefinition oPartDef;
		oPartDef = oPartDoc.ComponentDefinition;

		// Get the selected face to use as input for the iFeature.
		Face oFace;
		oFace = oPartDoc.SelectSet[1];

		if (oFace.SurfaceType != SurfaceTypeEnum.kPlaneSurface) {
			MessageBox.Show("A planar face must be selected.");
			return;
		}

		PartFeatures oFeatures;
		oFeatures = oPartDef.Features;

		string oideFile = OpenFile("(*.ide)|*.ide");
		// Create the definition object for the specified ide file.
		if ((string.IsNullOrEmpty(oideFile))) {
			MessageBox.Show("no ide file is  selected!");
			return;
		}
		iFeatureDefinition oiFeatureDef;
		oiFeatureDef = oPartDef.Features.iFeatures.CreateiFeatureDefinition(oideFile);

		// Set the input, which in this case is only the sketch plane
		// since the other input comes from the table.  The parameter input
		// should not be available at this point since it can't be changed
		// and is controlled by the table.
		//
		// When an existing table driven iFeature is accessed then this should
		// include the parameters so the programmer has access to the corresponding
		// reference parameter that's created.
		bool bFoundPlaneInput;
		bFoundPlaneInput = false;
	 
		foreach ( iFeatureInput oInput in oiFeatureDef.iFeatureInputs) {

			switch (oInput.Name) {
				case "Profile Plane1":
					iFeatureSketchPlaneInput oPlaneInput;
					oPlaneInput = (iFeatureSketchPlaneInput)oInput;
					oPlaneInput.PlaneInput = oFace;
					bFoundPlaneInput = true;
                    break;
			}
		}

		if (!bFoundPlaneInput) {
			MessageBox.Show("The ide file does not contain an iFeature input named \"Profile Plane1\".");
			return;
		}

		//** Look through the table to find the row where "Size" is "4.5".
		iFeatureTable oTable;
		oTable = oiFeatureDef.iFeatureTable;

		// Find the "Size" column.
	 
		bool bFoundSize;
		bFoundSize = false;
        iFeatureTableColumn oSelectColumn = null;
		foreach (iFeatureTableColumn  oColumn in oTable.iFeatureTableColumns) {
			if (oColumn.DisplayHeading == "Size") {
				bFoundSize = true;
                oSelectColumn = oColumn;
				break; // TODO: might not be correct. Was : Exit For
			}
		}

		if (!bFoundSize) {
			MessageBox.Show("The column \"Size\" was not found.");
			return;
		}

		// Find the row in the "Size" column with the value of "4.5"
        iFeatureTableCell oselectCell = null;
		bFoundSize = false;
        foreach (iFeatureTableCell oCell in oSelectColumn)
        {
			if (oCell.Value == "4.5") {
				bFoundSize = true;
                oselectCell = oCell;
				break; // TODO: might not be correct. Was : Exit For
			}
		}

		if (!bFoundSize) {
            MessageBox.Show("The cell with value \"4.5\" was not found.");
			return;
		}

		// Set this row as the active row.
        oiFeatureDef.ActiveTableRow = oselectCell.Row;

		// Create the iFeature.
		iFeature oiFeature;
		oiFeature = oFeatures.iFeatures.Add(oiFeatureDef);

	}

        public void DerivedPartExample()
        {

            PartDocument oPartDoc;
            oPartDoc = (PartDocument)_InvApplication.ActiveDocument;

            PartComponentDefinition oCompDef;
            oCompDef = oPartDoc.ComponentDefinition;

            DerivedPartComponents oDerivedPartComps;
            oDerivedPartComps = oCompDef.ReferenceComponents.DerivedPartComponents;

            //derive from the sample Part1.ipt

            //1. Create the definition corresponding to the desired 
            // type of derived part or assembly
            DerivedPartUniformScaleDef oDerivedPartDef;
            oDerivedPartDef = oDerivedPartComps.CreateUniformScaleDef("C:\\Temp\\Part1.ipt");
            //2. Set the various inputs.
            oDerivedPartDef.ScaleFactor = 0.75;

            // 3. Use the definition to create the derived component.
            DerivedPartComponent oDerivedComp;
            oDerivedComp = oDerivedPartComps.Add((DerivedPartDefinition)oDerivedPartDef);

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Use: Derive part with DeriveStyle control
        ////
        ////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CreateDerivedPart()
	{

		PartDocument oPartDoc;
		oPartDoc = (PartDocument)_InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), true);

		DerivedPartComponents oDerivedPartComps;
		oDerivedPartComps = oPartDoc.ComponentDefinition.ReferenceComponents.DerivedPartComponents;

		// assume there is one part named  PartRef.ipt in C:\temp\
		DerivedPartUniformScaleDef oDerivedPartDef;
		oDerivedPartDef = oDerivedPartComps.CreateUniformScaleDef("C:\\Temp\\PartRef.ipt");
		oDerivedPartDef.ScaleFactor = 0.75;

		oDerivedPartDef.IncludeAll();

		// other properties
		//oDerivedPartDef.ExcludeAll
		//oDerivedPartDef.IncludeAlliMateDefinitions
		//oDerivedPartDef.IncludeAllSurfaces

		 
		foreach ( DerivedPartEntity oDerivedEntity in oDerivedPartDef.Parameters) {

            if ((oDerivedEntity.ReferencedEntity is Inventor.Parameter))
            {
				Inventor.Parameter oParameter;
				oParameter = oDerivedEntity.ReferencedEntity;
				Debug.Print("Derived Parameter: " + oParameter.Name);

				//oDerivedEntity.IncludeEntity = False
			}

		}

		oDerivedPartDef.DeriveStyle = DerivedComponentStyleEnum.kDeriveAsSingleBodyNoSeams;

		DerivedPartComponent oDerivedComp;
        oDerivedComp = oDerivedPartComps.Add((DerivedPartDefinition)oDerivedPartDef);

	}

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample
        ////
        //// Use: Small utility method. Dumps the contents of the iTable
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void DumpiTable()
        {

            PartDocument oPartDoc;
            oPartDoc = (PartDocument)_InvApplication.ActiveDocument;

            if ((!oPartDoc.ComponentDefinition.IsiPartFactory)) return;


            iPartFactory oFactory;
            oFactory = oPartDoc.ComponentDefinition.iPartFactory;

            Microsoft.Office.Interop.Excel.Worksheet oWS;
            oWS = oFactory.ExcelWorkSheet;

            int rowIdx;
            int colIdx;

            for (rowIdx = 1; rowIdx <= oFactory.TableRows.Count + 1; rowIdx++)
            {

                Debug.Print("-------------------------------------------------");

                for (colIdx = 1; colIdx <= oFactory.TableColumns.Count; colIdx++)
                {

                    Debug.Print("Cell[" + rowIdx + "," + colIdx + "]: " + oWS.Cells[rowIdx, colIdx].Text);

                }
            }

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////
        ////
        //// Use: Lab solution. Creation of an iPart from scratch
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CreateiPart()
        {

            PartDocument oPartDoc;
            oPartDoc = (PartDocument)_InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), true);

            oPartDoc.SaveAs("c:\\Temp\\APIiPart.ipt", false);

            TransientGeometry oTG;
            oTG = _InvApplication.TransientGeometry;

            PartComponentDefinition oCompDef;
            oCompDef = oPartDoc.ComponentDefinition;

            Point2d[] oPoints = new Point2d[3];

            oPoints[0] = oTG.CreatePoint2d(0, 0);
            oPoints[1] = oTG.CreatePoint2d(5, 0);
            oPoints[2] = oTG.CreatePoint2d(5, 5);
            oPoints[3] = oTG.CreatePoint2d(0, 5);

            PlanarSketch oSketch;
            oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes[1]);

            SketchLine[] oLines = new SketchLine[3];

            oLines[0] = oSketch.SketchLines.AddByTwoPoints(oPoints[0], oPoints[1]);
            oLines[1] = oSketch.SketchLines.AddByTwoPoints(oLines[0].EndSketchPoint, oPoints[2]);
            oLines[2] = oSketch.SketchLines.AddByTwoPoints(oLines[1].EndSketchPoint, oPoints[3]);
            oLines[3] = oSketch.SketchLines.AddByTwoPoints(oLines[2].EndSketchPoint, oLines[0].StartSketchPoint);

            Profile oProfile;
            oProfile = oSketch.Profiles.AddForSolid();

            ExtrudeFeature oExtrude;
            oExtrude = oCompDef.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 15, PartFeatureExtentDirectionEnum.kPositiveExtentDirection, PartFeatureOperationEnum.kNewBodyOperation, 0);

            oExtrude.FeatureDimensions[1].Parameter.Name = "Length";
            oExtrude.FeatureDimensions[2].Parameter.Name = "TaperAngle";

            iPartFactory oFactory;
            oFactory = oCompDef.CreateFactory();

            Microsoft.Office.Interop.Excel.Worksheet oWS;
            oWS = oFactory.ExcelWorkSheet;

            oWS.Cells[1, 1] = "Member<defaultRow>1</defaultRow><filename></filename>";
            oWS.Cells[1, 2] = "Part Number [Project]";
            oWS.Cells[1, 3] = "Length<free>150 mm</free>";
            oWS.Cells[1, 4] = "TaperAngle";

            oWS.Cells[2, 1] = "APIiPart-01";
            oWS.Cells[2, 2] = "APIiPart-01";
            oWS.Cells[2, 3] = "150 mm";
            oWS.Cells[2, 4] = "0 deg";

            oWS.Cells[3, 1] = "APIiPart-02";
            oWS.Cells[3, 2] = "APIiPart-02";
            oWS.Cells[3, 3] = "100 mm";
            oWS.Cells[3, 4] = "5 deg";

            oWS.Cells[4, 1] = "APIiPart-03";
            oWS.Cells[4, 2] = "APIiPart-03";
            oWS.Cells[4, 3] = "50 mm";
            oWS.Cells[4, 4] = "10 deg";

            Microsoft.Office.Interop.Excel.Workbook oWB;
            oWB = oWS.Parent;

            oWB.Save();
            oWB.Close();

            oPartDoc.Update();
            oPartDoc.Save();

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample
        ////
        //// Use: Add a new row to the factory
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void AddRow()
        {

            PartDocument oFactoryDoc;
            oFactoryDoc = (PartDocument)_InvApplication.ActiveDocument;

            if ((oFactoryDoc.ComponentDefinition.iPartFactory == null))
            {
                MessageBox.Show ("Not an iPart document...");
                return;
            }

            iPartFactory oFactory;
            oFactory = oFactoryDoc.ComponentDefinition.iPartFactory;

            Microsoft.Office.Interop.Excel.Worksheet oWorksheet;
            oWorksheet = oFactory.ExcelWorkSheet;

            int newRowIndex;
            newRowIndex = oFactory.TableRows.Count + 1;

            //Write new row values
            oWorksheet.Cells[newRowIndex + 1, 1] = "Factory-0" + newRowIndex.ToString();
            oWorksheet.Cells[newRowIndex + 1, 2] = "Factory-0" + newRowIndex.ToString();
            oWorksheet.Cells[newRowIndex + 1, 3] = "15 mm";
            oWorksheet.Cells[newRowIndex + 1, 4] = "15 mm";
            oWorksheet.Cells[newRowIndex + 1, 5] = "5 mm";

            Microsoft.Office.Interop.Excel.Workbook oWorkbook;
            oWorkbook = oWorksheet.Parent;

            Microsoft.Office.Interop.Excel.Application oXlsApp;
            oXlsApp = oWorkbook.Parent;

            oWorkbook.Save();
            oWorkbook.Close();

            oWorksheet = null;
            oWorkbook = null;

            //Set our new row as default
            oFactory.DefaultRow = oFactory.TableRows[newRowIndex];
            oFactoryDoc.Save();

            //oFactoryDoc.close

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Use: Multi Solid Bodies
        ////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CreateMultiBodies()
        {

            PartDocument oPartDoc;
            oPartDoc = (PartDocument)_InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), true);

            PartComponentDefinition oPartDef;
            oPartDef = oPartDoc.ComponentDefinition;

            // Create a sketch.
            PlanarSketch oSketch;
            oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes[3]);

            TransientGeometry oTG;
            oTG = _InvApplication.TransientGeometry;

            // Draw a rectangle and extrude it to create a new body.
            // *** The kNewBodyOperation type of operation is new in Inventor 2010.
            oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(-3, -2), oTG.CreatePoint2d(3, 2));

            Profile oProfile;
            oProfile = oSketch.Profiles.AddForSolid();

            ExtrudeFeature oExtrude;
            oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 2, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, PartFeatureOperationEnum.kNewBodyOperation);

            RenderStyle oRenderStyle = oPartDoc.RenderStyles["Glass (Limo Tint)"];
            oPartDef.SurfaceBodies[1].SetRenderStyle(StyleSourceTypeEnum.kOverrideRenderStyle, oRenderStyle);

            // Create a second sketch.
            oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes[3]);

            // *** The kNewBodyOperation type of operation is new in Inventor 2010.
            // Draw a rectangle and extrude it to create a new body.
            oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(-2.5, -1.5), oTG.CreatePoint2d(2.5, 1.5));

            oProfile = oSketch.Profiles.AddForSolid();
            oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 1.5, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, PartFeatureOperationEnum.kNewBodyOperation, "-5 deg");


        }



        /// <summary>
        /// Create move feature 
        /// assume a part document with one solid is opened
        /// </summary>
        /// <remarks></remarks>
        public void CreateMoveFeature()
        {

            PartDocument oPartDoc;
            oPartDoc = (PartDocument)_InvApplication.ActiveDocument;

            PartComponentDefinition oPartDef;
            oPartDef = oPartDoc.ComponentDefinition;

            //collect the faces to move
            ObjectCollection oSolidColl;
            oSolidColl = _InvApplication.TransientObjects.CreateObjectCollection();

            //add the first solid
            oSolidColl.Add(oPartDef.SurfaceBodies[1]);

            //create move feature definition
            MoveDefinition oMoveFDef;
            oMoveFDef = oPartDef.Features.MoveFeatures.CreateMoveDefinition(oSolidColl);

            // Define a rotate, move, and free drag.
            oMoveFDef.AddRotateAboutAxis(oPartDef.WorkAxes[3], true, 3.14159265358979 / 4);
            oMoveFDef.AddMoveAlongRay(oPartDef.WorkAxes[1], true, 3);
            oMoveFDef.AddFreeDrag(0.5, 6, 3);

            //create the move feature
            MoveFeature oMoveF;
            oMoveF = oPartDef.Features.MoveFeatures.Add(oMoveFDef);

        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample
        ////
        //// Use: Combines bodies created by previous sample
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void CombineBodies()
        {

            PartDocument oPartDoc;
            oPartDoc = (PartDocument)_InvApplication.ActiveDocument;

            PartComponentDefinition oPartDef;
            oPartDef = oPartDoc.ComponentDefinition;

            //add first body
            SurfaceBody oBaseBody;
            oBaseBody = oPartDef.SurfaceBodies[1];

            ObjectCollection oToolBodies;
            oToolBodies = _InvApplication.TransientObjects.CreateObjectCollection();

            //add second body
            oToolBodies.Add(oPartDef.SurfaceBodies[2]);

            CombineFeature oCombineFeature;
            oCombineFeature = oPartDef.Features.CombineFeatures.Add(oBaseBody, oToolBodies, PartFeatureOperationEnum.kCutOperation, false);

        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //// Sample
        ////
        //// Use: Demonstrates use of "Feature.SetAffectedBodies" method
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void FeatureMultibody()
	{

		PartDocument oPartDoc;
		oPartDoc = (PartDocument)_InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), true);

		PartComponentDefinition oPartDef;
		oPartDef = oPartDoc.ComponentDefinition;

		// Create a sketch.
		PlanarSketch oSketch;
		oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes[3]);

		TransientGeometry oTG;
		oTG = _InvApplication.TransientGeometry;

		// *** The kNewBodyOperation type of operation is new in Inventor 2010.
		// Draw a rectangle and oExtrude it to create a new body.
		oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(-4, -2), oTG.CreatePoint2d(-3, 2));

		Profile oProfile;
		oProfile = oSketch.Profiles.AddForSolid();

		ExtrudeFeature oExtrude;
		oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 2, PartFeatureExtentDirectionEnum.kSymmetricExtentDirection, PartFeatureOperationEnum.kNewBodyOperation);

		// Create a second sketch.
		oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes[3]);

		// *** The kNewBodyOperation type of operation is new in Inventor 2010.
		// Draw a rectangle and oExtrude it to create a new body.
		oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(-2, -2), oTG.CreatePoint2d(-1, 2));

		oProfile = oSketch.Profiles.AddForSolid();
		oExtrude = oPartDef.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 2, PartFeatureExtentDirectionEnum.kSymmetricExtentDirection, PartFeatureOperationEnum.kNewBodyOperation);

		// Create a third sketch.
		oSketch = oPartDef.Sketches.Add(oPartDef.WorkPlanes[1]);

		// Draw a circle and oExtrude it to cut through the existing bodies.
		oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(0, 0), 0.5);

		oProfile = oSketch.Profiles.AddForSolid();
		oExtrude = oPartDef.Features.ExtrudeFeatures.AddByThroughAllExtent(oProfile, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, PartFeatureOperationEnum.kCutOperation);

		// *** The SurfaceBodies property on the feature
		// *** and the SetAffectedBodies method are new in Inventor 2010.
		// Check to see if the feature affected all bodies.
		if (oExtrude.SurfaceBodies.Count != oPartDef.SurfaceBodies.Count) {
			ObjectCollection bodies;
			bodies = _InvApplication.TransientObjects.CreateObjectCollection();
			 
			foreach ( SurfaceBody body in oPartDef.SurfaceBodies) {
				bodies.Add(body);
			}
			oExtrude.SetAffectedBodies(bodies);
		}

	}



    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Built and maintained by Todd Anglin and Telerik
    //=======================================================

}
