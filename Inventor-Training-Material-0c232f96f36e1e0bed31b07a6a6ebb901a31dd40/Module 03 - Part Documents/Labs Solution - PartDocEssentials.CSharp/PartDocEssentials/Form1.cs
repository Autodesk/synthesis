using System;
using System.Windows.Forms;
using Inventor; 

namespace PartDocEssentials
{
    public partial class Form1 : Form
    {
        Inventor.Application mApp;

        public Form1(Inventor.Application oApp)
        {
            InitializeComponent();

            mApp = oApp;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            PartDocument oDoc = mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject,
                    mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject,
                                                     SystemOfMeasureEnum.kDefaultSystemOfMeasure,
                                                     DraftingStandardEnum.kDefault_DraftingStandard, null),
                                                     true) as PartDocument;

            // Get the XZ Plane
            WorkPlane oWorkPlane = oDoc.ComponentDefinition.WorkPlanes[2];

            PlanarSketch oSketch = oDoc.ComponentDefinition.Sketches.Add(oWorkPlane, false);

            TransientGeometry oTG = mApp.TransientGeometry;

            //Create some transient points used for defining the lines (see BRep Module)
            Point2d[] oPoints = new Point2d[5];

            oPoints[0] = oTG.CreatePoint2d(0, 0);
            oPoints[1] = oTG.CreatePoint2d(-10, 0);
            oPoints[2] = oTG.CreatePoint2d(-10, -10);
            oPoints[3] = oTG.CreatePoint2d(5, -10);
            oPoints[4] = oTG.CreatePoint2d(5, -5);

            //Add the sketchlines, coincident constraints will be created automatically 
            //since the "Line.EndSketchPoint" are provided each time we create a new line
            SketchLine[] oLines = new SketchLine[5];

            oLines[0] = oSketch.SketchLines.AddByTwoPoints(oPoints[0], oPoints[1]);
            oLines[1] = oSketch.SketchLines.AddByTwoPoints(oLines[0].EndSketchPoint, oPoints[2]);
            oLines[2] = oSketch.SketchLines.AddByTwoPoints(oLines[1].EndSketchPoint, oPoints[3]);
            oLines[3] = oSketch.SketchLines.AddByTwoPoints(oLines[2].EndSketchPoint, oPoints[4]);

            oSketch.SketchArcs.AddByCenterStartEndPoint(oTG.CreatePoint2d(0, -5), oLines[3].EndSketchPoint, oLines[0].StartSketchPoint, true);

            //Create a profile for the extrusion, here no need to worry since there is only 
            //a single profile that is possible
            Profile oProfile = oSketch.Profiles.AddForSolid(true, null, null);

            //this is the old way, i.e. create the extrude feature directly. it is still supported for backward compatibility
            //ExtrudeFeature oExtrude = oDoc.ComponentDefinition.Features.ExtrudeFeatures.AddByDistanceExtent(oProfile, 
            //    5.0, PartFeatureExtentDirectionEnum.kPositiveExtentDirection, PartFeatureOperationEnum.kNewBodyOperation, 0.0);


            // Definition Way:
            PartComponentDefinition oPartDocDef = oDoc.ComponentDefinition;

            // get ExtrudeFeatures collection
            ExtrudeFeatures extrudes = oPartDocDef.Features.ExtrudeFeatures;

            // Create an extrude definition in the new surface body
            ExtrudeDefinition extrudeDef = extrudes.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kNewBodyOperation);

            // Modify the extent and taper angles.
            extrudeDef.SetDistanceExtent(8, PartFeatureExtentDirectionEnum.kPositiveExtentDirection);
            extrudeDef.SetDistanceExtentTwo(20);
            extrudeDef.TaperAngle = "-2 deg";
            extrudeDef.TaperAngleTwo = "-10 deg";
 
            // Create the extrusion.
            ExtrudeFeature extrude = extrudes.Add(extrudeDef);

            //Fit the view programmatically
            Camera oCamera = mApp.ActiveView.Camera;

            oCamera.ViewOrientationType = ViewOrientationTypeEnum.kIsoTopRightViewOrientation;
            oCamera.Apply();

            mApp.ActiveView.Fit(true);
        }

    }
}
