// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Linq;
using System;
using System.Collections;
using System.Xml.Linq;
using System.Windows.Forms;
// End of VB project level imports

using Inventor;
using System.Reflection;


namespace InvExeApp
{
	public partial class Form1
	{
		
		Macros _macros;
		
		public Form1(Inventor.Application oApp)
		{
			
			// This call is required by the Windows Form Designer.
			InitializeComponent();
			
			// Add any initialization after the InitializeComponent() call.
			_macros = new Macros(oApp);
			
			MemberInfo[] methods = _macros.GetType().GetMembers();
			
			foreach (MemberInfo member in methods)
			{
				if (member.DeclaringType.Name == "Macros" && member.MemberType == MemberTypes.Method)
				{
					ComboBoxMacros.Items.Add(member.Name);
				}
			}
			
			if (ComboBoxMacros.Items.Count > 0)
			{
				ComboBoxMacros.SelectedIndex = 0;
				Button1.Enabled = true;
			}
			
		}
		
		public void Button1_Click(System.Object sender, System.EventArgs e)
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
			string returnValue;
			
			string filename = "";
			
			System.Windows.Forms.OpenFileDialog ofDlg = new System.Windows.Forms.OpenFileDialog();
			
			string user = System.Windows.Forms.SystemInformation.UserName;
			
			ofDlg.Title = "Open File";
			ofDlg.InitialDirectory = "C:\\Documents and Settings\\" + user + "\\Desktop\\";
			
			ofDlg.Filter = StrFilter; //Example: "Inventor files (*.ipt; *.iam; *.idw)|*.ipt;*.iam;*.idw"
			ofDlg.FilterIndex = 1;
			ofDlg.RestoreDirectory = true;
			
			if (ofDlg.ShowDialog() == DialogResult.OK)
			{
				filename = ofDlg.FileName;
			}
			
			returnValue = filename;
			
