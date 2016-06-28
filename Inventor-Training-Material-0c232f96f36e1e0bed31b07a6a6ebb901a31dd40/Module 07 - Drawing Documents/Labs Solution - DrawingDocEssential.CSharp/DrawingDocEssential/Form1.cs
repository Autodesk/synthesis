using System;
using Inventor;

namespace DrawingDocEssential
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private Inventor.Application mApp = null;

        public Form1(Inventor.Application oApp)
        {
            InitializeComponent();

            mApp = oApp;
        }

        //Create a  Title block 
        private void Button1_Click(object sender, EventArgs e)
        {
            DrawingDocument oDoc = mApp.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject,
                                                        mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kDrawingDocumentObject, 
                                                            SystemOfMeasureEnum.kDefaultSystemOfMeasure, 
                                                            DraftingStandardEnum.kDefault_DraftingStandard, 
                                                            null),
                                                        true) as DrawingDocument;

            Sheet oSheet = oDoc.Sheets.Add(DrawingSheetSizeEnum.kADrawingSheetSize,
                                           PageOrientationTypeEnum.kDefaultPageOrientation, 
                                           "A Size", 0, 0);

            oSheet.AddDefaultBorder(null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            oSheet.AddTitleBlock(oDoc.TitleBlockDefinitions["ANSI A"], null, null);
        }

        //To execute this command, a drawing document with a view need to be available
        private void Border2_Click(object sender, EventArgs e)
        {
            DrawingDocument oDrawDoc = mApp.ActiveDocument as DrawingDocument;

            TransientGeometry oTG = mApp.TransientGeometry;

            //Create the new border definition
            BorderDefinition oBorderDef = oDrawDoc.BorderDefinitions.Add("Sample Border");

            //Open the border definition's sketch for edit.  This is done by calling the Edit
            // method of the BorderDefinition to obtain a DrawingSketch.  This actually creates
            // a copy of the border definition's and opens it for edit.
            DrawingSketch oSketch = null;

            oBorderDef.Edit(out oSketch);

            //Use the functionality of the sketch to add geometry
            oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(2, 2), oTG.CreatePoint2d(25.94, 19.59));

            oBorderDef.ExitEdit(true, "");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            //Create a new drawing document.
            DrawingDocument oDoc = mApp.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject,
                                                      mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kDrawingDocumentObject, 
                                                        SystemOfMeasureEnum.kDefaultSystemOfMeasure, 
                                                        DraftingStandardEnum.kDefault_DraftingStandard, 
                                                        null), 
                                                      true) as DrawingDocument;

            //Create a new B size sheet.
            Sheet oSheet = oDoc.Sheets.Add(DrawingSheetSizeEnum.kBDrawingSheetSize,  
                                           PageOrientationTypeEnum.kDefaultPageOrientation, 
                                           "A Size", 0, 0);

            //Add the default border.
            oSheet.AddDefaultBorder(null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            //Add ANSI A TitleBlock
            TitleBlock oTitleBlock = oSheet.AddTitleBlock(oDoc.TitleBlockDefinitions["ANSI A"], null, null);
            
            //Open the part document, invisibly.
            PartDocument oBlockPart = mApp.Documents.Open(@"C:\Temp\TestPart.ipt", false) as PartDocument;
            
            TransientGeometry oTG = mApp.TransientGeometry;

            //Create base drawing view
            DrawingView oBaseView = oSheet.DrawingViews.AddBaseView(oBlockPart as _Document,
                                                                    oTG.CreatePoint2d(10, 10), 1,
                                                                    ViewOrientationTypeEnum.kFrontViewOrientation, 
                                                                    DrawingViewStyleEnum.kHiddenLineDrawingViewStyle, "", null, null);

            //Create Projected views
            DrawingView oRightView = oSheet.DrawingViews.AddProjectedView(oBaseView, 
                                                                          oTG.CreatePoint2d(20, 18), 
                                                                          DrawingViewStyleEnum.kFromBaseDrawingViewStyle, null);

            DrawingView oIsoView = oSheet.DrawingViews.AddProjectedView(oBaseView, 
                                                                        oTG.CreatePoint2d(10, 20), 
                                                                        DrawingViewStyleEnum.kFromBaseDrawingViewStyle, null);


            //Find an edge in the part to dimension.  Any method can be used, (attributes, B-Rep query, selection, etc.).  This
            //looks through the curves in the drawing view and finds the top horizontal curve.

            DrawingCurve oSelectedCurve = null;

            foreach(DrawingCurve oCurve in oBaseView.get_DrawingCurves(null))
            {
                //Skip Circles
                if(oCurve.StartPoint!=null && oCurve.EndPoint!=null)
                {
                    if(WithinTol(oCurve.StartPoint.X, oCurve.EndPoint.X, 0.001))
                    {
                        if(oSelectedCurve == null)
                        {
                            //This is the first horizontal curve found.
                            oSelectedCurve = oCurve;
                        }
                        else
                        {
                            //Check to see if this curve is higher (smaller x value) than the current selected
                            if(oCurve.MidPoint.X < oSelectedCurve.MidPoint.X) 
                            {
                                oSelectedCurve = oCurve;
                            }
                        }
                    }
                }
            }

            if (oSelectedCurve == null)
            {
                System.Windows.Forms.MessageBox.Show("no curve is selected!");
                return;
            }
            //Create geometry intents point for the curve.
            GeometryIntent oGeomIntent1 = oSheet.CreateGeometryIntent(oSelectedCurve, PointIntentEnum.kStartPointIntent);
            GeometryIntent oGeomIntent2 = oSheet.CreateGeometryIntent(oSelectedCurve, PointIntentEnum.kEndPointIntent);

            GeneralDimensions oGeneralDimensions = oSheet.DrawingDimensions.GeneralDimensions;

            Point2d oDimPos = oTG.CreatePoint2d(oSelectedCurve.MidPoint.X - 2, oSelectedCurve.MidPoint.Y);

            DimensionStyle dimstyle = oDoc.StylesManager.DimensionStyles["Default (ANSI)"];

            Layer layer = oDoc.StylesManager.Layers["Dimension (ANSI)"];

            //Create the dimension.
            LinearGeneralDimension oLinearDim; 
            oLinearDim = oGeneralDimensions.AddLinear(oDimPos, oGeomIntent1, oGeomIntent2, 
                                                      DimensionTypeEnum.kAlignedDimensionType, true, 
                                                      dimstyle, 
                                                      layer);
        }

        private bool WithinTol(double Value1, double Value2, double tol)
        {
            return (Math.Abs(Value1 - Value2) < tol);
        }

    }
}
