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

        public void CreateSketch()
        {
            DrawingDocument oDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            Sheet oSheet = default(Sheet);
            oSheet = oDoc.ActiveSheet;

            // Create the sketch.
            DrawingSketch oSketch = default(DrawingSketch);
            oSketch = oSheet.Sketches.Add();

            // Open the sketch for edit in the user interface.
            oSketch.Edit();

            oSketch.SketchCircles.AddByCenterRadius(_InvApplication.TransientGeometry.CreatePoint2d(8, 8), 2);

            // Exit edit.
            oSketch.ExitEdit();
        }


        public void CreateStyles()
        {
            DrawingDocument oDoc = (DrawingDocument)_InvApplication.ActiveDocument;


            //get Drawing Styles Manager
            DrawingStylesManager oDStylesMan = default(DrawingStylesManager);
            oDStylesMan = oDoc.StylesManager;

            // create a new text style 
            // by copying from an existing style
            //the new name is "MyNewTextStyle"
            TextStyle oNewTextStyle = default(TextStyle);
            oNewTextStyle = (TextStyle)oDStylesMan.TextStyles["Label Text (ANSI)"].Copy("MyNewTextStyle");

            // change some properties of the new style
            oNewTextStyle.FontSize *= 2;
            oNewTextStyle.Italic = true;


            LeaderStyle oNewLeaderStyle = default(LeaderStyle);
            oNewLeaderStyle = (LeaderStyle)oDStylesMan.LeaderStyles[1].Copy("MyNewLeaderStyle");

            oNewLeaderStyle.Color = _InvApplication.TransientObjects.CreateColor(255, 0, 0);
            oNewLeaderStyle.LineWeight *= 2;

            DrawingStandardStyle oNewDrawingStyle = default(DrawingStandardStyle);
            oNewDrawingStyle = (DrawingStandardStyle)oDStylesMan.StandardStyles[1].Copy("MyNewDSStyle");

            //change some general settings
            oNewDrawingStyle.LinearUnits = UnitsTypeEnum.kCentimeterLengthUnits;

            ObjectDefaultsStyle oNewObjDefaultStyle = default(ObjectDefaultsStyle);
            oNewObjDefaultStyle = (ObjectDefaultsStyle)oDStylesMan.ObjectDefaultsStyles[1].Copy("MyNewObjStyles");

            //change some properties
            // e.g. the border text style uses the MyNewTextStyle
            oNewObjDefaultStyle.BorderTextStyle = oNewTextStyle;

            //new standard style uses the new objects styles
            oNewDrawingStyle.ActiveObjectDefaults = oNewObjDefaultStyle;

            // the document uses the new DrawingStandardStyle
            oDStylesMan.ActiveStandardStyle = oNewDrawingStyle;

        }

        /// <summary>
        /// This sample demonstrates the creation of a baseline set dimension in a drawing. 
        /// Create a drawing view and select multiple edges in the view before running the sample.  
        /// </summary>
        /// <remarks></remarks>

        public void CreateBaselineDimensionSet()
            {
             // Set a reference to the drawing document.
             // This assumes a drawing document is active.
               DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

             // Set a reference to the active sheet.
             Sheet oActiveSheet = default(Sheet);
             oActiveSheet = oDrawDoc.ActiveSheet;

             ObjectCollection oIntentCollection = default(ObjectCollection);
             oIntentCollection = _InvApplication.TransientObjects.CreateObjectCollection();

             // Get all the selected drawing curve segments.
        
             DrawingCurve oDrawingCurve = default(DrawingCurve);

             foreach ( DrawingCurveSegment oSeg in oDrawDoc.SelectSet) {
              // Set a reference to the drawing curve.
                 oDrawingCurve = oSeg.Parent;

              GeometryIntent oDimIntent = oActiveSheet.CreateGeometryIntent(oDrawingCurve);

              oIntentCollection.Add(oDimIntent);
             }

             // Set a reference to the view to which the curve belongs.
             DrawingView oDrawingView    = oDrawingCurve.Parent;

             // Set a reference to the baseline dimension sets collection.
             BaselineDimensionSets oBaselineSets   = oActiveSheet.DrawingDimensions.BaselineDimensionSets;

             // Determine the placement point
             Point2d oPlacementPoint   = _InvApplication.TransientGeometry.CreatePoint2d(oDrawingView.Left - 5, oDrawingView.Center.Y);

             // Create a vertical baseline set dimension.
             BaselineDimensionSet oBaselineSet = oBaselineSets.Add(oIntentCollection, oPlacementPoint, DimensionTypeEnum.kHorizontalDimensionType);

            }

        /// <summary>
        /// creation of a balloon. Select a linear drawing curve and run the sample
        /// </summary>
        /// <remarks></remarks>
        public void CreateBalloon()
            {
             // Set a reference to the drawing document.
             // This assumes a drawing document is active.
              DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

             // Set a reference to the active sheet.
             Sheet oActiveSheet  = oDrawDoc.ActiveSheet;

             // Set a reference to the drawing curve segment.
             // This assumes that a drwaing curve is selected.
             DrawingCurveSegment oDrawingCurveSegment   = oDrawDoc.SelectSet[1];

             // Set a reference to the drawing curve.
             DrawingCurve oDrawingCurve = oDrawingCurveSegment.Parent;

             // Get the mid point of the selected curve
             // assuming that the selection curve is linear
             Point2d oMidPoint  = oDrawingCurve.MidPoint;

             // Set a reference to the TransientGeometry object.
             TransientGeometry oTG  = _InvApplication.TransientGeometry;

             ObjectCollection oLeaderPoints  = _InvApplication.TransientObjects.CreateObjectCollection();

             // Create a couple of leader points.
             oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 10));
             oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 5));

             // Add the GeometryIntent to the leader points collection.
             // This is the geometry that the balloon will attach to.
             GeometryIntent oGeometryIntent  = oActiveSheet.CreateGeometryIntent(oDrawingCurve);
             oLeaderPoints.Add(oGeometryIntent);

             // Set a reference to the parent drawing view of the selected curve
             DrawingView oDrawingView   = oDrawingCurve.Parent;

             // Set a reference to the referenced model document
             Document oModelDoc = oDrawingView.ReferencedDocumentDescriptor.ReferencedDocument;

             // Check if a partslist or a balloon has already been created for this model
             bool IsDrawingBOMDefined = false;
             IsDrawingBOMDefined = oDrawDoc.DrawingBOMs.IsDrawingBOMDefined(oModelDoc.FullFileName);

             Balloon oBalloon = null;


             if (IsDrawingBOMDefined) {
              // Just create the balloon with the leader points
              // All other arguments can be ignored
              oBalloon = oDrawDoc.ActiveSheet.Balloons.Add(oLeaderPoints);

             } else {
              // First check if the 'structured' BOM view has been enabled in the model

              // Set a reference to the model's BOM object
                 AssemblyDocument oAssDoc = (AssemblyDocument)oModelDoc;
                 AssemblyComponentDefinition oComDef = oAssDoc.ComponentDefinition;
              BOM oBOM =  oComDef.BOM;


              if (oBOM.StructuredViewEnabled) {
               // Level needs to be specified
               // Numbering options have already been defined
               // Get the Level ('All levels' or 'First level only')
               // from the model BOM view - must use the same here
               PartsListLevelEnum Level = default(PartsListLevelEnum);
               if (oBOM.StructuredViewFirstLevelOnly) {
                Level = PartsListLevelEnum.kStructured;
               } else {
                Level = PartsListLevelEnum.kStructuredAllLevels;
               }

               // Create the balloon by specifying just the level
               oBalloon = oActiveSheet.Balloons.Add(oLeaderPoints,null , Level);

              } else {
               // Level and numbering options must be specified
               // The corresponding model BOM view will automatically be enabled
               NameValueMap oNumberingScheme  = _InvApplication.TransientObjects.CreateNameValueMap();

               // Add the option for a comma delimiter
               oNumberingScheme.Add("Delimiter", ",");

               // Create the balloon by specifying the level and numbering scheme
               oBalloon = oActiveSheet.Balloons.Add(oLeaderPoints,null , PartsListLevelEnum.kStructuredAllLevels, oNumberingScheme);
              }
             }
            }

        /// <summary>
        ///  creation of a parts list. The parts list is placed at the
        ///  top right corner of the border if one exists, else it is placed
        ///  at the top right corner of the sheet. 
        /// To run this sample, have a drawing document open. 
        /// The active sheet in the drawing should have at least 
        /// one drawing view and the first drawing view on the sheet 
        /// should not be a draft view. 
        /// </summary>
        /// <remarks></remarks>

        public void CreatePartsList()
        { 
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            //Set a reference to the active sheet.
            Sheet oSheet  = oDrawDoc.ActiveSheet;

            // Set a reference to the first drawing view on
            // the sheet. This assumes the first drawing
            // view on the sheet is not a draft view.
            DrawingView oDrawingView  = oSheet.DrawingViews[1];

            // Set a reference to th sheet's border
            Inventor.Border oBorder   = oSheet.Border;

            Point2d oPlacementPoint =null;

            if ((oBorder != null))
            {
                // A border exists. The placement point
                // is the top-right corner of the border.
                oPlacementPoint = oBorder.RangeBox.MaxPoint;
            }
            else
            {
                // There is no border. The placement point
                // is the top-right corner of the sheet.
                oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(oSheet.Width, oSheet.Height);
            }

            // Create the parts list.
            PartsList oPartsList  = oSheet.PartsLists.Add(oDrawingView, oPlacementPoint);

        }

        /// <summary>
        ///  how to create a custom table
        /// </summary>
        /// <remarks></remarks>
        public void CreateCustomTable()
        {
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            // Set a reference to the active sheet.
            Sheet oSheet = default(Sheet);
            oSheet = oDrawDoc.ActiveSheet;

            // Set the column titles
            string[] oTitles = new string[] {
                  "Part Number",
                  "Quantity",
                  "Material"
                 };



            // Set the contents of the custom table (contents are set row-wise)
            string[] oContents = new string[] {
                                                  "1",
                                                  "1",
                                                  "Brass",
                                                  "2",
                                                  "2",
                                                  "Aluminium",
                                                  "3",
                                                  "1",
                                                  "Steel"
                                                 };

            // Set the column widths (defaults to the column title width if not specified)
            double[] oColumnWidths = new double[] {
                                                      2.5,
                                                      2.5,
                                                      4
                                                     };

            // Create the custom table
            CustomTable oCustomTable = default(CustomTable);
            oCustomTable = oSheet.CustomTables.Add("My Table", _InvApplication.TransientGeometry.CreatePoint2d(15, 15), 3, 3, oTitles, oContents, oColumnWidths);

            // Change the 3rd column to be left justified.
            oCustomTable.Columns[3].ValueHorizontalJustification = HorizontalTextAlignmentEnum.kAlignTextLeft;

            // Create a table format object
            TableFormat oFormat =   oSheet.CustomTables.CreateTableFormat();

            // Set inside line color to red.
            oFormat.InsideLineColor = _InvApplication.TransientObjects.CreateColor(255, 0, 0);

            // Set outside line weight.
            oFormat.OutsideLineWeight = 0.1;

            // Modify the table formats
            oCustomTable.OverrideFormat = oFormat;
        }

        /// <summary>
        /// sample illustrates querying the contents of the revision table. 
        /// and how to add a row
        /// To run this sample have a sheet active that contains a revision table
        /// </summary>
        /// <remarks></remarks>
        public void RevisionTableQuery()
        {
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            // Set a reference to the first revision table on the active sheet.
            // This assumes that a revision table is on the active sheet.
            RevisionTable oRevTable = default(RevisionTable);
            oRevTable = oDrawDoc.ActiveSheet.RevisionTables[1];

            // Iterate through the contents of the revision table.
            int i = 0;

            for (i = 1; i <= oRevTable.RevisionTableRows.Count; i++)
            {
                // Get the current row.
                RevisionTableRow oRow= oRevTable.RevisionTableRows[i];

                // Iterate through each column in the row.
                long j = 0;
                for (j = 1; j <= oRevTable.RevisionTableColumns.Count; j++)
                {
                    // Get the current cell.
                    RevisionTableCell oCell = default(RevisionTableCell);
                    oCell = oRow[j];

                    // Display the value of the current cell.
                    Debug.Print("Row: " + i + ", Column: " + oRevTable.RevisionTableColumns[j].Title + " = " + oCell.Text);



                }
            }

            //add a new row
            RevisionTableRow oNewRow = default(RevisionTableRow);
            oNewRow = oRevTable.RevisionTableRows.Add();


        }

        /// <summary>
        ///  creation of hole tables in a drawing. 
        /// Select a drawing view that contains holes and run the following sample
        /// </summary>
        /// <remarks></remarks>
        public void CreateHoleTables()
        {
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            // Set a reference to the active sheet.
            Sheet oActiveSheet  = oDrawDoc.ActiveSheet;

            // Set a reference to the drawing view.
            // This assumes that a drawing view is selected.
            DrawingView oDrawingView  = oDrawDoc.SelectSet[1];

            // Create origin indicator if it has not been already created.
            if (!oDrawingView.HasOriginIndicator)
            {
                // Create point intent to anchor the origin to.
                GeometryIntent oDimIntent = null;
                Point2d oPointIntent = null;

                // Get the first curve on the view
                DrawingCurve oCurve =  oDrawingView.get_DrawingCurves()[1];

                // Check if it has a strt point
                oPointIntent = oCurve.StartPoint;

                if (oPointIntent == null)
                {
                    // Else use the center point
                    oPointIntent = oCurve.CenterPoint;
                }

                oDimIntent = oActiveSheet.CreateGeometryIntent(oCurve, oPointIntent);

                oDrawingView.CreateOriginIndicator(oDimIntent);
            }

            Point2d oPlacementPoint = null;

            // Set a reference to th sheet's border
            Inventor.Border oBorder = oActiveSheet.Border;

            if ((oBorder != null))
            {
                // A border exists. The placement point
                // is the top-left corner of the border.
                oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(oBorder.RangeBox.MinPoint.X, oBorder.RangeBox.MaxPoint.Y);
            }
            else
            {
                // There is no border. The placement point
                // is the top-left corner of the sheet.
                oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(0, oActiveSheet.Height);
            }

            // Create a 'view' hole table
            // This hole table includes all holes as specified by the active hole table style
            HoleTable oViewHoleTable = default(HoleTable);
            oViewHoleTable = oActiveSheet.HoleTables.Add(oDrawingView, oPlacementPoint);

            oPlacementPoint.X = oActiveSheet.Width  / 2;

            // Create a 'feature type' hole table
            // This hole table includes specified hole types only
            HoleTable oFeatureHoleTable  = oActiveSheet.HoleTables.AddByFeatureType(oDrawingView, oPlacementPoint, true, true, true, true, false, false, false);

            //add a new row

            // get the model document
            Document oModelDoc = oDrawingView.ReferencedDocumentDescriptor.ReferencedDocument;

            HoleFeature oHoleF = null;
            if (oModelDoc.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                AssemblyDocument oRefAssDoc = (AssemblyDocument)oModelDoc;
                AssemblyComponentDefinition oAssDef =  oRefAssDoc.ComponentDefinition;

                if (oAssDef.Features.HoleFeatures.Count > 0)
                {
                    //as a demo: get the first hole feature
                    oHoleF = oAssDef.Features.HoleFeatures[1];
                }
            }
            else if (oModelDoc.DocumentType == DocumentTypeEnum.kPartDocumentObject)
            {
                PartDocument oRefPartDoc = (PartDocument)oModelDoc;
                PartComponentDefinition oPartDef = oRefPartDoc.ComponentDefinition;

                if (oPartDef.Features.HoleFeatures.Count > 0)
                {
                    //as a demo: get the first hole feature
                    oHoleF = oPartDef.Features.HoleFeatures[1];
                }
            }


            // add a new row to the hole table
            if ((oHoleF != null))
            {
                DrawingCurvesEnumerator oHoleCurves   = oDrawingView.get_DrawingCurves(oHoleF);
                if (oHoleCurves.Count > 0)
                {
                    oFeatureHoleTable.HoleTableRows.Add(oHoleCurves[1]);
                }

            }
        }

        /// <summary>
        ///  creating leader text on a sheet. 
        /// Before running this sample, open a drawing document and select a linear drawing edge. 
        /// </summary>
        /// <remarks></remarks>
        public void AddLeaderNote()
        {
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc =  (DrawingDocument)_InvApplication.ActiveDocument;

            // Set a reference to the active sheet.
            Sheet oActiveSheet  = oDrawDoc.ActiveSheet;

            // Set a reference to the drawing curve segment.
            // This assumes that a drawing curve is selected.
            DrawingCurveSegment oDrawingCurveSegment   = oDrawDoc.SelectSet[1];

            // Set a reference to the drawing curve.
            DrawingCurve oDrawingCurve  = oDrawingCurveSegment.Parent;

            // Get the mid point of the selected curve
            // assuming that the selected curve is linear
            Point2d oMidPoint   = oDrawingCurve.MidPoint;

            // Set a reference to the TransientGeometry object.
            TransientGeometry oTG  = _InvApplication.TransientGeometry;

            ObjectCollection oLeaderPoints    = _InvApplication.TransientObjects.CreateObjectCollection();

            // Create a few leader points.
            oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 10));
            oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 5));

            // Create an intent and add to the leader points collection.
            // This is the geometry that the leader text will attach to.
            GeometryIntent oGeometryIntent  = oActiveSheet.CreateGeometryIntent(oDrawingCurve);

            oLeaderPoints.Add(oGeometryIntent);

            // Create text with simple string as input. Since this doesn't use
            // any text overrides, it will default to the active text style.
            string sText = null;
            sText = "API Leader Note";

            LeaderNote oLeaderNote  = oActiveSheet.DrawingNotes.LeaderNotes.Add(oLeaderPoints, sText);

            // Insert a node.
            LeaderNode oFirstNode   = oLeaderNote.Leader.RootNode.ChildNodes[1];

            LeaderNode oSecondNode  = oFirstNode.ChildNodes[1];

            oFirstNode.InsertNode(oSecondNode, oTG.CreatePoint2d(oMidPoint.X + 5, oMidPoint.Y + 5));
        }

        #region "Sketched Symbol demos"
        //creating a new sketched symbol definition object and 
        //    inserting it into the active sheet. 
        //    This sample consists of two subs. 
        //    The first demonstrates the creation of a sketched symbol definition and 
        //    the second inserts it into the active sheet. 
        //    To run the sample have a drawing document open and run the CreateSketchedSymbolDefinition Sub.
        //    After this you can run the InsertSketchedSymbolOnSheet to insert the sketched symbol into the active sheet. 
        //    The insertion sub demonstrates the use of the insertion point in the symbol's definition while inserting the symbol. 

        /// <summary>
        ///  creating a new sketched symbol definition object     '''   
        /// </summary>
        /// <remarks></remarks>
        public void CreateSketchedSymbolDefinition()
        {
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            // Create the new sketched symbol definition.
            SketchedSymbolDefinition oSketchedSymbolDef   = oDrawDoc.SketchedSymbolDefinitions.Add("Circular Callout");

            // Open the sketched symbol definition's sketch for edit. This is done by calling the Edit
            // method of the SketchedSymbolDefinition to obtain a DrawingSketch. This actually creates
            // a copy of the sketched symbol definition's and opens it for edit.
            DrawingSketch oSketch= null;
            oSketchedSymbolDef.Edit(out oSketch);

            TransientGeometry oTG  = _InvApplication.TransientGeometry;

            // Use the functionality of the sketch to add sketched symbol graphics.
            SketchLine oSketchLine  = oSketch.SketchLines.AddByTwoPoints(oTG.CreatePoint2d(0, 0), oTG.CreatePoint2d(20, 0));

            SketchCircle oSketchCircle  = oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(22, 0), 2);

            oSketch.GeometricConstraints.AddCoincident((SketchEntity)oSketchLine.EndSketchPoint, (SketchEntity)oSketchCircle);

            // Make the starting point of the sketch line the insertion point
            oSketchLine.StartSketchPoint.InsertionPoint = true;

            // Add a prompted text field at the center of the sketch circle.
            string sText = null;
            sText = "<Prompt>Enter text 1</Prompt>";
            Inventor.TextBox oTextBox   = oSketch.TextBoxes.AddFitted(oTG.CreatePoint2d(22, 0), sText);
            oTextBox.VerticalJustification = VerticalTextAlignmentEnum.kAlignTextMiddle;
            oTextBox.HorizontalJustification = HorizontalTextAlignmentEnum.kAlignTextCenter;

            oSketchedSymbolDef.ExitEdit(true);
        }

        /// <summary>
        /// insert a sketched symbol by the definition above
        /// </summary>
        /// <remarks></remarks>
        public void InsertSketchedSymbolOnSheet()
        {
            // Set a reference to the drawing document.
            // This assumes a drawing document is active.
            DrawingDocument oDrawDoc = (DrawingDocument)_InvApplication.ActiveDocument;

            // Obtain a reference to the desired sketched symbol definition.
            SketchedSymbolDefinition oSketchedSymbolDef = default(SketchedSymbolDefinition);
            oSketchedSymbolDef = oDrawDoc.SketchedSymbolDefinitions["Circular Callout"];

            Sheet oSheet  = oDrawDoc.ActiveSheet;

            // This sketched symbol definition contains one prompted string input. An array
            // must be input that contains the strings for the prompted strings.
            string[] sPromptStrings = new string[1];
            sPromptStrings[0] = "A";

            TransientGeometry oTG  = _InvApplication.TransientGeometry;

            // Add an instance of the sketched symbol definition to the sheet.
            // Rotate the instance by 45 degrees and scale by .75 when adding.
            // The symbol will be inserted at (0,0) on the sheet. Since the
            // start point of the line was marked as the insertion point, the
            // start point should end up at (0,0).
            SketchedSymbol oSketchedSymbol  = oSheet.SketchedSymbols.Add(oSketchedSymbolDef, oTG.CreatePoint2d(0, 0), (3.14159 / 4), 0.75, sPromptStrings);
        }
        #endregion



    }


}