			return returnValue;
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use:
		//// [Sheet Metal Style Creation Example (Visual Basic)]
		//// This sample illustrates creating a new sheet metal style.
		//// It uses a sample bend table delivered with Inventor. You can
		//// edit the path below to reference any existing bend table.
		//// To use the sample make sure a bend table is available at the
		//// specified path, open a sheet metal document, and run the sample.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void CreateSheetMetalStyle()
		{
			
			// Set a reference to the sheet metal document.
			// This assumes a part document is active.
			PartDocument oPartDoc;
			oPartDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			// Make sure the document is a sheet metal document.
			if (oPartDoc.SubType != "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}")
			{
				MessageBox.Show("A sheet metal document must be open.");
				return;
			}
			
			// Get the sheet metal component definition. Because this is a part document whose
			// sub type is sheet metal, the document will return a SheetMetalComponentDefinition
			// instead of a PartComponentDefinition.
			SheetMetalComponentDefinition oSheetMetalCompDef;
			oSheetMetalCompDef =  (Inventor.SheetMetalComponentDefinition) (oPartDoc.ComponentDefinition);
			
			// Copy a sheet metal style to create a new one. There will always be at least
			// one style in a document. This sample uses the first style, which is the default.
			SheetMetalStyle oStyle;
			
			try
			{
				oStyle = oSheetMetalCompDef.SheetMetalStyles.Copy(
				oSheetMetalCompDef.SheetMetalStyles[1],
				"Custom Style");
			}
			catch
			{
				MessageBox.Show("Custom Style already exists :(");
				return;
			}
			
			// Get the name of the parameter used for the thickness. We need the actual name
			// to use in expressions to set the other values. It's best to get the name rather
			// than hard code it because the name changes with various languages and the user
			// can change the name in the Parameters dialog.
			
			// This gets the name of the thickness from the component definition.
			string sThicknessName;
			sThicknessName = oSheetMetalCompDef.Thickness.Name;
			
			// Set the various values associated with the style.
			oStyle.BendRadius = sThicknessName + " * 1.5";
			oStyle.BendReliefWidth = sThicknessName + " / 2";
			oStyle.BendReliefDepth = sThicknessName + " * 1.5";
			oStyle.CornerReliefSize = sThicknessName + " * 2.0";
			oStyle.MinimumRemnant = sThicknessName + " * 2.0";
			
			oStyle.BendReliefShape = BendReliefShapeEnum.kRoundBendReliefShape;
			oStyle.BendTransition = BendTransitionEnum.kArcBendTransition;
			oStyle.CornerReliefShape = CornerReliefShapeEnum.kRoundCornerReliefShape;
			
			// Add a linear unfold method.  Unfold methods are now separate
			// from sheet metal styles.
			try
			{
				oSheetMetalCompDef.UnfoldMethods.AddLinearUnfoldMethod(
				"Linear Sample",
				"0.43");
			}
			catch
			{
				MessageBox.Show("Linear Sample UnfoldMethod already exists :(");
				return;
			}
			
			// Add a bend table fold method. This uses error trapping to catch if an
			// invalid bend table file was specified.
			try
			{
				oSheetMetalCompDef.UnfoldMethods.AddBendTableFromFile(
				"Table Sample",
				OpenFile("Bend Table (*.txt)|*.txt"));
			}
			catch
			{
				MessageBox.Show("Unable to load bend table");
			}
			
			// Make the new linear method the active unfold method for the document.
			UnfoldMethod oUnfoldMethod;
			oUnfoldMethod = oSheetMetalCompDef.UnfoldMethods["Linear Sample"];
			oStyle.UnfoldMethod = oUnfoldMethod;
			
			// Activate this style, which will also update the part.
			oStyle.Activate();
			
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use:
		//// This sample illustrates getting information about sheet metal styles,
		//// unfold methods, and thickness.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void SheetMetalStyleDisplay()
		{
			
			// Set a reference to the sheet metal document.
			// This assumes a part document is active.
			PartDocument oPartDoc;
			oPartDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			
			// Make sure the document is a sheet metal document.
			if (oPartDoc.SubType != "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}")
			{
				MessageBox.Show("A sheet metal document must be open.");
				return;
			}
			
			// Get the sheet metal component definition. Because this is a part document whose
			// sub type is sheet metal, the document will return a SheetMetalComponentDefinition
			// instead of a PartComponentDefinition.
			SheetMetalComponentDefinition oSheetMetalCompDef;
			oSheetMetalCompDef =  (Inventor.SheetMetalComponentDefinition) (oPartDoc.ComponentDefinition);
			
			oSheetMetalCompDef.ActiveSheetMetalStyle.PunchRepresentationType = PunchRepresentationTypeEnum.k2DSketchAndCenterMarkPunchRepresentation;
			// Iterate through the sheet metal styles.
			
			foreach (SheetMetalStyle oStyle in oSheetMetalCompDef.SheetMetalStyles)
			{
				
				// Display information about the style.
				if (oStyle == oSheetMetalCompDef.ActiveSheetMetalStyle)
				{
					Debug.Print("** Active SheetMetal Style **");
				}
				
				Debug.Print("Name: " + oStyle.Name);
				Debug.Print(" Bend Radius: " + oStyle.BendRadius);
				Debug.Print(" Bend Relief Depth: " + oStyle.BendReliefDepth);
				Debug.Print(" Bend Relief Width: " + oStyle.BendReliefWidth);
				
				switch (oStyle.BendReliefShape)
				{
					case BendReliefShapeEnum.kDefaultBendReliefShape:
						Debug.Print(" Bend Relief Shape: Default");
						break;
					case BendReliefShapeEnum.kRoundBendReliefShape:
						Debug.Print(" Bend Relief Shape: Round");
						break;
					case BendReliefShapeEnum.kStraightBendReliefShape:
						Debug.Print(" Bend Relief Shape: Straight");
						break;
					case BendReliefShapeEnum.kTearBendReliefShape:
						Debug.Print(" Bend Relief Shape: Tear");
						break;
				}
				
				switch (oStyle.BendTransition)
				{
					case BendTransitionEnum.kDefaultBendTransition:
						Debug.Print(" Bend Transition: Default");
						break;
					case BendTransitionEnum.kArcBendTransition:
						Debug.Print(" Bend Transition: Arc");
						break;
					case BendTransitionEnum.kIntersectionBendTransition:
						Debug.Print(" Bend Transition: Intersection");
						break;
					case BendTransitionEnum.kNoBendTransition:
						Debug.Print(" Bend Transition: No Bend");
						break;
					case BendTransitionEnum.kStraightLineBendTransition:
						Debug.Print(" Bend Transition: Straight Line");
						break;
					case BendTransitionEnum.kTrimToBendBendTransition:
						Debug.Print(" Bend Transition: Trom to Bend");
						break;
				}
				
				switch (oStyle.CornerReliefShape)
				{
					case CornerReliefShapeEnum.kDefaultCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Default");
						break;
					case CornerReliefShapeEnum.kRoundCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Round");
						break;
					case CornerReliefShapeEnum.kSquareCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Square");
						break;
					case CornerReliefShapeEnum.kTearCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Tear");
						break;
					case CornerReliefShapeEnum.kArcWeldCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Arc Weld");
						break;
					case CornerReliefShapeEnum.kFullRoundCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Full Found");
						break;
					case CornerReliefShapeEnum.kIntersectionCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Intersection");
						break;
					case CornerReliefShapeEnum.kLinearWeldReliefShape:
						Debug.Print(" Corner Relief Shape: Linear Weld");
						break;
					case CornerReliefShapeEnum.kTrimToBendReliefShape:
						Debug.Print(" Corner Relief Shape: Trim to Bend");
						break;
					case CornerReliefShapeEnum.kNoReplacementCornerReliefShape:
						Debug.Print(" Corner Relief Shape: No Replacement");
						break;
					case CornerReliefShapeEnum.kRoundWithRadiusCornerReliefShape:
						Debug.Print(" Corner Relief Shape: Round with Radius");
						break;
				}
				
				Debug.Print(" Corner Relief Size: " + oStyle.CornerReliefSize);
				Debug.Print(" Minimum Remnant: " + oStyle.MinimumRemnant);
				
				Debug.Print(" Thickness: " + oStyle.Thickness);
				
				Debug.Print(" -------------------------- ");
				
			}
			
			// Display information about the unfold methods.
			Debug.Print("");
			Debug.Print("Unfold Methods");
			
			foreach (UnfoldMethod oUnfoldMethod in oSheetMetalCompDef.UnfoldMethods)
			{
				Debug.Print(" " + oUnfoldMethod.Name);
				switch (oUnfoldMethod.UnfoldMethodType)
				{
					case UnfoldMethodTypeEnum.kBendTableUnfoldMethod:
						Debug.Print(" Unfold Method Type: Bend Table");
						break;
					case UnfoldMethodTypeEnum.kLinearUnfoldMethod:
						Debug.Print(" Unfold Method Type: Linear");
						Debug.Print(" Value: " + oUnfoldMethod.kFactor);
						break;
					case UnfoldMethodTypeEnum.kCustomEquationUnfoldMethod:
						Debug.Print(" Unfold Method Type: Custom Equation");
						break;
				}
				
				Debug.Print(" -------------------------- ");
			}
			
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use: This sample illustrates editing the thickness of a sheet metal part
		////
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void SetSheetMetalThickness()
		{
			
			// Set a reference to the sheet metal document.
			// This assumes a sheet metal document is active.
			PartDocument oPartDoc;
			oPartDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			// Get the sheet metal component definition. Because this is a part document whose
			// sub type is sheet metal, the document will return a SheetMetalComponentDefinition
			// instead of a PartComponentDefinition.
			SheetMetalComponentDefinition oSheetMetalCompDef;
			oSheetMetalCompDef =  (Inventor.SheetMetalComponentDefinition) (oPartDoc.ComponentDefinition);
			
			// Change Thickness
			oSheetMetalCompDef.ActiveSheetMetalStyle.Thickness = "0.50 in";
			
			// Update the part.
			_InvApplication.ActiveDocument.Update();
			
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use:
		//// This sample demonstrates the creation of sheet metal face and cut features.
		//// It creates a new sheet metal document, create a face feature, a cut feature
		//// and another face feature.  The second face feature butts up to the first
		//// face feature so it automatically creates a bend between them.
		////
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void FaceAndCutFeatureCreation()
		{
			
			// Create a new sheet metal document, using the default sheet metal template.
			PartDocument oSheetMetalDoc;
			oSheetMetalDoc =  (Inventor.PartDocument) (_InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject, SystemOfMeasureEnum.kDefaultSystemOfMeasure, DraftingStandardEnum.kDefault_DraftingStandard, "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}"), System.Convert.ToBoolean(-1)));
			
			// Set a reference to the component definition.
			SheetMetalComponentDefinition oCompDef;
			oCompDef =  (Inventor.SheetMetalComponentDefinition) (oSheetMetalDoc.ComponentDefinition);
			
			// Set a reference to the sheet metal features collection.
			SheetMetalFeatures oSheetMetalFeatures;
			oSheetMetalFeatures =  (Inventor.SheetMetalFeatures) (oCompDef.Features);
			
			// Create a new sketch on the X-Y work plane.
			PlanarSketch oSketch;
			oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes[3], false);
			
			// Set a reference to the transient geometry object.
			TransientGeometry oTransGeom;
			oTransGeom = _InvApplication.TransientGeometry;
			
			// Draw a 20cm x 15cm rectangle with the corner at (0,0)
			oSketch.SketchLines.AddAsTwoPointRectangle(oTransGeom.CreatePoint2d(0, 0), oTransGeom.CreatePoint2d(20, 15));
			
			// Create a profile.
			Profile oProfile;
			oProfile = oSketch.Profiles.AddForSolid();
			
			FaceFeatureDefinition oFaceFeatureDefinition;
			oFaceFeatureDefinition = oSheetMetalFeatures.FaceFeatures.CreateFaceFeatureDefinition(oProfile);
			
			// Create a face feature.
			FaceFeature oFaceFeature;
			oFaceFeature = oSheetMetalFeatures.FaceFeatures.Add(oFaceFeatureDefinition);
			
			// Get the top face for creating the new sketch.
			SelectionFilterEnum[] aSelectTypes = new SelectionFilterEnum[1];
			aSelectTypes[0] = SelectionFilterEnum.kPartFaceFilter;
			ObjectsEnumerator oFoundFaces;
			oFoundFaces = oCompDef.FindUsingPoint(oTransGeom.CreatePoint(1, 1, oCompDef.Thickness.Value),
                ref aSelectTypes, 0.001, System.Convert.ToBoolean(-1));
			Face oFrontFace;
			oFrontFace =  (Inventor.Face) (oFoundFaces[1]);
			
			// Create a new sketch on this face, but use the method that allows you to
			// control the orientation and orgin of the new sketch.
			oSketch = oCompDef.Sketches.Add(oFrontFace, false);
			
			// Create the interior 3cm x 2cm rectangle for the cut.
			oSketch.SketchLines.AddAsTwoPointRectangle(oTransGeom.CreatePoint2d(2, 5.5), oTransGeom.CreatePoint2d(5, 11));
			
			// Create a profile.
			oProfile = oSketch.Profiles.AddForSolid();
			
			// Create a cut definition object
			CutDefinition oCutDefinition;
			oCutDefinition = oSheetMetalFeatures.CutFeatures.CreateCutDefinition(oProfile);
			
			// Set extents to 'Through All'
			oCutDefinition.SetThroughAllExtent(PartFeatureExtentDirectionEnum.kNegativeExtentDirection);
			
			// Create the cut feature
			CutFeature oCutFeature;
			oCutFeature = oSheetMetalFeatures.CutFeatures.Add(oCutDefinition);
			
			// Create a new sketch on the X-Z work plane.
			oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes[2], false);
			
