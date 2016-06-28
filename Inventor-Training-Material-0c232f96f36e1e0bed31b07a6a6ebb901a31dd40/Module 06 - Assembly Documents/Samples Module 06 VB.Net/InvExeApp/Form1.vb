Imports Inventor
Imports System.Reflection

Public Class Form1

    Dim _macros As Macros

    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _macros = New Macros(oApp)

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


Public Class Macros

    Dim _InvApplication As Inventor.Application

    Public Sub New(ByVal oApp As Inventor.Application)

        _InvApplication = oApp

    End Sub

    '*********** Declare here your public Sub routines ***********

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Assembly Traversal
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub AssemblyTraversal()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        'Call the recursive function to iterate through the assembly tree
        Call AssemblyTraversalRec(oDoc.ComponentDefinition.Occurrences, 0)

    End Sub

    Private Sub AssemblyTraversalRec(ByVal InCollection As ComponentOccurrences, ByVal level As Long)

        'Iterate through the components in the current collection
        Dim oCompOccurrence As ComponentOccurrence
        For Each oCompOccurrence In InCollection

            If oCompOccurrence.DefinitionDocumentType = DocumentTypeEnum.kAssemblyDocumentObject Then

                Debug.Print(Space(3 * level) & " - [Asm]  " & oCompOccurrence.Name)

            Else

                Debug.Print(Space(3 * level) & " - [Part] " & oCompOccurrence.Name)

            End If

            'Recursively call this function for the sub-occurrences of the current component
            Call AssemblyTraversalRec(oCompOccurrence.SubOccurrences, level + 1)

        Next

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Create Occurrences
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub AddFromFile()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oMatrix As Matrix
        oMatrix = _InvApplication.TransientGeometry.CreateMatrix

        Dim oOcc As ComponentOccurrence
        oOcc = oDoc.ComponentDefinition.Occurrences.Add("C:\Temp\Part1.ipt", oMatrix)

    End Sub


    Public Sub AddFromMemory()

        Dim oDoc As AssemblyDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oPartDoc As PartDocument
        oPartDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, , False)

        Dim oMatrix As Matrix
        oMatrix = _InvApplication.TransientGeometry.CreateMatrix

        Dim oOcc As ComponentOccurrence
        oOcc = oDoc.ComponentDefinition.Occurrences.AddByComponentDefinition(oPartDoc.ComponentDefinition, oMatrix)

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Move occurrence
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub MoveOccurrence()

        Dim oAsm As AssemblyDocument
        oAsm = _InvApplication.ActiveDocument

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oOcc1 As ComponentOccurrence
        oOcc1 = oAsm.ComponentDefinition.Occurrences(1)

        If oOcc1.Grounded Then
            oOcc1.Grounded = False 
        End If
        Dim oNewMatrix As Matrix
        oNewMatrix = oOcc1.Transformation

        Call oNewMatrix.SetTranslation(oTG.CreateVector(15, 5, 5), False)

        oOcc1.Transformation = oNewMatrix

        oAsm.Update()

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '// Sample
    '//
    '// Use: Rotate occurrence
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Private Class Timer

        Dim _previous As DateTime

        Public Sub New()
            _previous = DateTime.Now
        End Sub

        Public Function GetElapsedSeconds() As Double

            Dim Now As DateTime = DateTime.Now
            Dim Elaspsed As TimeSpan = Now.Subtract(_previous)

            _previous = Now
            GetElapsedSeconds = Elaspsed.TotalSeconds

        End Function

    End Class

    Private Sub RotateOccurrence(ByVal oOccurrence As ComponentOccurrence, ByVal oAngleDeg As Double)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        Dim oRotMatrix As Matrix
        oRotMatrix = oTG.CreateMatrix

        Call oRotMatrix.SetToRotation(oAngleDeg * (System.Math.PI / 180), oTG.CreateVector(0, 0, 1), oTG.CreatePoint(0, 0, 0))

        Dim oNewMatrix As Matrix
        oNewMatrix = oOccurrence.Transformation

        Call oNewMatrix.PreMultiplyBy(oRotMatrix)

        oOccurrence.Transformation = oNewMatrix

    End Sub

    Public Sub RotateOccurrenceTest()

        Dim oAsm As AssemblyDocument
        oAsm = _InvApplication.ActiveDocument

        Dim oOcc1 As ComponentOccurrence
        oOcc1 = oAsm.ComponentDefinition.Occurrences(1)

        Dim Timer As New Timer
        Dim dT As Double = 0

        While dT < 10

            Call RotateOccurrence(oOcc1, 5)
            System.Windows.Forms.Application.DoEvents()
            dT = dT + Timer.GetElapsedSeconds

        End While

    End Sub

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    '//
    '//
    '// Use: Utility Method for the Lab
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////
    Public Sub AttachAttribute()

        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        If oDoc.SelectSet.Count <> 1 Then
            MsgBox("A single Edge must be selected...")
            Exit Sub
        End If

        If Not TypeOf oDoc.SelectSet(1) Is Edge Then
            MsgBox("Not an edge...")
            Exit Sub
        End If

        Try
            Dim oEdge As Edge
            oEdge = oDoc.SelectSet(1)

            'Create a new custom set
            Dim oAttSet As AttributeSet
            oAttSet = oEdge.AttributeSets.Add("CustomSet")

            'Create Attribute (Notice the syntax "Inventor.Attribute" is REQUIRED)
            Dim oAttribute As Inventor.Attribute
            oAttribute = oAttSet.Add("Insert", ValueTypeEnum.kStringType, "Insert edge")
        Catch
            MsgBox("Attribute or Set already exists...")
            Exit Sub
        End Try

    End Sub

End Class
