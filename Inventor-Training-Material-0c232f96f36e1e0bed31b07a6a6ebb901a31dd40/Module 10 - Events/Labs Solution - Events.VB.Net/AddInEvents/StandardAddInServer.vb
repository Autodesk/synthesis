Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace AddInEvents

    <ProgIdAttribute("AddInEvents.StandardAddInServer"), _
    GuidAttribute("49d3dbd4-20c4-4c03-8027-49206f131596")> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application
        Private mControlForm As Form1

        Private m_ApplicationEvents As Inventor.ApplicationEvents


#Region "ApplicationAddInServer Members"


        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, ByVal firstTime As Boolean) Implements Inventor.ApplicationAddInServer.Activate

            m_inventorApplication = addInSiteObject.Application

            m_ApplicationEvents = m_inventorApplication.ApplicationEvents
            AddHandler m_ApplicationEvents.OnOpenDocument, AddressOf Me.m_ApplicationEvents_OnOpenDocument

            mControlForm = New Form1(m_inventorApplication)
            mControlForm.ShowModeless()

        End Sub

        Private Sub m_ApplicationEvents_OnOpenDocument(ByVal DocumentObject As Inventor._Document, _
                                                       ByVal FullDocumentName As String, _
                                                       ByVal BeforeOrAfter As Inventor.EventTimingEnum, _
                                                       ByVal Context As Inventor.NameValueMap, _
                                                       ByRef HandlingCode As Inventor.HandlingCodeEnum) _

            System.Windows.Forms.MessageBox.Show("OnOpenDocument: " + DocumentObject.DisplayName)

        End Sub

        Public Sub Deactivate() Implements Inventor.ApplicationAddInServer.Deactivate

            ' This method is called by Inventor when the AddIn is unloaded.
            ' The AddIn will be unloaded either manually by the user or
            ' when the Inventor session is terminated.

            ' TODO:  Add ApplicationAddInServer.Deactivate implementation

            ' Release objects.
            Marshal.ReleaseComObject(m_inventorApplication)
            m_inventorApplication = Nothing

            System.GC.WaitForPendingFinalizers()
            System.GC.Collect()

        End Sub

        Public ReadOnly Property Automation() As Object Implements Inventor.ApplicationAddInServer.Automation

            ' This property is provided to allow the AddIn to expose an API 
            ' of its own to other programs. Typically, this  would be done by
            ' implementing the AddIn's API interface in a class and returning 
            ' that class object through this property.

            Get
                Return Nothing
            End Get

        End Property

        Public Sub ExecuteCommand(ByVal commandID As Integer) Implements Inventor.ApplicationAddInServer.ExecuteCommand

            ' Note:this method is now obsolete, you should use the 
            ' ControlDefinition functionality for implementing commands.

        End Sub

#End Region
       
    End Class

End Namespace

