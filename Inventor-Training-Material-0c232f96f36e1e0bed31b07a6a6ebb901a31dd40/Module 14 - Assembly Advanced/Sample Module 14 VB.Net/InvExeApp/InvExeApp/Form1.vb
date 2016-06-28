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

    Private Sub ComboBoxMacros_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ComboBoxMacros.SelectedIndexChanged

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

    ''' <summary>
    ''' Add iMate definitions using AddMateiMateDefinition and AddInsertiMateDefinition.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateiMateDefinition()

        ' Create a new part document, using the default part template.
        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                    _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject))

        ' Set a reference to the component definition.
        Dim oCompDef As PartComponentDefinition
        oCompDef = oPartDoc.ComponentDefinition

        ' Create a new sketch on the X-Y work plane.
        Dim oSketch As PlanarSketch
        oSketch = oCompDef.Sketches.Add(oCompDef.WorkPlanes(3))

        ' Set a reference to the transient geometry object.
        Dim oTransGeom As TransientGeometry
        oTransGeom = _InvApplication.TransientGeometry

        ' Draw a 4cm x 3cm rectangle with the corner at (0,0)
        Dim oRectangleLines As SketchEntitiesEnumerator
        oRectangleLines = oSketch.SketchLines.AddAsTwoPointRectangle( _
                                    oTransGeom.CreatePoint2d(0, 0), _
                                    oTransGeom.CreatePoint2d(4, 3))

        ' Create a profile.
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        ' Create a base extrusion 1cm thick.
        Dim oExtrudeDef As ExtrudeDefinition
        oExtrudeDef = oCompDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kNewBodyOperation)
        Call oExtrudeDef.SetDistanceExtent(1, PartFeatureExtentDirectionEnum.kNegativeExtentDirection)
        Dim oExtrude1 As ExtrudeFeature
        oExtrude1 = oCompDef.Features.ExtrudeFeatures.Add(oExtrudeDef)

        ' Get the top face of the extrusion to use for creating the new sketch.
        Dim oFrontFace As Face
        oFrontFace = oExtrude1.StartFaces.Item(1)

        ' Create a new sketch on this face, but use the method that allows you to
        ' control the orientation and orgin of the new sketch.
        oSketch = oCompDef.Sketches.AddWithOrientation(oFrontFace, _
                        oCompDef.WorkAxes.Item(1), True, True, oCompDef.WorkPoints(1))

        ' Create a sketch circle with the center at (2, 1.5).
        Dim oCircle As SketchCircle
        oCircle = oSketch.SketchCircles.AddByCenterRadius(oTransGeom.CreatePoint2d(2, 1.5), 0.5)

        ' Create a profile.
        oProfile = oSketch.Profiles.AddForSolid

        ' Create the second extrude (a hole).
        Dim oExtrude2 As ExtrudeFeature
        oExtrude2 = oCompDef.Features.ExtrudeFeatures.AddByThroughAllExtent( _
                            oProfile, PartFeatureExtentDirectionEnum.kNegativeExtentDirection, PartFeatureOperationEnum.kCutOperation)

        ' Create a mate iMateDefinition on a side face of the first extrude.
        Dim oMateiMateDefinition As MateiMateDefinition
        oMateiMateDefinition = oCompDef.iMateDefinitions.AddMateiMateDefinition( _
                            oExtrude1.SideFaces.Item(1), 0, , , "MateA")

        ' Create a match list of names to use for the next iMateDefinition.
        Dim strMatchList(2) As String
        strMatchList(0) = "InsertA"
        strMatchList(1) = "InsertB"
        strMatchList(2) = "InsertC"

        ' Create an insert iMateDefinition on the cylindrical face of the second extrude.
        Dim oInsertiMateDefinition As InsertiMateDefinition
        oInsertiMateDefinition = oCompDef.iMateDefinitions.AddInsertiMateDefinition( _
                            oExtrude2.SideFaces.Item(1), False, 0, , "InsertA", strMatchList)
    End Sub

    Public Sub iMateResultCreation()
        ' Get the component definition of the currently open assembly.
        ' This will fail if an assembly document is not open.
        Dim oAsmCompDef As AssemblyComponentDefinition
        oAsmCompDef = _InvApplication.ActiveDocument.ComponentDefinition

        ' Create a new matrix object.  It will be initialized to an identity matrix.
        Dim oMatrix As Matrix
        oMatrix = _InvApplication.TransientGeometry.CreateMatrix

        ' Place the first occurrence.
        Dim oOcc1 As ComponentOccurrence
        oOcc1 = oAsmCompDef.Occurrences.Add("C:\\Temp\\iMatePart.ipt", oMatrix)

        ' Place the second occurrence, but adjust the matrix slightly so they're
        ' not right on top of each other.
        oMatrix.Cell(1, 4) = 10
        Dim oOcc2 As ComponentOccurrence
        oOcc2 = oAsmCompDef.Occurrences.Add("C:\\Temp\\iMatePart.ipt", oMatrix)

        ' Look through the iMateDefinitions defined for the first occurrence
        ' and find the one named "iMate:1".  This loop demonstrates using the
        ' Count and Item properties of the iMateDefinitions object.
        Dim i As Long
        Dim oiMateDef1 As iMateDefinition
        For i = 1 To oOcc1.iMateDefinitions.Count
            If oOcc1.iMateDefinitions.Item(i).Name = "iMate:1" Then
                oiMateDef1 = oOcc1.iMateDefinitions.Item(i)
                Exit For
            End If
        Next

        If oiMateDef1 Is Nothing Then
            MsgBox("An iMate definition named ""iMate:1"" does not exist in " & oOcc1.Name)
            Exit Sub
        End If

        ' Look through the iMateDefinitions defined for the second occurrence
        ' and find the one named "iMate:1".  This loop demonstrates using the
        ' For Each method of iterating through a collection.
        Dim oiMateDef2 As iMateDefinition
        Dim bFoundDefinition As Boolean
        For Each oiMateDef2 In oOcc2.iMateDefinitions
            If oiMateDef2.Name = "iMate:1" Then
                bFoundDefinition = True
                Exit For
            End If
        Next

        If Not bFoundDefinition Then
            MsgBox("An iMate definition named ""iMate:1"" does not exist in " & oOcc2.Name)
            Exit Sub
        End If

        ' Create an iMate result using the two definitions.
        Dim oiMateResult As iMateResult
        oiMateResult = oAsmCompDef.iMateResults.AddByTwoiMates(oiMateDef1, oiMateDef2)

    End Sub


    Public Sub AssemblyFeature()

        Dim oAsmDef As AssemblyComponentDefinition
        oAsmDef = _InvApplication.ActiveDocument.ComponentDefinition

        ' Create a sketch on the XY workplane.
        Dim oSketch As PlanarSketch
        oSketch = oAsmDef.Sketches.Add(oAsmDef.WorkPlanes.Item(3))

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Draw a rectangle.
        oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(0.1, 0.1), _
                                                        oTG.CreatePoint2d(1, 0.5))
        ' Create a profile
        Dim oProfile As Profile
        oProfile = oSketch.Profiles.AddForSolid

        ' Create the extrusion.
        Dim oExtrudeDef As ExtrudeDefinition
        oExtrudeDef = oAsmDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile, PartFeatureOperationEnum.kCutOperation)
        oExtrudeDef.SetDistanceExtent("2 cm", PartFeatureExtentDirectionEnum.kSymmetricExtentDirection)
         
        oAsmDef.Features.ExtrudeFeatures.Add(oExtrudeDef)

    End Sub

    Public Sub AddAssemblyBrowserFolder()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oDef As AssemblyComponentDefinition
        oDef = oDoc.ComponentDefinition

        Dim oPane As BrowserPane
        oPane = oDoc.BrowserPanes.ActivePane

        ' Create an object collection
        Dim oOccurrenceNodes As ObjectCollection
        oOccurrenceNodes = _InvApplication.TransientObjects.CreateObjectCollection

        Dim oOcc As ComponentOccurrence
        For Each oOcc In oDef.Occurrences

            ' Get the node associated with this occurrence.
            Dim oNode As BrowserNode
            oNode = oPane.GetBrowserNodeFromObject(oOcc)

            ' Add the node to the collection.
            oOccurrenceNodes.Add(oNode)
        Next

        ' Add the folder to the browser and specify the nodes to be grouped within it.
        Dim oFolder As BrowserFolder
        oFolder = oPane.AddBrowserFolder("My Occurrences Folder", oOccurrenceNodes)

    End Sub


    Public Sub BomAccess()

        Dim oAsm As AssemblyDocument
        oAsm = _InvApplication.ActiveDocument

        Dim oBOM As BOM
        oBOM = oAsm.ComponentDefinition.BOM

        oBOM.StructuredViewEnabled = True

        Dim oBomView As BOMView
        oBomView = oBOM.BOMViews("Structured")

        Dim rowIdx As Long
        For rowIdx = 1 To oBomView.BOMRows.Count

            Dim oRow As BOMRow
            oRow = oBomView.BOMRows(rowIdx)

            Debug.Print("ItemNumber: " & oRow.ItemNumber & " TotalQuantity = " & oRow.TotalQuantity)

            Dim oCompDef As ComponentDefinition
            oCompDef = oRow.ComponentDefinitions(1)

            Dim oDesignPropSet As PropertySet
            oDesignPropSet = oCompDef.Document.PropertySets("Design Tracking Properties")

        Next

    End Sub

    Public Sub ExportBOM()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oBOM As BOM
        oBOM = oDoc.ComponentDefinition.BOM

        ' Set the structured view to 'all levels'
        oBOM.StructuredViewFirstLevelOnly = False

        ' Make sure that the structured view is enabled.
        oBOM.StructuredViewEnabled = True

        ' Set a reference to the "Structured" BOMView
        Dim oStructuredBOMView As BOMView
        oStructuredBOMView = oBOM.BOMViews.item("Structured")

        ' Export the BOM view to an Excel file
        oStructuredBOMView.Export("C:\Temp\BOM-StructuredAllLevels.xls", FileFormatEnum.kMicrosoftExcelFormat)

    End Sub

    Public Sub AddiAssemblyOccurrence()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oOccurrences As ComponentOccurrences
        oOccurrences = oDoc.ComponentDefinition.Occurrences

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oPos As Matrix
        oPos = oTG.CreateMatrix

        Dim oNewOcc As ComponentOccurrence

        'Row specified either by a Long (row index), String (MemberName), or iAssemblyTableRow.
        oNewOcc = oOccurrences.AddiAssemblyMember("C:\MyiAsm.iam", oPos, 1)
        oNewOcc = oOccurrences.AddiAssemblyMember("C:\MyiAsm.iam", oPos, "MemberName")

    End Sub


    Public Sub DesignViewSample()

        Dim oAsmDef As AssemblyComponentDefinition
        oAsmDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oDesignViewReps As DesignViewRepresentations
        oDesignViewReps = oAsmDef.RepresentationsManager.DesignViewRepresentations

        ' Use ComponentOccurrence functionality to set the state, (visibility, color, etc.).  
        ' When the design view is created it will capture the current state of the assembly.

        Dim oDesignViewRep As DesignViewRepresentation
        oDesignViewRep = oDesignViewReps.Add("Test")

        ' Activate the master design view.
        oDesignViewReps.Item("Master").Activate()

    End Sub

    Public Sub PositionalRepSample()

        Dim oAsmDoc As AssemblyDocument
        oAsmDoc = _InvApplication.ActiveDocument

        Dim oAsmDef As AssemblyComponentDefinition
        oAsmDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oPositionalReps As PositionalRepresentations
        oPositionalReps = oAsmDef.RepresentationsManager.PositionalRepresentations

        ' Create a new position representation.
        Dim oPosRep As PositionalRepresentation
        oPosRep = oPositionalReps.Add("New Test")

        ' Get a constraint and override it's value.
        Dim oConstraint As AssemblyConstraint
        oConstraint = oAsmDoc.ComponentDefinition.Constraints.Item(1)
        Call oPosRep.SetConstraintValueOverride(oConstraint, "1 in")

    End Sub

    Public Sub LevelOfDetail()

        Dim oAsmDef As AssemblyComponentDefinition
        oAsmDef = _InvApplication.ActiveDocument.ComponentDefinition

        Dim oLODReps As LevelOfDetailRepresentations
        oLODReps = oAsmDef.RepresentationsManager.LevelOfDetailRepresentations

        ' Create a new level of detail.
        Dim oLODRep As LevelOfDetailRepresentation
        oLODRep = oLODReps.Add("My LOD")

        ' Suppress an occurrence.
        oAsmDef.Occurrences.Item(1).Suppress()

        ' Save the document, which is really saving the LOD.
        _InvApplication.ActiveDocument.Save()

        ' Activate the master LOD.
        oAsmDef.RepresentationsManager.LevelOfDetailRepresentations.Item("Master").Activate()
        

    End Sub

    Public Sub BOMfromLoD()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oAsmDocMasterLOD As AssemblyDocument
        oAsmDocMasterLOD = _InvApplication.Documents.Open(oDoc.File.FullFileName, False)

        'Obtains BOM only from Master LOD
        Dim oBOM As BOM
        oBOM = oAsmDocMasterLOD.ComponentDefinition.BOM

        'From here you can operate on the BOM object...
        'Following lines of code are examples only

        oBOM.StructuredViewFirstLevelOnly = True
        oBOM.StructuredViewEnabled = True

    End Sub


    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample 
    '//
    '// Use: Copy & Replace assembly references (1st Level refs only)
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub ReplaceReference()

        Dim oAsm As AssemblyDocument
        oAsm = _InvApplication.ActiveDocument

        Dim oNewRefDoc As Document
        Dim filename As String

        Dim oFileDesc As FileDescriptor
        For Each oFileDesc In oAsm.File.ReferencedFileDescriptors

            oNewRefDoc = oFileDesc.ReferencedFile.AvailableDocuments(1)

            filename = "C:\Temp\Copy-" + oNewRefDoc.DisplayName

            Call oNewRefDoc.SaveAs(filename, True)

            Call oFileDesc.ReplaceReference(filename)

        Next

        filename = "C:\Temp\Copy-" + oAsm.DisplayName
        Call oAsm.SaveAs(filename, True)

        Call oAsm.Close(True)

    End Sub

