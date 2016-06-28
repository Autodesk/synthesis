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

    Public Sub UsingTrans()
        ' Get a reference to the active document.
        ' This can be an Assembly or Part document.
        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        Dim oCmpDef As PartComponentDefinition
        oCmpDef = oDoc.ComponentDefinition

        Dim oSketch As PlanarSketch
        oSketch = oCmpDef.Sketches(1)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Get the transaction manager from the application
        Dim oTxnMgr As TransactionManager
        oTxnMgr = _InvApplication.TransactionManager

        ' Start a regular transaction
        Dim oTxn As Transaction
        oTxn = oTxnMgr.StartTransaction(oDoc, "My Rectangle Command")

        ' Draw four sketch lines
        Dim oLine As SketchLine
        oLine = oSketch.SketchLines.AddByTwoPoints(oTG.CreatePoint2d(0, 0), oTG.CreatePoint2d(1, 0))
        oLine = oSketch.SketchLines.AddByTwoPoints(oLine.EndSketchPoint, oTG.CreatePoint2d(1, 2))
        oLine = oSketch.SketchLines.AddByTwoPoints(oLine.EndSketchPoint, oTG.CreatePoint2d(0, 2))
        oLine = oSketch.SketchLines.AddByTwoPoints(oLine.EndSketchPoint, oTG.CreatePoint2d(0, 0))

        oTxn.End()
    End Sub

    Public Sub CreateMyProperty()

        Dim oPropSet As PropertySet
        Dim oProp As Inventor.Property
        Dim oTransMgr As TransactionManager
        Dim oTrans As Transaction
        oTransMgr = _InvApplication.TransactionManager
        oTrans = oTransMgr.StartTransaction(_InvApplication.ActiveDocument, "Stuff")
        oPropSet = _InvApplication.ActiveDocument.PropertySets.Add("MyPropSet")
        oProp = oPropSet.Add(5.0#, "MyProp")
        oTrans.End()

    End Sub

    Public Sub start_end_abort()
        ' Get a reference to the active document.
        ' This can be an Assembly or Part document.
        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        ' Get the transaction manager from the application
        Dim oTxnMgr As TransactionManager
        oTxnMgr = _InvApplication.TransactionManager

        ' Start a transaction
        Dim oTxn As Transaction
        oTxn = oTxnMgr.StartTransaction(oDoc, "MyTransaction")

        Try
            ' Perform an operation that you wish to transact

            ' as a demo we do an invalid operation
            ' assumes  c:\dummy.ipt does NOT exist

            Dim dummyDoc As Document = _InvApplication.Documents.Open("c:\dummy.ipt")
             
            ' End the transaction
            oTxn.End()
        Catch ex As Exception
            ' If the error from the operation is not recoverable, abort the Txn
            MsgBox("Unrecoverable error occurred during the operation")
            oTxn.Abort()
        End Try

    End Sub

    ''' <summary>
    ''' checkPoints
    ''' assume a part document   is opened
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub checkPoints()

        Dim oDoc As PartDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oDef As PartComponentDefinition
        oDef = oDoc.ComponentDefinition

        Dim oSketch As PlanarSketch
        oSketch = oDef.Sketches(1) 

        ' Get the transaction manager from the application
        Dim oTxnMgr As TransactionManager
        oTxnMgr = _InvApplication.TransactionManager


        ' Start a regular transaction
        Dim oTxn As Transaction
        oTxn = oTxnMgr.StartTransaction(oDoc, "Checkpoint Txn")

        Dim oChkPt As CheckPoint
        Try

            ' *****************************************
            ' Perform the creation of extrude profile
            ' *****************************************
            oSketch.SketchCircles.AddByCenterRadius(_InvApplication.TransientGeometry.CreatePoint2d(0, 0), 10)
          


            ' Create a checkpoint before the second job 
            oChkPt = oTxnMgr.SetCheckPoint

            ' *****************************************
            ' Perform the second  job
            ' *****************************************   
            Dim oExFDef As ExtrudeDefinition
            oExFDef = oDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oSketch.Profiles.AddForSolid(), PartFeatureOperationEnum.kJoinOperation)

            ' we intentionally set the extent distance as 0. this will fail  ExtrudeFeatures.Add
            oExFDef.SetDistanceExtent(0, PartFeatureExtentDirectionEnum.kPositiveExtentDirection)
            oDef.Features.ExtrudeFeatures.Add(oExFDef)
         
            ' End the transaction
            oTxn.End()
        Catch ex As Exception
            ' Handle any error condition from the extrude command 
            Dim result As DialogResult = MessageBox.Show("Extrude operation failed. Modify profile ?", "error", MessageBoxButtons.YesNo)

            If result = DialogResult.Yes Then
                oTxnMgr.GoToCheckPoint(oChkPt)
            Else
                oTxn.Abort()
            End If

        End Try 

    End Sub

    Public Sub Parent_Child_Trasns()
        ' Get a reference to the active document.
        ' This can be an Assembly or Part document.
        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        Dim oCmpDef As PartComponentDefinition
        oCmpDef = oDoc.ComponentDefinition

        Dim oSketch As PlanarSketch
        oSketch = oCmpDef.Sketches(1)

        Dim oTG As TransientGeometry
        oTG = _InvApplication.TransientGeometry

        ' Get the transaction manager from the application
        Dim oTxnMgr As TransactionManager
        oTxnMgr = _InvApplication.TransactionManager

        ' Nesting regular transactions
        ' Start a regular transaction
        Dim oTxn1 As Transaction
        oTxn1 = oTxnMgr.StartTransaction(oDoc, "My Txn")

        ' Draw a sketch line
        Dim oLine As SketchLine
        oLine = oSketch.SketchLines.AddByTwoPoints(oTG.CreatePoint2d(0, 0), oTG.CreatePoint2d(1, 0))

        ' Start a nested transaction
        Dim oTxn2 As Transaction
        oTxn2 = oTxnMgr.StartTransaction(oDoc, "My child Txn")

        ' Draw a circle
        Dim oCircle As SketchCircle
        oCircle = oSketch.SketchCircles.AddByCenterRadius(oLine.EndSketchPoint, 3)

        oTxn2.End()

        oTxn1.End()


    End Sub

#Region "Transaction Events"
    Public Sub startEvents()
        Dim oTransMgr As TransactionManager

        oTransMgr = _InvApplication.TransactionManager

        Dim oTransEvents As TransactionEvents
        oTransEvents = oTransMgr.TransactionEvents

        AddHandler oTransEvents.OnUndo, AddressOf OnUndo
        AddHandler oTransEvents.OnCommit, AddressOf OnCommit
        AddHandler oTransEvents.OnDelete, AddressOf OnDelete
        AddHandler oTransEvents.OnRedo, AddressOf OnRedo
    End Sub

    Public Sub stopEvents()
        Dim oTransMgr As TransactionManager
        oTransMgr = _InvApplication.TransactionManager

        Dim oTransEvents As TransactionEvents
        oTransEvents = oTransMgr.TransactionEvents

        RemoveHandler oTransEvents.OnUndo, AddressOf OnUndo
    End Sub

    Sub OnUndo(TransactionObject As Inventor.Transaction,
            Context As Inventor.NameValueMap,
            BeforeOrAfter As Inventor.EventTimingEnum,
            ByRef HandlingCode As Inventor.HandlingCodeEnum)

        MsgBox("undo [" & TransactionObject.DisplayName & " ] ")

    End Sub
 


    Sub OnRedo(TransactionObject As Inventor.Transaction,
               Context As Inventor.NameValueMap,
               BeforeOrAfter As Inventor.EventTimingEnum,
               ByRef HandlingCode As Inventor.HandlingCodeEnum)
        MsgBox("redo  [" & TransactionObject.DisplayName & " ] ")
    End Sub

    Sub OnDelete(TransactionObject As Inventor.Transaction,
                 Context As Inventor.NameValueMap,
                 BeforeOrAfter As Inventor.EventTimingEnum)
        MsgBox("delete  [" & TransactionObject.DisplayName & " ] ")
    End Sub

    Sub OnCommit(TransactionObject As Inventor.Transaction,
                 Context As Inventor.NameValueMap,
                 BeforeOrAfter As Inventor.EventTimingEnum,
                 ByRef HandlingCode As Inventor.HandlingCodeEnum)
        MsgBox("commit  [" & TransactionObject.DisplayName & " ] ")
    End Sub
#End Region




End Class

