////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;

using Inventor;
using Autodesk.ADN.Utility.WinUtils;
using Autodesk.ADN.Utility.Graphics;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.BasicPrimitivesSamples
{
    class BasicPrimitives
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a LineGraphics between points [0, 0, 0] and [1, 1, 1].
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void LineGraphicsDemo()
        {
            PartDocument doc =
                AdnInventorUtilities.InvApplication.ActiveDocument
                    as PartDocument;

            // ClientId, can be any string
            // Typically use instead current add-in GUID
            string clientId = "{Add-in Guid}";

            // Add a new graphics group. 
            // This will fail if a group with same name already exists
            ClientGraphics graphics =
                doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);

            // Add a new graphic node, with Id=1.
            // Id needs to be unique within graphics group
            GraphicsNode node = graphics.AddNode(1);

            // Add new data set
            GraphicsDataSets dataSets =
                doc.GraphicsDataSetsCollection.Add(clientId);

            // Add new coordinate set
            // Id needs to be unique within data set
            GraphicsCoordinateSet coordSet = dataSets.CreateCoordinateSet(1);

            // Fill up coordinates
            // Point1: [0.0, 0.0, 0.0]
            // Point2: [1.0, 1.0, 1.0]
            double[] coords = new double[]
            {
                0.0, 0.0, 0.0,
                1.0, 1.0, 1.0
            };

            coordSet.PutCoordinates(ref coords);

            // Create new GraphicsPrimitive
            LineGraphics lineGraphPrimitive = node.AddLineGraphics();
            lineGraphPrimitive.LineWeight = 5.0;

            // Set coordinates
            lineGraphPrimitive.CoordinateSet = coordSet;

            // Update the current view, so we see the graphics
            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a LineGraphics using Index and Color Sets.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void IndexSetDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;
            GraphicsDataSets dataSets = null;

            // Add some error handling in case 
            // graphics collection and data already exist
            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
                dataSets = doc.GraphicsDataSetsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
                dataSets = doc.GraphicsDataSetsCollection.Add(clientId);
            }

            // Add new node and coord set 
            // Id generation by increment - bad because previous nodes/sets 
            // may have been deleted previously, hence making count invalid

            GraphicsNode node = 
                graphics.AddNode(graphics.Count + 1);

            GraphicsCoordinateSet coordSet = 
                dataSets.CreateCoordinateSet(dataSets.Count + 1);

            double[] coords = new double[]
            {
                0.0, 0.0, 0.0, //point 1
                1.0, 1.0, 0.0, //point 2
                0.0, 1.0, 0.0, //point 3
                1.0, 0.0, 0.0  //point 4
            };

            coordSet.PutCoordinates(ref coords);

            LineGraphics lineGraphPrimitive = node.AddLineGraphics();
            lineGraphPrimitive.LineWeight = 5.0;

            //Create Coordinate Index Set
            GraphicsIndexSet indexSetCoords = dataSets.CreateIndexSet(dataSets.Count + 1); 

            indexSetCoords.Add(1, 1); //from point 1
            indexSetCoords.Add(2, 3); //connect to point 3
            indexSetCoords.Add(3, 3); //from point 3
            indexSetCoords.Add(4, 2); //connect to point 2
            indexSetCoords.Add(5, 2); //from point 2
            indexSetCoords.Add(6, 4); //connect to point 4   

            lineGraphPrimitive.CoordinateSet = coordSet;
            lineGraphPrimitive.CoordinateIndexSet = indexSetCoords;


            //Create the color set with two colors
            GraphicsColorSet colorSet = dataSets.CreateColorSet(dataSets.Count + 1); 

            colorSet.Add(1, 221, 0, 0);
            colorSet.Add(2, 255, 170, 0);
            colorSet.Add(3, 119, 187, 17);

            //Create the index set for color
            GraphicsIndexSet indexSetColors = dataSets.CreateIndexSet(dataSets.Count + 1);

            indexSetColors.Add(1, 3); //line 1 uses color 3
            indexSetColors.Add(2, 1); //line 2 uses color 1
            indexSetColors.Add(3, 2); //line 3 uses color 2

            lineGraphPrimitive.ColorSet = colorSet;
            lineGraphPrimitive.ColorIndexSet = indexSetColors;

            lineGraphPrimitive.ColorBinding = ColorBindingEnum.kPerItemColors;

            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a PointGraphics using custom bitmap image.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void PointGraphicsDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;
            GraphicsDataSets dataSets = null;

            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
                dataSets = doc.GraphicsDataSetsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
                dataSets = doc.GraphicsDataSetsCollection.Add(clientId);
            }

            GraphicsNode node = graphics.AddNode(graphics.Count + 1);

            GraphicsCoordinateSet coordSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);

            double[] coords = new double[]
            {
                5.0, 0.0, 0.0
            };

            coordSet.PutCoordinates(ref coords);

            GraphicsImageSet imageSet = dataSets.CreateImageSet(dataSets.Count + 1);

            stdole.IPictureDisp image = 
                PictureDispConverter.ToIPictureDisp(Resources.PointImage);
           
            imageSet.Add(1, image, null, -1, -1);

            PointGraphics pointGraphPrimitive = node.AddPointGraphics();

            pointGraphPrimitive.CoordinateSet = coordSet;
            pointGraphPrimitive.SetCustomImage(imageSet, 1);

            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays PointGraphics with various RenderStyles
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void PointGraphicsRenderDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;
            GraphicsDataSets dataSets = null;

            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
                dataSets = doc.GraphicsDataSetsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
                dataSets = doc.GraphicsDataSetsCollection.Add(clientId);
            }

            TransientGeometry Tg = AdnInventorUtilities.InvApplication.TransientGeometry;


            GraphicsNode node = graphics.AddNode(graphics.Count + 1);

            PointGraphics[] pointGraphics = new PointGraphics[4];

            pointGraphics[0] = node.AddPointGraphics();
            pointGraphics[1] = node.AddPointGraphics();
            pointGraphics[2] = node.AddPointGraphics();
            pointGraphics[3] = node.AddPointGraphics();

            pointGraphics[0].PointRenderStyle = PointRenderStyleEnum.kCirclePointStyle;
            pointGraphics[0].CoordinateSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);
            pointGraphics[0].CoordinateSet.Add(1, Tg.CreatePoint(5, 5, 0));

            pointGraphics[1].PointRenderStyle = PointRenderStyleEnum.kCrossPointStyle;
            pointGraphics[1].CoordinateSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);
            pointGraphics[1].CoordinateSet.Add(1, Tg.CreatePoint(10, 0, 0));

            pointGraphics[2].PointRenderStyle = PointRenderStyleEnum.kXPointStyle;
            pointGraphics[2].CoordinateSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);
            pointGraphics[2].CoordinateSet.Add(1, Tg.CreatePoint(5, -5, 0));

            pointGraphics[3].PointRenderStyle = PointRenderStyleEnum.kFilledCircleSelectPointStyle;
            pointGraphics[3].CoordinateSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);
            pointGraphics[3].CoordinateSet.Add(1, Tg.CreatePoint(0, 0, 0));
            
            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a TextGraphics with different styles and properties.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void TextGraphicsDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;
            GraphicsDataSets dataSets = null;

            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
                dataSets = doc.GraphicsDataSetsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
                dataSets = doc.GraphicsDataSetsCollection.Add(clientId);
            }

            TransientGeometry Tg = AdnInventorUtilities.InvApplication.TransientGeometry;

            GraphicsNode node = graphics.AddNode(graphics.Count + 1);

            //Create scalable text graphics
            TextGraphics scalableTxt = node.AddScalableTextGraphics(); 

            //Set the properties of the text
            scalableTxt.Text = "Scalable Text";
            scalableTxt.Anchor = Tg.CreatePoint(0, 20, 0);
            scalableTxt.Bold = true;
            scalableTxt.Font = "Arial";
            scalableTxt.FontSize = 10;
            scalableTxt.HorizontalAlignment = HorizontalTextAlignmentEnum.kAlignTextLeft;
            scalableTxt.Italic = true;
            scalableTxt.PutTextColor(119, 187, 17);
            scalableTxt.VerticalAlignment = VerticalTextAlignmentEnum.kAlignTextMiddle;


            //Create anchored text graphics
            TextGraphics anchoredTxt = node.AddTextGraphics();

            //Set the properties of the text.
            anchoredTxt.Text = "Anchored Text";
            anchoredTxt.Bold = true;
            anchoredTxt.FontSize = 30;
            anchoredTxt.PutTextColor(255, 170, 0);

            Point anchorPoint = Tg.CreatePoint(1, 1, 1);

            //Set the text's anchor in model space.
            anchoredTxt.Anchor = anchorPoint;

            //Anchor the text graphics in the view.
            anchoredTxt.SetViewSpaceAnchor(
                anchorPoint, 
                Tg.CreatePoint2d(30, 30), 
                ViewLayoutEnum.kTopLeftViewCorner);


            TextGraphics symbolTxt1 = node.AddTextGraphics();
            symbolTxt1.Text = "n ";

            Point modelAnchorPoint = Tg.CreatePoint(50, 0, 0);

            //Because this text will have pixel scaling behavior these coordinates are in pixel space.
            symbolTxt1.Anchor = Tg.CreatePoint(0, 0, 0);
            symbolTxt1.Font = "AIGDT";
            symbolTxt1.FontSize = 25;
            symbolTxt1.HorizontalAlignment = HorizontalTextAlignmentEnum.kAlignTextLeft;
            symbolTxt1.PutTextColor(221, 0, 0);
            symbolTxt1.VerticalAlignment = VerticalTextAlignmentEnum.kAlignTextMiddle;
            symbolTxt1.SetTransformBehavior(
                modelAnchorPoint, 
                DisplayTransformBehaviorEnum.kFrontFacingAndPixelScaling, 
                1);

            Box box = symbolTxt1.RangeBox;
           
            //Draw the next section of the string relative to the first section.
            TextGraphics symbolTxt2 = node.AddTextGraphics();

            symbolTxt2.Text = "9.4 - 9.8";

            //The range of the previous character is used to determine where to position
            //the next string. The range is returned in pixels.
            symbolTxt2.Anchor = Tg.CreatePoint(box.MaxPoint.X, 0, 0);

            symbolTxt2.Font = "Arial";
            symbolTxt2.FontSize = 25;
            symbolTxt2.HorizontalAlignment = HorizontalTextAlignmentEnum.kAlignTextLeft;
            symbolTxt2.PutTextColor(221, 0, 0);
            symbolTxt2.VerticalAlignment = VerticalTextAlignmentEnum.kAlignTextMiddle;
            symbolTxt2.SetTransformBehavior(
                modelAnchorPoint, 
                DisplayTransformBehaviorEnum.kFrontFacingAndPixelScaling, 
                1);

            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a TriangleStrip Graphics.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void TriangleStripGraphicsDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;
            GraphicsDataSets dataSets = null;

            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
                dataSets = doc.GraphicsDataSetsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
                dataSets = doc.GraphicsDataSetsCollection.Add(clientId);
            }

            GraphicsNode node = graphics.AddNode(graphics.Count + 1);

            GraphicsCoordinateSet coordSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);

            double[] coords = new double[]
            {
                0.0, 0.0, 0.0, //point 1
                1.0, 1.0, 0.0, //point 2
                2.0, 0.0, 0.0, //point 3
                3.0, 1.0, 0.0, //point 4
                4.0, 0.0, 0.0, //point 5
                5.0, 1.0, 0.0, //point 6
                6.0, 0.0, 0.0, //point 7
            };

            coordSet.PutCoordinates(ref coords);

            TriangleStripGraphics triStripPrimitive = node.AddTriangleStripGraphics();

            int[] strips = new int[]
            {
                3, //points 1,2,3 for strip 1
                4  //points 4,5,6,7 for strip 2
            };

            triStripPrimitive.PutStripLengths(ref strips);

           
            //Create the color set with two colors
            GraphicsColorSet colorSet = dataSets.CreateColorSet(dataSets.Count + 1); 

            colorSet.Add(1, 221, 0, 0);
            colorSet.Add(2, 119, 187, 17);

            //Create the index set for color
            GraphicsIndexSet indexSetColors = dataSets.CreateIndexSet(dataSets.Count + 1);

            indexSetColors.Add(1, 2); //strip 1 uses color 1
            indexSetColors.Add(2, 1); //strip 2 uses color 2  

            triStripPrimitive.CoordinateSet = coordSet;
            triStripPrimitive.ColorIndexSet = indexSetColors;
            triStripPrimitive.ColorSet = colorSet;
            triStripPrimitive.ColorBinding = ColorBindingEnum.kPerStripColors;

            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a TriangleFan Graphics.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void TriangleFanGraphicsDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;
            GraphicsDataSets dataSets = null;

            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
                dataSets = doc.GraphicsDataSetsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
                dataSets = doc.GraphicsDataSetsCollection.Add(clientId);
            }

            GraphicsNode node = graphics.AddNode(graphics.Count + 1);

            GraphicsCoordinateSet coordSet = dataSets.CreateCoordinateSet(dataSets.Count + 1);

            double[] coords = new double[]
            {
                0.0, 0.0, 0.0, //point 1
                1.0, 1.0, 0.0, //point 2
                2.0, 0.0, 0.0, //point 3
                3.0, 1.0, 0.0, //point 4
                4.0, 0.0, 0.0, //point 5
                5.0, 1.0, 0.0, //point 6
                6.0, 0.0, 0.0, //point 7
            };

            coordSet.PutCoordinates(ref coords);

            TriangleFanGraphics triFanPrimitive = node.AddTriangleFanGraphics();

            int[] strips = new int[]
            {
                3, //points 1,2,3 for strip 1
                4  //points 4,5,6,7 for strip 2
            };

            triFanPrimitive.PutStripLengths(ref strips);

            //Create the color set with 3 colors
            GraphicsColorSet colorSet = dataSets.CreateColorSet(dataSets.Count + 1);

            colorSet.Add(1, 221, 0, 0);
            colorSet.Add(2, 119, 187, 17);
            colorSet.Add(3, 119, 187, 17);

            //Create the index set for color
            GraphicsIndexSet indexSetColors = dataSets.CreateIndexSet(dataSets.Count + 1);

            indexSetColors.Add(1, 2); //strip 1 uses color 1
            indexSetColors.Add(2, 1); //strip 2 uses color 2  

            triFanPrimitive.CoordinateSet = coordSet;
            triFanPrimitive.ColorIndexSet = indexSetColors;
            triFanPrimitive.ColorSet = colorSet;
            triFanPrimitive.ColorBinding = ColorBindingEnum.kPerStripColors;

            doc.Views[1].Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Copies SurfaceBody 1 of current part and performs boolean operation 
        //              to displays result as SurfaceGraphics.
        //////////////////////////////////////////////////////////////////////////////////////////////
        static public void SurfaceGraphicsDemo()
        {
            PartDocument doc =
               AdnInventorUtilities.InvApplication.ActiveDocument
                   as PartDocument;

            string clientId = "{Add-in Guid}";

            ClientGraphics graphics = null;

            try
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection[clientId];
            }
            catch
            {
                graphics = doc.ComponentDefinition.ClientGraphicsCollection.Add(clientId);
            }

            // Store utility objects
            TransientGeometry geom = AdnInventorUtilities.InvApplication.TransientGeometry;
            TransientBRep brep = AdnInventorUtilities.InvApplication.TransientBRep;

            GraphicsNode node = graphics.AddNode(graphics.Count + 1);

            // We will work on the first surface body in our document
            SurfaceBody nativeBody = doc.ComponentDefinition.SurfaceBodies[1];

            // Create a transient copy of the native body to work on it
            SurfaceBody body = brep.Copy(nativeBody);

            // Compute bottom/top points based on body bounding box
            Point bottom = geom.CreatePoint(
                (nativeBody.RangeBox.MinPoint.X + nativeBody.RangeBox.MaxPoint.X)/2,
                (nativeBody.RangeBox.MinPoint.Y + nativeBody.RangeBox.MaxPoint.Y)/2,
                nativeBody.RangeBox.MinPoint.Z);

            Point top = geom.CreatePoint(
                (nativeBody.RangeBox.MinPoint.X + nativeBody.RangeBox.MaxPoint.X)/2,
                (nativeBody.RangeBox.MinPoint.Y + nativeBody.RangeBox.MaxPoint.Y)/2,
                nativeBody.RangeBox.MaxPoint.Z);

            // Create transient cylinder tool body
            double radius = bottom.DistanceTo(top);

            SurfaceBody tool = brep.CreateSolidCylinderCone(bottom, top,  radius, radius, radius, null);

            // Do boolean operation between transient bodies to remove cylinder
            brep.DoBoolean(body, tool, BooleanTypeEnum.kBooleanTypeDifference);

            // Add SurfaceGraphics primitive
            SurfaceGraphics surfGraphPrimitive = node.AddSurfaceGraphics(body);

            // Copy render style of native body if any
            StyleSourceTypeEnum source;
            RenderStyle style = nativeBody.GetRenderStyle(out source);

            node.RenderStyle = style;
            
            // Hide native body
            nativeBody.Visible = false;

            doc.Views[1].Update();
        }
    }
}