#Region "Apprentice demo"


    Dim mApprenticeApp As Inventor.ApprenticeServerComponent = _
            New ApprenticeServerComponent()
    Private Sub SaveWithApprentice()
        Dim NewFolder As String = "C:\Temp\"
        Dim AsmFullFilename As String = _
 "C:\Program Files\Autodesk\Inventor 2013\Samples\Models\Tube & Pipe\Tank\Tank.iam"

        Dim oApprenticeDoc As Inventor.ApprenticeServerDocument = _
 mApprenticeApp.Open(AsmFullFilename)

        SaveRec(NewFolder, oApprenticeDoc)
        mApprenticeApp.FileSaveAs.ExecuteSaveCopyAs()
        oApprenticeDoc.Close()

    End Sub

    Private Function newFileName(ByVal FullFileName As String) As String
        Dim fileInfo As New IO.FileInfo(FullFileName)
        Return fileInfo.Name.Substring(0, fileInfo.Name.Length - 4) + "_new" + fileInfo.Extension
    End Function

    Private Sub SaveRec(ByRef NewFolder As String, ByRef oApprenticeDoc As ApprenticeServerDocument)

        Dim NewFullFilename As String = NewFolder + newFileName(oApprenticeDoc.FullFileName)

        Try
            mApprenticeApp.FileSaveAs.AddFileToSave(oApprenticeDoc, NewFullFilename)

            For Each oRefDoc As ApprenticeServerDocument In oApprenticeDoc.ReferencedDocuments
                SaveRec(NewFolder, oRefDoc)
            Next

        Catch
            'Content Center Parts will fail
        End Try
    End Sub

#End Region

End Class
