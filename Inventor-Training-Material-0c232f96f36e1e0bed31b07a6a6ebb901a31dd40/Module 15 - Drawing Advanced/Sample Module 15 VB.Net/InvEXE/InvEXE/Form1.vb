Imports Inventor
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Diagnostics


Public Class Form1

    Dim _InvApp As Inventor.Application
    Dim _macros As Macros

    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        Try
            _InvApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")
        Catch ex As Exception
            MessageBox.Show("please open Inventor!")
            Exit Sub
        End Try


        ' Add any initialization after the InitializeComponent() call.
        _macros = New Macros(_InvApp)

        Dim methods As MemberInfo() = _macros.GetType().GetMembers()

        For Each member As MemberInfo In methods
            If (member.DeclaringType.Name = "Macros" And member.MemberType = MemberTypes.Method) Then
                ComboBoxMacros.Items.Add(member.Name)
            End If
        Next

        If ComboBoxMacros.Items.Count > 0 Then
            ComboBoxMacros.SelectedIndex = 0
            Button1.Enabled = True
        End If
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Try

            Dim memberName As String = ComboBoxMacros.SelectedItem.ToString()

            Dim params() As Object = Nothing
            _macros.GetType().InvokeMember(memberName, BindingFlags.InvokeMethod, Nothing, _macros, params, Nothing, Nothing, Nothing)

        Catch ex As Exception

            Dim Caption As String = ex.Message
            Dim Buttons As MessageBoxButtons = MessageBoxButtons.OK
            Dim Result As DialogResult = MessageBox.Show(ex.StackTrace, Caption, Buttons, MessageBoxIcon.Exclamation)

        End Try
    End Sub
End Class


