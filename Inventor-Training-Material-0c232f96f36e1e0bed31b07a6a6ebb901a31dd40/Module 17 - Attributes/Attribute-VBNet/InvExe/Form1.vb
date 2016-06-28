Imports Inventor
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Diagnostics


Public Class Form1

    Dim _InvApp As Inventor.Application
    Dim _macros As Macros

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
    ''' create an attribute
    ''' this assumes a part document with some features is opened.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub AddAttribute()

        ' Get the selected edge.  This assumes an edge is already selected.
        Dim oEdge As Edge
        oEdge = _InvApplication.ActiveDocument.SelectSet.Item(1)

        ' Add an attribute set to the edge.  If you only need to "name" the
        ' edge this is enough to find it later and you can skip the next step.
        Dim oAttribSet As AttributeSet
        oAttribSet = oEdge.AttributeSets.Add("BoltEdge")

        ' Add an attribute to the set.  This can be any information you
        ' want to associate with the edge.
        Call oAttribSet.Add("BoltRadius", ValueTypeEnum.kDoubleType, 0.5)

    End Sub

    ''' <summary>
    ''' Query Attribute
    '''  this assumes a part document with some features is opened.
    ''' and you have run  AddAttribute on an edge
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub QueryAttribute()

        ' Set a reference to the attribute manager of the active document.
        Dim oAttribMgr As AttributeManager
        oAttribMgr = _InvApplication.ActiveDocument.AttributeManager

        ' Get the objects with a particular attribute attached.
        Dim oObjs As ObjectCollection
        oObjs = oAttribMgr.FindObjects("BoltEdge", "BoltRadius", 0.5)

        ' Get the objects that have an attribute set of a certain name.
        oObjs = oAttribMgr.FindObjects("BoltEdge")

        ' Get the attribute sets with a certain name.
        Dim oAttribSets As AttributeSetsEnumerator
        oAttribSets = oAttribMgr.FindAttributeSets("BoltEdge")

        ' Get the attribute sets with a certain name using a wild card.
        oAttribSets = oAttribMgr.FindAttributeSets("Bolt*")

    End Sub

    ''' <summary>
    ''' assumes a document is opened    ''' 
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Add_Persistent_Transient_Attribute()

        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument


        MsgBox("add one attribute set named PersistentAttSet which is persistent" & vbCrLf & "add one attribute set named TransientAttSet which is transient")

            ' add persistent attribute
            Dim oPersistentAttSet As AttributeSet
            oPersistentAttSet = oDoc.AttributeSets.Add("PersistentAttSet")

            oPersistentAttSet.Add("PersistentAtt", ValueTypeEnum.kDoubleType, 0.5)
 

            'add transient attribute
            Dim oTransientAttSet As AttributeSet
            oTransientAttSet = oDoc.AttributeSets.AddTransient("TransientAttSet")

            oPersistentAttSet.Add("TransientAtt", ValueTypeEnum.kDoubleType, 1)


        MsgBox("now try to find the attributes PersistentAttSet in the current document")


        ' Set a reference to the attribute manager of the active document.
        Dim oAttribMgr As AttributeManager
        oAttribMgr = oDoc.AttributeManager

        Dim oAttSets As AttributeSetsEnumerator
        oAttSets = oAttribMgr.FindAttributeSets("PersistentAttSet")

        If oAttSets.Count > 0 Then
            MsgBox("find Persistent AttSet!")
        Else
            MsgBox("cannot find Persistent AttSet!")
        End If

        MsgBox("now try to find the attributes PersistentAttSet in the current document")

        oAttSets = oAttribMgr.FindAttributeSets("TransientAttSet")

        If oAttSets.Count > 0 Then
            MsgBox("find Transient AttSet!")
        Else
            MsgBox("cannot find Transient AttSet!")
        End If

        MsgBox("Please save and close this document, open it again. Then click [OK] of this dialog")

        oDoc = _InvApplication.ActiveDocument

        oAttribMgr = oDoc.AttributeManager 

        oAttSets = oAttribMgr.FindAttributeSets("PersistentAttSet")

        If oAttSets.Count > 0 Then
            MsgBox("find Persistent AttSet!")
        Else
            MsgBox("cannot find Persistent AttSet!")
        End If
 
        oAttSets = oAttribMgr.FindAttributeSets("TransientAttSet")

        If oAttSets.Count > 0 Then
            MsgBox("find Transient AttSet!")
        Else
            MsgBox("cannot find Transient AttSet!")
        End If


    End Sub


End Class