			// Draw a 15cm x 10cm rectangle with the corner at (0,0)
			oSketch.SketchLines.AddAsTwoPointRectangle(oTransGeom.CreatePoint2d(0, 0), oTransGeom.CreatePoint2d(-15, 10));
			
			// Create a profile.oBendEdgesoBendEdges
			oProfile = oSketch.Profiles.AddForSolid();
			
			oFaceFeatureDefinition = oSheetMetalFeatures.FaceFeatures.CreateFaceFeatureDefinition(oProfile);
			
			// Create a face feature.
			oFaceFeature = oSheetMetalFeatures.FaceFeatures.Add(oFaceFeatureDefinition);
			
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use: The Unfold and Refold feature related calls
		////
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void FoldPart()
		{
			
			// Set a reference to the sheet metal document.
			// This assumes a part document is active.
			PartDocument oPartDoc;
			oPartDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			// Make sure the document is a sheet metal document.
			if (oPartDoc.SubType != "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}")
			{
				MessageBox.Show("A sheet metal document must be open.");
				return;
			}
			
			SheetMetalComponentDefinition sheetMetalDef;
			sheetMetalDef =  (Inventor.SheetMetalComponentDefinition) (oPartDoc.ComponentDefinition);
			
			// Look at the face for a planar face that lies on the X-Y plane.
			
			Face baseFace;
			baseFace = null;
			foreach (Face face in sheetMetalDef.SurfaceBodies[1].Faces)
			{
				if (face.SurfaceType == SurfaceTypeEnum.kPlaneSurface)
				{
					if (System.Math.Round(face.PointOnFace.Z, 7) == 0)
					{
						baseFace = face;
						break;
					}
				}
			}
			
			SheetMetalFeatures sheetMetalFeatures;
			sheetMetalFeatures =  (Inventor.SheetMetalFeatures) (sheetMetalDef.Features);
			
			// Check to see if a base found was found.
			if (baseFace != null)
			{
				
				// Unfold all of the bends so the part is flat.
				UnfoldFeature unfoldFeature;
				unfoldFeature = sheetMetalFeatures.UnfoldFeatures.Add(baseFace, null, null);
				//MsgBox "Part unfolded."
				
				// Refold each bend, one at a time.
				int i;
				for (i = 1; i <= sheetMetalDef.Bends.Count; i++)
				{
					//MsgBox "Refolding bend " & i & " of " & sheetMetalDef.bends.count
					
					// Add the bend to an ObjectCollection.
					ObjectCollection bends;
					bends = _InvApplication.TransientObjects.CreateObjectCollection();
					bends.Add(sheetMetalDef.Bends[i]);
					
					// Create the refold feature.
					sheetMetalFeatures.RefoldFeatures.Add(unfoldFeature.StationaryFace, bends, null);
				}
			}
			
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use: Gets Flat bend info for active doc
		////
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void GetBendResults()
		{
			
			// Set a reference to the sheet metal document.
			// This assumes a part document is active.
			PartDocument oPartDoc;
			oPartDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			// Make sure the document is a sheet metal document.
			if (oPartDoc.SubType != "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}")
			{
				MessageBox.Show("A sheet metal document must be open.");
				return;
			}
			
			SheetMetalComponentDefinition oSheetMetalCompDef;
			oSheetMetalCompDef =  (Inventor.SheetMetalComponentDefinition) (oPartDoc.ComponentDefinition);
			
			if (! oSheetMetalCompDef.HasFlatPattern)
			{
				oSheetMetalCompDef.Unfold();
			}
			
			FlatPattern oFlatPattern;
			oFlatPattern = oSheetMetalCompDef.FlatPattern;
			
			oFlatPattern.PunchRepresentationType = PunchRepresentationTypeEnum.kFormedFeaturePunchRepresentation;
			
			
			foreach (FlatBendResult oBendResult in oFlatPattern.FlatBendResults)
			{
				
				string strResult;
				strResult = "Internal Name: " + oBendResult.InternalName + ", ";
				
				if (oBendResult.IsOnBottomFace)
				{
					strResult = strResult + "On Bottom, ";
				}
				else
				{
					strResult = strResult + "On Top, ";
				}
				
				strResult = strResult + "Angle: " + _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.Angle, UnitsTypeEnum.kDefaultDisplayAngleUnits) + ", ";
				
				strResult = strResult + "Inner Radius: " + _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.InnerRadius, UnitsTypeEnum.kDefaultDisplayLengthUnits) + ", ";
				
				if (oBendResult.IsDirectionUp)
				{
					strResult = strResult + "Bend Direction: " + "Bend Up";
				}
				else
				{
					strResult = strResult + "Bend Direction: " + "Bend Down";
				}
				
				
				Edge oWireEdge = oBendResult.Edge;
				strResult = strResult + "start point of edge (" + oWireEdge.StartVertex.Point.X + "  ," + oWireEdge.StartVertex.Point.Y + " , " + oWireEdge.StartVertex.Point.Z + ")";
				
				
				strResult = strResult + "end point of edge (" + oWireEdge.StopVertex.Point.X + " , " + oWireEdge.StopVertex.Point.Y + " , " + oWireEdge.StopVertex.Point.Z + ")";
				
				oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
				oWireEdge.StartVertex.Point.X,
				oWireEdge.StartVertex.Point.Y,
				oWireEdge.StartVertex.Point.Z), false);
				oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
				oWireEdge.StopVertex.Point.X,
				oWireEdge.StopVertex.Point.Y,
				oWireEdge.StopVertex.Point.Z), false);
				
				
				Debug.Print(strResult);
			}
			
			//Dim oE As Edge
			//For Each oE In oFlatPattern.GetEdgesOfType(FlatPatternEdgeTypeEnum.kBendUpFlatPatternEdge )
			//     oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
			//                                             oE.StartVertex.Point.X,
			//                                              oE.StartVertex.Point.Y,
			//                                               oE.StartVertex.Point.Z))
			//    oFlatPattern.WorkPoints.AddFixed(_InvApplication.TransientGeometry.CreatePoint(
			//                                             oE.StopVertex.Point.X,
			//                                              oE.StopVertex.Point.Y,
			//                                               oE.StopVertex.Point.Z))
			//Next
			
		}
		
		
		//User has to select a bend line in a drawing document
		//that belongs to the flat bend pattern of a SheetMetalPart
		public void GetBendResultFromSelectedCurve()
		{
			
			//Gets the selected curve segment
			DrawingCurveSegment oDwCurveSegment;
			oDwCurveSegment =  (Inventor.DrawingCurveSegment) (_InvApplication.ActiveDocument.SelectSet[1]);
			
			//Gets full drawing curve from the segment
			DrawingCurve oDrawingCurve;
			oDrawingCurve = oDwCurveSegment.Parent;
			
			//Gets edge
			Edge oEdge;
			oEdge =  (Inventor.Edge) (oDrawingCurve.ModelGeometry);
			
			//Retrieves component definition from the edge
			SheetMetalComponentDefinition oSMDef;
			oSMDef =  (Inventor.SheetMetalComponentDefinition) (oEdge.Parent.ComponentDefinition);
			
			FlatPattern oFlatPattern;
			oFlatPattern = oSMDef.FlatPattern;
			
			//Gets flat bend result corresponding to the edge
			FlatBendResult oBendResult;
			oBendResult = oFlatPattern.FlatBendResults[oEdge];
			
			
			//Prints Flat Bend Results
			Debug.Print("---------------- Flat Bend Infos ----------------");
			
			Debug.Print("Internal Name: " + oBendResult.InternalName);
			
			if (oBendResult.IsOnBottomFace)
			{
				Debug.Print("Bend On Bottom Face");
			}
			else
			{
				Debug.Print("Bend On Top Face");
			}
			
			Debug.Print((string) ("Bend Angle = " + _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.Angle, UnitsTypeEnum.kDefaultDisplayAngleUnits)));
			
			Debug.Print((string) ("Bend Radius = " + _InvApplication.ActiveDocument.UnitsOfMeasure.GetStringFromValue(oBendResult.InnerRadius, UnitsTypeEnum.kDefaultDisplayLengthUnits)));
			
			if (oBendResult.IsDirectionUp)
			{
				Debug.Print("Bend Direction: " + "Bend Up");
			}
			else
			{
				Debug.Print("Bend Direction: " + "Bend Down");
			}
			
			Debug.Print("-------------------------------------------------");
			
		}
		
		
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use:
		//// This program demonstrates the creation of a punch tool feature.
		//// It uses one of the punch features that’s delivered with Inventor.
		//// It assumes you already have an existing sheet metal part and have
		//// selected a face to place the punch feature on.  The selected face
		//// should be large so there is room for the punch features.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void PlacePunchFeature()
		{
			
			// Set a reference to the sheet metal document.
			// This assumes a part document is active.
			PartDocument oPartDoc;
			oPartDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			// Make sure the document is a sheet metal document.
			if (oPartDoc.SubType != "{9C464203-9BAE-11D3-8BAD-0060B0CE6BB4}")
			{
				MessageBox.Show("A sheet metal document must be open.");
				return;
			}
			
			SheetMetalComponentDefinition oSMDef;
			oSMDef =  (Inventor.SheetMetalComponentDefinition) (oPartDoc.ComponentDefinition);
			
			
			
			// Get the selected face that will be used for the creation
			// of the sketch that will contain the sketch points.
			Face oFace;
			
			try
			{
				oFace =  (Inventor.Face) (oPartDoc.SelectSet[1]);
			}
			catch
			{
				MessageBox.Show("A planar face must be selected.");
				return;
			}
			
			if (oFace.SurfaceType != SurfaceTypeEnum.kPlaneSurface)
			{
				MessageBox.Show("A planar face must be selected.");
				return;
			}
			
			// Create a sketch on the selected face.
			PlanarSketch oSketch;
			oSketch = oSMDef.Sketches.Add(oFace, false);
			
			// Create some points on the sketch.  The model will need to
			// be of a size that these points lie on the model.
			ObjectCollection oPoints;
			oPoints = _InvApplication.TransientObjects.CreateObjectCollection();
			
			TransientGeometry oTG;
			oTG = _InvApplication.TransientGeometry;
			
			SketchPoint oPoint;
			oPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(8, 8), true);
			oPoints.Add(oPoint);
			
			oPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(12, 6), true);
			oPoints.Add(oPoint);
			
			oPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(10, 10), false);
			oPoints.Add(oPoint);
			
			SheetMetalFeatures oSMFeatures;
			oSMFeatures =  (Inventor.SheetMetalFeatures) (oSMDef.Features);
			
			// Create an iFeatureDefinition object for a punch tool.
			iFeatureDefinition oiFeatureDef;
			oiFeatureDef = oSMFeatures.PunchToolFeatures.CreateiFeatureDefinition(OpenFile("iFeature Def (*.ide)|*.ide"));
			
			// Set the input.
			
			foreach (iFeatureInput oInput in oiFeatureDef.iFeatureInputs)
			{
				iFeatureParameterInput oParamInput;
				switch (oInput.Name)
				{
					case "Length":
						oParamInput =  (Inventor.iFeatureParameterInput) (oInput);
						oParamInput.Expression = "0.875 in";
						break;
					case "Hole_Diameter":
						oParamInput =  (Inventor.iFeatureParameterInput) (oInput);
						oParamInput.Expression = "0.5 in";
						break;
					case "Slot_Width":
						oParamInput =  (Inventor.iFeatureParameterInput) (oInput);
						oParamInput.Expression = "0.3875 in";
						break;
					case "Fillet":
						oParamInput =  (Inventor.iFeatureParameterInput) (oInput);
						oParamInput.Expression = "0.0625 in";
						break;
					case "Thickness":
						oParamInput =  (Inventor.iFeatureParameterInput) (oInput);
						oParamInput.Expression = "0.125 in";
						break;
				}
			}
			
			// Create the iFeature at a 45 degree angle.
			PunchToolFeature oPunchTool;
			oPunchTool = oSMFeatures.PunchToolFeatures.Add(oPoints, oiFeatureDef, System.Math.PI / 4);
			
			//oSMDef.ActiveSheetMetalStyle.PunchRepresentationType = PunchRepresentationTypeEnum.kDefaultPunchRepresentation
		}
		
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//// Use:
		//// demonstrates creating an R12 DXF file that will have a layer called "Outer"
		//// where the curves for the outer shape will be created.
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public void WriteSheetMetalDXF()
		{
			
			// Get the active document.  This assumes it is a part document.
			PartDocument oDoc;
			oDoc =  (Inventor.PartDocument) (_InvApplication.ActiveDocument);
			
			// Get the DataIO object.
			DataIO oDataIO;
			oDataIO = oDoc.ComponentDefinition.DataIO;
			
			// Build the string that defines the format of the DXF file.
			string sOut;
			sOut = "FLAT PATTERN DXF?AcadVersion=R12&OuterProfileLayer=Outer";
			
			// Create the DXF file.   
			oDataIO.WriteDataToFile(sOut, "c:\\Temp\\flat.dxf");
			
		}
		
	}
	
}