'''macro class
Public Class Macros

    Dim _InvApplication As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        _InvApplication = oApp

    End Sub

    'Small helper function that prompts user for a file selection
    Private Function OpenFile(ByVal StrFilter As String) As String

        Dim filename As String = ""

        Dim ofDlg As System.Windows.Forms.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()

        Dim user As String = System.Windows.Forms.SystemInformation.UserName

        ofDlg.Title = "Open File"
        ofDlg.InitialDirectory = "C:\Documents and Settings\" + user + "\Desktop\"

        ofDlg.Filter = StrFilter 'Example: "Inventor files (*.ipt; *.iam; *.idw)|*.ipt;*.iam;*.idw"
        ofDlg.FilterIndex = 1
        ofDlg.RestoreDirectory = True

        If (ofDlg.ShowDialog() = DialogResult.OK) Then
            filename = ofDlg.FileName
        End If

        OpenFile = filename

    End Function

    Public Sub CreateSketch()
        Dim oDoc As DrawingDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oSheet As Sheet
        oSheet = oDoc.ActiveSheet

        ' Create the sketch.
        Dim oSketch As DrawingSketch
        oSketch = oSheet.Sketches.Add

        ' Open the sketch for edit in the user interface.
        oSketch.Edit()

        oSketch.SketchCircles.AddByCenterRadius( _
                 _InvApplication.TransientGeometry.CreatePoint2d(8, 8), 2)

        ' Exit edit.
        oSketch.ExitEdit()
    End Sub

    Public Sub CreateStyles()

        Dim oDoc As DrawingDocument
        oDoc = _InvApplication.ActiveDocument

        'get Drawing Styles Manager
        Dim oDStylesMan As DrawingStylesManager
        oDStylesMan = oDoc.StylesManager

        ' create a new text style 
        ' by copying from an existing style
        'the new name is "MyNewTextStyle"
        Dim oNewTextStyle As TextStyle
        oNewTextStyle = oDStylesMan.TextStyles("Label Text (ANSI)").Copy("MyNewTextStyle")

        ' change some properties of the new style
        oNewTextStyle.FontSize *= 2
        oNewTextStyle.Italic = True


        Dim oNewLeaderStyle As LeaderStyle
        oNewLeaderStyle = oDStylesMan.LeaderStyles(1).Copy("MyNewLeaderStyle")

        oNewLeaderStyle.Color = _InvApplication.TransientObjects.CreateColor(255, 0, 0)
        oNewLeaderStyle.LineWeight *= 2

        Dim oNewDrawingStyle As DrawingStandardStyle
        oNewDrawingStyle = oDStylesMan.StandardStyles(1).Copy("MyNewDSStyle")

        'change some general settings
        oNewDrawingStyle.LinearUnits = UnitsTypeEnum.kCentimeterLengthUnits

        Dim oNewObjDefaultStyle As ObjectDefaultsStyle
        oNewObjDefaultStyle = oDStylesMan.ObjectDefaultsStyles(1).Copy("MyNewObjStyles")

        'change some properties
        ' e.g. the border text style uses the MyNewTextStyle
        oNewObjDefaultStyle.BorderTextStyle = oNewTextStyle

        'new standard style uses the new objects styles
        oNewDrawingStyle.ActiveObjectDefaults = oNewObjDefaultStyle

        ' the document uses the new DrawingStandardStyle
        oDStylesMan.ActiveStandardStyle = oNewDrawingStyle

    End Sub

    ''' <summary>
    ''' This sample demonstrates the creation of a baseline set dimension in a drawing. 
    ''' Create a drawing view and select multiple edges in the view before running the sample.  
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateBaselineDimensionSet()

        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Set a reference to the active sheet.
        Dim oActiveSheet As Sheet
        oActiveSheet = oDrawDoc.ActiveSheet

        Dim oIntentCollection As ObjectCollection
        oIntentCollection = _InvApplication.TransientObjects.CreateObjectCollection

        ' Get all the selected drawing curve segments.
        Dim oDrawingCurveSegment As DrawingCurveSegment
        Dim oDrawingCurve As DrawingCurve
        For Each oDrawingCurveSegment In oDrawDoc.SelectSet

            ' Set a reference to the drawing curve.
            oDrawingCurve = oDrawingCurveSegment.Parent

            Dim oDimIntent As GeometryIntent
            oDimIntent = oActiveSheet.CreateGeometryIntent(oDrawingCurve)

            Call oIntentCollection.Add(oDimIntent)
        Next

        ' Set a reference to the view to which the curve belongs.
        Dim oDrawingView As DrawingView
        oDrawingView = oDrawingCurve.Parent

        ' Set a reference to the baseline dimension sets collection.
        Dim oBaselineSets As BaselineDimensionSets
        oBaselineSets = oActiveSheet.DrawingDimensions.BaselineDimensionSets

        ' Determine the placement point
        Dim oPlacementPoint As Point2d
        oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(oDrawingView.Left - 5, oDrawingView.Center.Y)

        ' Create a vertical baseline set dimension.
        Dim oBaselineSet As BaselineDimensionSet
        oBaselineSet = oBaselineSets.Add(oIntentCollection, oPlacementPoint, DimensionTypeEnum.kHorizontalDimensionType)

    End Sub

    ''' <summary>
    ''' creation of a balloon. Select a linear drawing curve and run the sample
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateBalloon()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Set a reference to the active sheet.
        Dim oActiveSheet As Sheet
        oActiveSheet = oDrawDoc.ActiveSheet

        ' Set a reference to the drawing curve segment.
        ' This assumes that a drwaing curve is selected.
        Dim oDrawingCurveSegment As DrawingCurveSegment
        oDrawingCurveSegment = oDrawDoc.SelectSet.Item(1)

        ' Set a reference to the drawing curve.
        Dim oDrawingCurve As DrawingCurve
        oDrawingCurve = oDrawingCurveSegment.Parent

        ' Get the mid point of the selected curve
        ' assuming that the selection curve is linear
        Dim oMidPoint As Point2d
        oMidPoint = oDrawingCurve.MidPoint

        ' Set a reference to the TransientGeometry object.
        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oLeaderPoints As ObjectCollection
        oLeaderPoints = _InvApplication.TransientObjects.CreateObjectCollection

        ' Create a couple of leader points.
        Call oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 10))
        Call oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 5))

        ' Add the GeometryIntent to the leader points collection.
        ' This is the geometry that the balloon will attach to.
        Dim oGeometryIntent As GeometryIntent
        oGeometryIntent = oActiveSheet.CreateGeometryIntent(oDrawingCurve)
        Call oLeaderPoints.Add(oGeometryIntent)

        ' Set a reference to the parent drawing view of the selected curve
        Dim oDrawingView As DrawingView
        oDrawingView = oDrawingCurve.Parent

        ' Set a reference to the referenced model document
        Dim oModelDoc As Document
        oModelDoc = oDrawingView.ReferencedDocumentDescriptor.ReferencedDocument

        ' Check if a partslist or a balloon has already been created for this model
        Dim IsDrawingBOMDefined As Boolean
        IsDrawingBOMDefined = oDrawDoc.DrawingBOMs.IsDrawingBOMDefined(oModelDoc.FullFileName)

        Dim oBalloon As Balloon

        If IsDrawingBOMDefined Then

            ' Just create the balloon with the leader points
            ' All other arguments can be ignored
            oBalloon = oDrawDoc.ActiveSheet.Balloons.Add(oLeaderPoints)
        Else

            ' First check if the 'structured' BOM view has been enabled in the model

            ' Set a reference to the model's BOM object
            Dim oBOM As BOM
            oBOM = oModelDoc.ComponentDefinition.BOM

            If oBOM.StructuredViewEnabled Then

                ' Level needs to be specified
                ' Numbering options have already been defined
                ' Get the Level ('All levels' or 'First level only')
                ' from the model BOM view - must use the same here
                Dim Level As PartsListLevelEnum
                If oBOM.StructuredViewFirstLevelOnly Then
                    Level = PartsListLevelEnum.kStructured
                Else
                    Level = PartsListLevelEnum.kStructuredAllLevels
                End If

                ' Create the balloon by specifying just the level
                oBalloon = oActiveSheet.Balloons.Add(oLeaderPoints, , Level)
            Else

                ' Level and numbering options must be specified
                ' The corresponding model BOM view will automatically be enabled
                Dim oNumberingScheme As NameValueMap
                oNumberingScheme = _InvApplication.TransientObjects.CreateNameValueMap

                ' Add the option for a comma delimiter
                oNumberingScheme.Add("Delimiter", ",")

                ' Create the balloon by specifying the level and numbering scheme
                oBalloon = oActiveSheet.Balloons.Add(oLeaderPoints, , PartsListLevelEnum.kStructuredAllLevels, oNumberingScheme)
            End If
        End If
    End Sub

    ''' <summary>
    '''  creation of a parts list. The parts list is placed at the
    '''  top right corner of the border if one exists, else it is placed
    '''  at the top right corner of the sheet. 
    ''' To run this sample, have a drawing document open. 
    ''' The active sheet in the drawing should have at least 
    ''' one drawing view and the first drawing view on the sheet 
    ''' should not be a draft view. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreatePartsList()

        On Error Resume Next

        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        'Set a reference to the active sheet.
        Dim oSheet As Sheet
        oSheet = oDrawDoc.ActiveSheet

        ' Set a reference to the first drawing view on
        ' the sheet. This assumes the first drawing
        ' view on the sheet is not a draft view.
        Dim oDrawingView As DrawingView
        oDrawingView = oSheet.DrawingViews(1)

        ' Set a reference to th sheet's border
        Dim oBorder As Border
        oBorder = oSheet.Border

        Dim oPlacementPoint As Point2d

        If Not oBorder Is Nothing Then
            ' A border exists. The placement point
            ' is the top-right corner of the border.
            oPlacementPoint = oBorder.RangeBox.MaxPoint
        Else
            ' There is no border. The placement point
            ' is the top-right corner of the sheet.
            oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(oSheet.Width, oSheet.Height)
        End If

        ' Create the parts list.
        Dim oPartsList As PartsList
        oPartsList = oSheet.PartsLists.Add(oDrawingView, oPlacementPoint)

    End Sub

    ''' <summary>
    '''  how to create a custom table
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateCustomTable()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Set a reference to the active sheet.
        Dim oSheet As Sheet
        oSheet = oDrawDoc.ActiveSheet

        ' Set the column titles
        Dim oTitles() As String =
                New String() {"Part Number", "Quantity", "Material"}

        

        ' Set the contents of the custom table (contents are set row-wise)
        Dim oContents() As String =
                  New String() {"1",
                                "1",
                            "Brass",
                             "2",
                             "2",
                              "Aluminium",
                              "3",
                              "1",
                            "Steel"}

        ' Set the column widths (defaults to the column title width if not specified)
        Dim oColumnWidths() As Double =
                    New Double() {2.5, 2.5, 4}

        ' Create the custom table
        Dim oCustomTable As CustomTable
        oCustomTable = oSheet.CustomTables.Add("My Table", _InvApplication.TransientGeometry.CreatePoint2d(15, 15), _
                                            3, 3, oTitles, oContents, oColumnWidths)

        ' Change the 3rd column to be left justified.
        oCustomTable.Columns.Item(3).ValueHorizontalJustification =
                HorizontalTextAlignmentEnum.kAlignTextLeft

        ' Create a table format object
        Dim oFormat As TableFormat
        oFormat = oSheet.CustomTables.CreateTableFormat

        ' Set inside line color to red.
        oFormat.InsideLineColor = _InvApplication.TransientObjects.CreateColor(255, 0, 0)

        ' Set outside line weight.
        oFormat.OutsideLineWeight = 0.1

        ' Modify the table formats
        oCustomTable.OverrideFormat = oFormat
    End Sub

    ''' <summary>
    ''' sample illustrates querying the contents of the revision table. 
    ''' and how to add a row
    ''' To run this sample have a sheet active that contains a revision table
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RevisionTableQuery()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Set a reference to the first revision table on the active sheet.
        ' This assumes that a revision table is on the active sheet.
        Dim oRevTable As RevisionTable
        oRevTable = oDrawDoc.ActiveSheet.RevisionTables.Item(1)

        ' Iterate through the contents of the revision table.
        Dim i As Long

        For i = 1 To oRevTable.RevisionTableRows.Count
            ' Get the current row.
            Dim oRow As RevisionTableRow
            oRow = oRevTable.RevisionTableRows.Item(i)

            ' Iterate through each column in the row.
            Dim j As Long
            For j = 1 To oRevTable.RevisionTableColumns.Count
                ' Get the current cell.
                Dim oCell As RevisionTableCell
                oCell = oRow.Item(j)

                ' Display the value of the current cell.
                Debug.Print("Row: " & i & ", Column: " & oRevTable.RevisionTableColumns.Item(j).Title & " = " & oCell.Text)



            Next
        Next

        'add a new row
        Dim oNewRow As RevisionTableRow
        oNewRow = oRevTable.RevisionTableRows.Add()


    End Sub

    ''' <summary>
    '''  creation of hole tables in a drawing. 
    ''' Select a drawing view that contains holes and run the following sample
    ''' </summary>
    ''' <remarks></remarks>
    Sub CreateHoleTables()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Set a reference to the active sheet.
        Dim oActiveSheet As Sheet
        oActiveSheet = oDrawDoc.ActiveSheet

        ' Set a reference to the drawing view.
        ' This assumes that a drawing view is selected.
        Dim oDrawingView As DrawingView
        oDrawingView = oDrawDoc.SelectSet.Item(1)

        ' Create origin indicator if it has not been already created.
        If Not oDrawingView.HasOriginIndicator Then
            ' Create point intent to anchor the origin to.
            Dim oDimIntent As GeometryIntent
            Dim oPointIntent As Point2d

            ' Get the first curve on the view
            Dim oCurve As DrawingCurve
            oCurve = oDrawingView.DrawingCurves.Item(1)

            ' Check if it has a strt point
            oPointIntent = oCurve.StartPoint

            If oPointIntent Is Nothing Then
                ' Else use the center point
                oPointIntent = oCurve.CenterPoint
            End If

            oDimIntent = oActiveSheet.CreateGeometryIntent(oCurve, oPointIntent)

            oDrawingView.CreateOriginIndicator(oDimIntent)
        End If

        Dim oPlacementPoint As Point2d

        ' Set a reference to th sheet's border
        Dim oBorder As Border
        oBorder = oActiveSheet.Border

        If Not oBorder Is Nothing Then
            ' A border exists. The placement point
            ' is the top-left corner of the border.
            oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(oBorder.RangeBox.MinPoint.X, oBorder.RangeBox.MaxPoint.Y)
        Else
            ' There is no border. The placement point
            ' is the top-left corner of the sheet.
            oPlacementPoint = _InvApplication.TransientGeometry.CreatePoint2d(0, oActiveSheet.Height)
        End If

        ' Create a 'view' hole table
        ' This hole table includes all holes as specified by the active hole table style
        Dim oViewHoleTable As HoleTable
        oViewHoleTable = oActiveSheet.HoleTables.Add(oDrawingView, oPlacementPoint)

        oPlacementPoint.X = oActiveSheet.width / 2

        ' Create a 'feature type' hole table
        ' This hole table includes specified hole types only
        Dim oFeatureHoleTable As HoleTable
        oFeatureHoleTable = oActiveSheet.HoleTables.AddByFeatureType(oDrawingView, oPlacementPoint, _
                True, True, True, True, False, False, False)

        'add a new row

        ' get the model document
        Dim oModelDoc As Document
        oModelDoc = oDrawingView.ReferencedDocumentDescriptor.ReferencedDocument

        Dim oHoleF As HoleFeature
        If oModelDoc.DocumentType = DocumentTypeEnum.kAssemblyDocumentObject Then
            Dim oAssDef As AssemblyComponentDefinition
            oAssDef = oModelDoc.ComponentDefinition

            If oAssDef.Features.HoleFeatures.Count > 0 Then
                'as a demo: get the first hole feature
                oHoleF = oAssDef.Features.HoleFeatures(1)
            End If
        ElseIf oModelDoc.DocumentType = DocumentTypeEnum.kPartDocumentObject Then
            Dim oPartDef As PartComponentDefinition
            oPartDef = oModelDoc.ComponentDefinition

            If oPartDef.Features.HoleFeatures.Count > 0 Then
                'as a demo: get the first hole feature
                oHoleF = oPartDef.Features.HoleFeatures(1)
            End If
        End If


        ' add a new row to the hole table
        If Not oHoleF Is Nothing Then
            Dim oHoleCurves As DrawingCurvesEnumerator
            oHoleCurves = oDrawingView.DrawingCurves(oHoleF)
            If oHoleCurves.Count > 0 Then
                oFeatureHoleTable.HoleTableRows.Add(oHoleCurves(1))
            End If

        End If
    End Sub

    ''' <summary>
    '''  creating leader text on a sheet. 
    ''' Before running this sample, open a drawing document and select a linear drawing edge. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub AddLeaderNote()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Set a reference to the active sheet.
        Dim oActiveSheet As Sheet
        oActiveSheet = oDrawDoc.ActiveSheet

        ' Set a reference to the drawing curve segment.
        ' This assumes that a drawing curve is selected.
        Dim oDrawingCurveSegment As DrawingCurveSegment
        oDrawingCurveSegment = oDrawDoc.SelectSet.Item(1)

        ' Set a reference to the drawing curve.
        Dim oDrawingCurve As DrawingCurve
        oDrawingCurve = oDrawingCurveSegment.Parent

        ' Get the mid point of the selected curve
        ' assuming that the selected curve is linear
        Dim oMidPoint As Point2d
        oMidPoint = oDrawingCurve.MidPoint

        ' Set a reference to the TransientGeometry object.
        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oLeaderPoints As ObjectCollection
        oLeaderPoints = _InvApplication.TransientObjects.CreateObjectCollection

        ' Create a few leader points.
        Call oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 10))
        Call oLeaderPoints.Add(oTG.CreatePoint2d(oMidPoint.X + 10, oMidPoint.Y + 5))

        ' Create an intent and add to the leader points collection.
        ' This is the geometry that the leader text will attach to.
        Dim oGeometryIntent As GeometryIntent
        oGeometryIntent = oActiveSheet.CreateGeometryIntent(oDrawingCurve)

        Call oLeaderPoints.Add(oGeometryIntent)

        ' Create text with simple string as input. Since this doesn't use
        ' any text overrides, it will default to the active text style.
        Dim sText As String
        sText = "API Leader Note"

        Dim oLeaderNote As LeaderNote
        oLeaderNote = oActiveSheet.DrawingNotes.LeaderNotes.Add(oLeaderPoints, sText)

        ' Insert a node.
        Dim oFirstNode As LeaderNode
        oFirstNode = oLeaderNote.Leader.RootNode.ChildNodes.Item(1)

        Dim oSecondNode As LeaderNode
        oSecondNode = oFirstNode.ChildNodes.Item(1)

        Call oFirstNode.InsertNode(oSecondNode, oTG.CreatePoint2d(oMidPoint.X + 5, oMidPoint.Y + 5))
    End Sub

