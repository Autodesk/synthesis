Imports Inventor
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Diagnostics


Public Class Form1

    Dim _InvApp As Inventor.Application
    Dim _macros As Macros
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

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

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
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

    Dim ThisApplication As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        ThisApplication = oApp

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

    ' This sample demonstrates creating an assembly connection.  It connects the
    ' centers of two faces using a rigid connection.  The part SamplePart.ipt must
    ' be used because the sample assumes there are iMates in the part which it
    ' used to find specific geometry.
    Public Sub AssemblyConnect()
        ' Create a new assembly document.
        Dim asmDoc As AssemblyDocument
        asmDoc = ThisApplication.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject, _
                      ThisApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kAssemblyDocumentObject))
        Dim asmDef As AssemblyComponentDefinition
        asmDef = asmDoc.ComponentDefinition

        ' Place an occurrence into the assembly.
        Dim occ1 As ComponentOccurrence
        Dim occ2 As ComponentOccurrence
        Dim trans As Matrix
        trans = ThisApplication.TransientGeometry.CreateMatrix
        'select demo file "SamplePart.ipt"

        occ1 = asmDef.Occurrences.Add(OpenFile("(*.ipt)|*.ipt"), trans)

        ' Place a second occurrence with the matrix adjusted so it fits correctly with the first occurrence.
        trans.Cell(1, 4) = 6 * 2.54
        'select demo file "SamplePart.ipt"
        occ2 = asmDef.Occurrences.Add(OpenFile("(*.ipt)|*.ipt"), trans)

        ' Get Face 6 from occ1 and create a FaceProxy.
        Dim Face1 As Face
        Face1 = GetNamedEntity(occ1, "Face6")
        Dim Face2 As Face
        Face2 = GetNamedEntity(occ2, "Face7")

        ' Create two intents to define the geometry for the connection.
        Dim intentOne As GeometryIntent
        intentOne = asmDef.CreateGeometryIntent(Face1, PointIntentEnum.kPlanarFaceCenterPointIntent)
        Dim intentTwo As GeometryIntent
        intentTwo = asmDef.CreateGeometryIntent(Face2, PointIntentEnum.kPlanarFaceCenterPointIntent)

        ' Create a rigid connection between the two parts.
        Dim connectDef As AssemblyConnectionDefinition
        connectDef = asmDef.Connections.CreateAssemblyConnectionDefinition(AssemblyConnectionTypeEnum.kRigidConnectionType, intentOne, intentTwo)
        connectDef.FlipAlignmentDirection = False
        connectDef.FlipOriginDirection = True
        Dim connect As AssemblyConnection
        connect = asmDef.Connections.Add(connectDef)

        ' Make the connection visible.
        connect.Visible = True
    End Sub




    ' This sample demonstrates creating an assembly connection.  It connects the
    ' midpoints of the edges of two faces using a rotational connection.  To do this
    ' it first creates a geometry intent object of the midpoint of the edge and then
    ' creates another intent using the face and the midpoint intent.  It does this to
    ' create to midpoint intents which it then uses to create the rotational connection.
    ' The part SamplePart.ipt must be used because the sample assumes there are iMates in
    ' the part which it used to find specific geometry.
    Public Sub AssemblyConnect2()
        ' Create a new assembly document.
        Dim asmDoc As AssemblyDocument
        asmDoc = ThisApplication.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject, _
                      ThisApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kAssemblyDocumentObject))
        Dim asmDef As AssemblyComponentDefinition
        asmDef = asmDoc.ComponentDefinition

        ' Place an occurrence into the assembly.
        Dim occ1 As ComponentOccurrence
        Dim occ2 As ComponentOccurrence
        Dim trans As Matrix
        trans = ThisApplication.TransientGeometry.CreateMatrix
        'select demo file "SamplePart.ipt"
        occ1 = asmDef.Occurrences.Add(OpenFile("(*.ipt)|*.ipt"), trans)

        ' Place a second occurrence with the matrix adjusted so it fits correctly with the first occurrence.
        trans.Cell(1, 4) = 6 * 2.54
        'select demo file "SamplePart.ipt"
        occ2 = asmDef.Occurrences.Add(OpenFile("(*.ipt)|*.ipt"), trans)

        ' Get Face 1 from occ1 and create a FaceProxy.
        Dim Face1 As Face
        Face1 = GetNamedEntity(occ1, "Face1")

        ' Get Face 2 from occ2 and create a FaceProxy.
        Dim Face2 As Face
        Face2 = GetNamedEntity(occ2, "Face2")

        ' Get Edge 1 from occ2 and create an EdgeProxy.
        Dim Edge1 As Edge
        Edge1 = GetNamedEntity(occ2, "Edge1")

        ' Get Edge 3 from occ1 and create an EdgeProxy.
        Dim Edge3 As Edge
        Edge3 = GetNamedEntity(occ1, "Edge3")

        ' Create an intent to the center of Edge1.
        Dim edge1Intent As GeometryIntent
        edge1Intent = asmDef.CreateGeometryIntent(Edge1, PointIntentEnum.kMidPointIntent)

        ' Create an intent to the center of Edge3.
        Dim edge3Intent As GeometryIntent
        edge3Intent = asmDef.CreateGeometryIntent(Edge3, PointIntentEnum.kMidPointIntent)

        ' Create two intents to define the geometry for the connection.
        Dim intentOne As GeometryIntent
        intentOne = asmDef.CreateGeometryIntent(Face2, edge1Intent)
        Dim intentTwo As GeometryIntent
        intentTwo = asmDef.CreateGeometryIntent(Face1, edge3Intent)

        ' Create a rigid connection between the two parts.
        Dim connectDef As AssemblyConnectionDefinition
        connectDef = asmDef.Connections.CreateAssemblyConnectionDefinition(AssemblyConnectionTypeEnum.kRotationalConnectionType, intentOne, intentTwo)
        connectDef.FlipAlignmentDirection = False
        connectDef.FlipOriginDirection = True
        Dim connect As AssemblyConnection
        connect = asmDef.Connections.Add(connectDef)

        ' Make the connection visible.
        connect.Visible = True

        ' Drive the constraint to animate it.
        connect.DriveSettings.StartValue = "180 deg"
        connect.DriveSettings.EndValue = "360 deg"
        connect.DriveSettings.GoToStart()
        connect.DriveSettings.PlayForward()
        connect.DriveSettings.PlayReverse()
        connect.DriveSettings.PlayForward()
        connect.DriveSettings.PlayReverse()
        connect.DriveSettings.PlayForward()
        connect.DriveSettings.PlayReverse()
    End Sub


    ' This finds the entity associated with an iMate of a specified name.  This
    ' allows iMates to be used as a generic naming mechansim.
    Private Function GetNamedEntity(ByVal Occurrence As ComponentOccurrence, ByVal Name As String) As Object
        Dim resultEntity As Object
        resultEntity = Nothing

        ' Look for the iMate that has the specified name in the referenced file.
        Dim iMate As iMateDefinition
        Dim partDef As PartComponentDefinition
        partDef = Occurrence.Definition
        For Each iMate In partDef.iMateDefinitions
            ' Check to see if this iMate has the correct name.
            If UCase(iMate.Name) = UCase(Name) Then
                ' Get the geometry assocated with the iMate.
                Dim entity As Object
                entity = iMate.entity

                ' Create a proxy.

                Call Occurrence.CreateGeometryProxy(entity, resultEntity)

                Exit For
            End If
        Next

        ' Return the found entity, or Nothing if a match wasn't found.
        GetNamedEntity = resultEntity
    End Function

End Class

