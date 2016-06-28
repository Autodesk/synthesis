Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace HelloWorldVBNet

     Public Interface MyInterface
        Sub MyFunction(ByVal someString As String)
    End Interface

    <ProgIdAttribute("HelloWorldVBNet.StandardAddInServer"), _
    GuidAttribute("de675f64-9144-4b02-82c7-601a02bd2742")> _
    Public Class StandardAddInServer
          Implements Inventor.ApplicationAddInServer
        Implements MyInterface

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application

#Region "ApplicationAddInServer Members"

        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, ByVal firstTime As Boolean) Implements Inventor.ApplicationAddInServer.Activate

            ' This method is called by Inventor when it loads the AddIn.
            ' The AddInSiteObject provides access to the Inventor Application object.
            ' The FirstTime flag indicates if the AddIn is loaded for the first time.

            ' Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application

            ' TODO:  Add ApplicationAddInServer.Activate implementation.
            ' e.g. event initialization, command creation etc.

            System.Windows.Forms.MessageBox.Show("Registry-Free AddIn Vb.NET is loaded!")

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

         Sub MyFunction(ByVal someString As String) Implements MyInterface.MyFunction

            System.Windows.Forms.MessageBox.Show(someString, "HelloWorld AddIn")

        End Sub

        Public ReadOnly Property Automation() As Object Implements Inventor.ApplicationAddInServer.Automation

            ' This property is provided to allow the AddIn to expose an API 
            ' of its own to other programs. Typically, this  would be done by
            ' implementing the AddIn's API interface in a class and returning 
            ' that class object through this property.

            Get
                Return Me
            End Get

        End Property

        Public Sub ExecuteCommand(ByVal commandID As Integer) Implements Inventor.ApplicationAddInServer.ExecuteCommand

            ' Note:this method is now obsolete, you should use the 
            ' ControlDefinition functionality for implementing commands.

        End Sub

#End Region

    End Class

End Namespace

