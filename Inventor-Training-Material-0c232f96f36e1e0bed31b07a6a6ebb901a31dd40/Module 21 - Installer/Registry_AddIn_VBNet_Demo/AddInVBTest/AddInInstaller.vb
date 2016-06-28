Imports System.ComponentModel
Imports System.Configuration.Install
Imports System.Runtime.InteropServices


Public Class AddInInstaller

    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    Public Overrides Sub Install(ByVal stateSaver As System.Collections.IDictionary)

        MyBase.Install(stateSaver)
        Dim regsrv As RegistrationServices = New RegistrationServices()
        regsrv.RegisterAssembly(Me.GetType().Assembly, AssemblyRegistrationFlags.SetCodeBase)

    End Sub

    Public Overrides Sub Uninstall(ByVal savedState As System.Collections.IDictionary)

        MyBase.Uninstall(savedState)
        Dim regsrv As RegistrationServices = New RegistrationServices()
        regsrv.UnregisterAssembly(Me.GetType().Assembly)

    End Sub

End Class