#Region "Sketched Symbol demos"
    'creating a new sketched symbol definition object and 
    '    inserting it into the active sheet. 
    '    This sample consists of two subs. 
    '    The first demonstrates the creation of a sketched symbol definition and 
    '    the second inserts it into the active sheet. 
    '    To run the sample have a drawing document open and run the CreateSketchedSymbolDefinition Sub.
    '    After this you can run the InsertSketchedSymbolOnSheet to insert the sketched symbol into the active sheet. 
    '    The insertion sub demonstrates the use of the insertion point in the symbol's definition while inserting the symbol. 

    ''' <summary>
    '''  creating a new sketched symbol definition object     '''   
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateSketchedSymbolDefinition()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Create the new sketched symbol definition.
        Dim oSketchedSymbolDef As SketchedSymbolDefinition
        oSketchedSymbolDef = oDrawDoc.SketchedSymbolDefinitions.Add("Circular Callout")

        ' Open the sketched symbol definition's sketch for edit. This is done by calling the Edit
        ' method of the SketchedSymbolDefinition to obtain a DrawingSketch. This actually creates
        ' a copy of the sketched symbol definition's and opens it for edit.
        Dim oSketch As DrawingSketch
        Call oSketchedSymbolDef.Edit(oSketch)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Use the functionality of the sketch to add sketched symbol graphics.
        Dim oSketchLine As SketchLine
        oSketchLine = oSketch.SketchLines.AddByTwoPoints(oTG.CreatePoint2d(0, 0), oTG.CreatePoint2d(20, 0))

        Dim oSketchCircle As SketchCircle
        oSketchCircle = oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(22, 0), 2)

        Call oSketch.GeometricConstraints.AddCoincident(oSketchLine.EndSketchPoint, oSketchCircle)

        ' Make the starting point of the sketch line the insertion point
        oSketchLine.StartSketchPoint.InsertionPoint = True

        ' Add a prompted text field at the center of the sketch circle.
        Dim sText As String
        sText = "<Prompt>Enter text 1</Prompt>"
        Dim oTextBox As TextBox
        oTextBox = oSketch.TextBoxes.AddFitted(oTG.CreatePoint2d(22, 0), sText)
        oTextBox.VerticalJustification = VerticalTextAlignmentEnum.kAlignTextMiddle
        oTextBox.HorizontalJustification = HorizontalTextAlignmentEnum.kAlignTextCenter

        Call oSketchedSymbolDef.ExitEdit(True)
    End Sub

    ''' <summary>
    ''' insert a sketched symbol by the definition above
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub InsertSketchedSymbolOnSheet()
        ' Set a reference to the drawing document.
        ' This assumes a drawing document is active.
        Dim oDrawDoc As DrawingDocument
        oDrawDoc = _InvApplication.ActiveDocument

        ' Obtain a reference to the desired sketched symbol definition.
        Dim oSketchedSymbolDef As SketchedSymbolDefinition
        oSketchedSymbolDef = oDrawDoc.SketchedSymbolDefinitions.Item("Circular Callout")

        Dim oSheet As Sheet
        oSheet = oDrawDoc.ActiveSheet

        ' This sketched symbol definition contains one prompted string input. An array
        ' must be input that contains the strings for the prompted strings.
        Dim sPromptStrings(0) As String
        sPromptStrings(0) = "A"

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Add an instance of the sketched symbol definition to the sheet.
        ' Rotate the instance by 45 degrees and scale by .75 when adding.
        ' The symbol will be inserted at (0,0) on the sheet. Since the
        ' start point of the line was marked as the insertion point, the
        ' start point should end up at (0,0).
        Dim oSketchedSymbol As SketchedSymbol
        oSketchedSymbol = oSheet.SketchedSymbols.Add(oSketchedSymbolDef, oTG.CreatePoint2d(0, 0), (3.14159 / 4), 0.75, sPromptStrings)
    End Sub
#End Region


End Class
