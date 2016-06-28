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

    'Opens an existing document.assume part1.ipt exists
    Public Sub OpenDoc()
        Dim oDoc As Document
        oDoc = _InvApplication.Documents.Open("C:\Temp\Part1.ipt")
    End Sub

    'Creates a new document using a specified template.
    Public Sub CreateDoc()

        Dim oDoc As PartDocument
        oDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, _
                _InvApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), _
                True)
    End Sub

    'Creates a new document using internally defined template.
    '(Can be done in the UI by using Ctrl-Shift when creating new document.)
    Public Sub CreateDoc2()
        Dim oDoc As PartDocument
        oDoc = _InvApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, , True)
    End Sub

    'Sample iProperties Dump:
    Public Sub DumpDocProperties()

        Dim oDocument As Document
        oDocument = _InvApplication.ActiveDocument

        Dim oPropertySets As PropertySets
        oPropertySets = oDocument.PropertySets

        Dim oPropertySet As PropertySet

        'Namespace required to avoid conflict with Vb.Net "Property" keyword
        Dim oProperty As Inventor.Property

        For Each oPropertySet In oPropertySets

            Debug.Print(vbCrLf + "----------------- Property Set: " + oPropertySet.Name)

            For Each oProperty In oPropertySet

                Debug.Print(" - " + oProperty.Name + " [ID:" + oProperty.PropId.ToString() + "] : " + Space(30 - Len(oProperty.Name)) + oProperty.Expression)

            Next
        Next

    End Sub

    'access "Design Tracking Properties" "Designer">> "Designer" and change its value
    Public Sub iPropAccess()

        Dim oDoc As Document
        oDoc = _InvApplication.ActiveDocument

        ' Access a particular property set.  In this case the design tracking property set.
        Dim oDTProps As PropertySet
        oDTProps = oDoc.PropertySets.Item("{32853F0F-3444-11d1-9E93-0060B03C1CA6}")

        ' Access the same property set using the display name or name. DisplayName is not
        ' dependable because it can be localized, so the internal name or name is preferred.
        oDTProps = oDoc.PropertySets.Item("Design Tracking Properties")

        ' Get a specific property, in this case the designer property.
        Dim oDesignerProp As Inventor.Property
        oDesignerProp = oDTProps.ItemByPropId( _
     PropertiesForDesignTrackingPropertiesEnum.kDesignerDesignTrackingProperties)

        ' You can also use the name or display name
        ' the display name has the problem that it can be changed.
        oDesignerProp = oDTProps.Item("Designer")

        ' Show the display name and value.
        Debug.Print(oDesignerProp.DisplayName & " = " & oDesignerProp.Value)

        ' Change the designer name.
        oDesignerProp.Value = "Bill & Ted"

    End Sub

    'Parameters Creation
    Public Sub CreateUserParams()

        If  Not TypeOf _InvApplication.ActiveDocument is PartDocument then
            MsgBox( "Please open a part document!")
            Exit Sub 
        End If

        Dim oDoc As PartDocument
        oDoc = _InvApplication.ActiveDocument

        Dim oUserParams As UserParameters
        oUserParams = oDoc.ComponentDefinition.Parameters.UserParameters

        Dim oNewParam As Parameter

        oNewParam = oUserParams.AddByExpression("NewParam1", "3", UnitsTypeEnum.kInchLengthUnits)
        oNewParam = oUserParams.AddByExpression("NewParam1_2", "3", "in")

        oNewParam = oUserParams.AddByExpression("NewParam2", "5", UnitsTypeEnum.kDefaultDisplayLengthUnits)
        oNewParam = oUserParams.AddByExpression("NewParam2_2", "5 in", UnitsTypeEnum.kDefaultDisplayLengthUnits)

        oNewParam = oUserParams.AddByValue("NewParam3", 3 * 2.54, UnitsTypeEnum.kDefaultDisplayLengthUnits)


        'Set Param3 not deletable from the UI
        oNewParam.DisabledActionTypes = ActionTypeEnum.kDeleteAction

        Dim oGroup As CustomParameterGroup
        oGroup = oDoc.ComponentDefinition.Parameters.CustomParameterGroups.Add("CustomGroup", "intCustGrpName")

        oGroup.Add(oNewParam)

    End Sub

End Class
