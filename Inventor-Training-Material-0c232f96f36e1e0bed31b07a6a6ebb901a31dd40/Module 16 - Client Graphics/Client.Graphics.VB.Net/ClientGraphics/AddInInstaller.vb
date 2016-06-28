Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Reflection


Public Class AddInInstaller
    <DllImport("kernel32.dll", SetLastError:=True, CallingConvention:=CallingConvention.Winapi)> _
 Public Shared Function IsWow64Process( _
   <[In]()> ByVal hProcess As IntPtr, _
   <Out()> ByRef lpSystemInfo As Boolean) _
   As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    Public Sub New()
        MyBase.New()

        InitializeComponent()
    End Sub

    Public Overrides Sub Install( _
      ByVal stateSaver As System.Collections.IDictionary)

        MyBase.Install(stateSaver)

        Dim retVal As Boolean = False
        IsWow64Process(Process.GetCurrentProcess().Handle, retVal)

        If retVal Then ' 64bits
            RegAsmFor64bits(" /codebase")
        Else ' 32bits
            Dim regsrv As RegistrationServices = _
              New RegistrationServices()
            regsrv.RegisterAssembly( _
              Me.GetType().Assembly, _
              AssemblyRegistrationFlags.SetCodeBase)
        End If

    End Sub

    Public Overrides Sub Uninstall( _
      ByVal savedState As System.Collections.IDictionary)

        MyBase.Uninstall(savedState)

        Dim retVal As Boolean = False
        IsWow64Process(Process.GetCurrentProcess().Handle, retVal)

        If retVal Then ' 64bits
            RegAsmFor64bits(" /u")
        Else  '32bits
            Dim regsrv As RegistrationServices = _
              New RegistrationServices()
            regsrv.UnregisterAssembly(Me.GetType().Assembly)
        End If

    End Sub

    Private Sub RegAsmFor64bits(ByVal parameters As String)

        ' from http://resnikb.wordpress.com/2009/05/21/installing-the-same-managed-addin-in-32bit-and-64bit-autodesk-inventor/.


        Dim net_base As String = _
          Path.GetFullPath( _
            Path.Combine( _
              RuntimeEnvironment.GetRuntimeDirectory(), "..\.."))
        Dim to_execString As String = _
          String.Concat( _
            net_base, _
            "\Framework64\", _
            RuntimeEnvironment.GetSystemVersion(), _
            "\regasm.exe")

        Dim dll_path As String = Me.GetType().Assembly.Location
        If Not (File.Exists(to_execString)) Then
            MsgBox("Failed to find RegAsm")
            Exit Sub
        End If

        Dim oProcess As Process = New Process
        oProcess.StartInfo.CreateNoWindow = True
        oProcess.StartInfo.ErrorDialog = False
        oProcess.StartInfo.UseShellExecute = False
        oProcess.StartInfo.FileName = to_execString
        oProcess.StartInfo.Arguments = _
          " " & parameters & "     " & """" & dll_path & """"

        oProcess.Start()
        oProcess.WaitForExit()

    End Sub
End Class
