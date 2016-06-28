
Imports Inventor

Public Class Form1

    Private mApp As Inventor.Application

    'Illustrates how to create a custom class that holds our combobox items
    'The key to do that is to override the "ToString" method
    Private Class ComboAddinsItem

        Private _Name As String
        Private _clsId

        Public ReadOnly Property Name() As String
            Get
                Name = _Name
            End Get
        End Property

        Public ReadOnly Property clsId() As String
            Get
                clsId = _clsId
            End Get
        End Property

        Public Sub New(ByVal name As String, ByVal clsId As String)
            _Name = name
            _clsId = clsId
        End Sub

        Public Overrides Function ToString() As String
            ToString = _Name
        End Function

    End Class


    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try

            mApp = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")

            Dim oAddIn As ApplicationAddIn
            For Each oAddIn In mApp.ApplicationAddIns

                Try
                    'Fills up at runtime our addins combobox
                    If (oAddIn.AddInType = ApplicationAddInTypeEnum.kTranslationApplicationAddIn) Then
                        Dim newItem As New ComboAddinsItem(oAddIn.DisplayName, oAddIn.ClassIdString)
                        ComboAddIns.Items.Add(newItem)
                    End If

                Catch
                    'Exception can be thrown if "AddInType", "DisplayName" or "ClassIdString" properties
                    'are not implemented. For a custom addin for example.
                    Continue For
                End Try

            Next

            ComboAddIns.SelectedIndex = 0

        Catch ex As Exception

            System.Windows.Forms.MessageBox.Show("Error: Inventor must be running...")
            Button1.Enabled = False
            Exit Sub

        End Try

    End Sub


    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        OptionsView.View = System.Windows.Forms.View.Details

    End Sub


    Public Function GetTranslatorSaveAsOptions(ByVal TranslatorClsId As String, ByRef options As NameValueMap) As Boolean

        Dim oTranslator As TranslatorAddIn = mApp.ApplicationAddIns.ItemById(TranslatorClsId)

        If oTranslator Is Nothing Then
            GetTranslatorSaveAsOptions = False
            Exit Function
        End If

        oTranslator.Activate()

        If (oTranslator.AddInType <> ApplicationAddInTypeEnum.kTranslationApplicationAddIn) Then
            'Not a translator addin...
            GetTranslatorSaveAsOptions = False
            Exit Function
        End If

        'Gets application translation context and set type to UnspecifiedIOMechanism
        Dim Context As TranslationContext = mApp.TransientObjects.CreateTranslationContext
        Context.Type = IOMechanismEnum.kUnspecifiedIOMechanism

        options = mApp.TransientObjects.CreateNameValueMap

        Dim SourceObject As Object = mApp.ActiveDocument

        'Checks whether the translator has 'SaveCopyAs' options
        Try
            GetTranslatorSaveAsOptions = oTranslator.HasSaveCopyAsOptions(SourceObject, Context, options)
        Catch
            GetTranslatorSaveAsOptions = False
        End Try
    End Function


    Public Function GetTranslatorOpenOptions(ByVal TranslatorClsId As String, ByRef options As NameValueMap) As Boolean

        Dim oTranslator As TranslatorAddIn = mApp.ApplicationAddIns.ItemById(TranslatorClsId)

        If oTranslator Is Nothing Then
            GetTranslatorOpenOptions = False
            Exit Function
        End If

        oTranslator.Activate()

        If (oTranslator.AddInType <> ApplicationAddInTypeEnum.kTranslationApplicationAddIn) Then
            'Not a translator addin...
            GetTranslatorOpenOptions = False
            Exit Function
        End If

        Dim Medium As DataMedium = mApp.TransientObjects.CreateDataMedium
        Medium.FileName = "C:\Temp\File.xxx"
        Medium.MediumType = MediumTypeEnum.kFileNameMedium

        Dim Context As TranslationContext = mApp.TransientObjects.CreateTranslationContext

        options = mApp.TransientObjects.CreateNameValueMap

        Try
            GetTranslatorOpenOptions = oTranslator.HasOpenOptions(Medium, Context, options)
        Catch
            GetTranslatorOpenOptions = False
        End Try

    End Function


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim options As NameValueMap = Nothing

        OptionsView.Items.Clear()

        Dim item As ComboAddinsItem = ComboAddIns.SelectedItem

        If (GetTranslatorSaveAsOptions(item.clsId, options)) Then

            Dim saveGroup As ListViewGroup = OptionsView.Groups("GroupSaveOpts")

            Dim idx As Integer
            For idx = 1 To options.Count

                Dim listviewItem As ListViewItem

                listviewItem = OptionsView.Items.Add(options.Name(idx))
                listviewItem.SubItems.Add(options.Value(options.Name(idx)).ToString())

                listviewItem.Group = saveGroup
            Next

        End If

        If (GetTranslatorOpenOptions(item.clsId, options)) Then

            Dim openGroup As ListViewGroup = OptionsView.Groups("GroupOpenOpts")

            Dim idx As Integer
            For idx = 1 To options.Count

                Dim listviewItem As ListViewItem

                listviewItem = OptionsView.Items.Add(options.Name(idx))
                listviewItem.SubItems.Add(options.Value(options.Name(idx)).ToString())

                listviewItem.Group = openGroup
            Next

        End If

    End Sub


End Class
