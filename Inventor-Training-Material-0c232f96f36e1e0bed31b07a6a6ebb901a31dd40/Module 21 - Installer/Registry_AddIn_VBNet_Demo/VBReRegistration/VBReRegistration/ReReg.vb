Imports System
imports System.Runtime.InteropServices
imports System.Reflection
imports System.IO

imports InventorRegistration

Class CReReg : Implements IAddInRegistrationInfo

    Dim m_guidStr As String = "{9C28ED35-8E07-40e6-AD2C-3B2C3BD9FA04}"
    Dim m_descriptiveName As String = "RegisterAddInVB2009"

    'Dim m_guidStr As String = "{DD417439-B0B1-4b7e-BBE4-129BFFF76AC8}"
    'Dim m_descriptiveName As String = "RegisterAddInVB2010"

    Dim m_executablePath As String = Assembly.GetExecutingAssembly().Location

    Dim m_supportedVersions() As VersionSupportRule = New VersionSupportRule() _
    { _
        New VersionSupportRule(VersionSupportType.SupportedSoftwareVersionGreaterThan, "11..") _
    }

    Dim m_dotNetAssemblies() As String = New String() _
    { _
        "AddInVBTest.dll" _
    }


    Public ReadOnly Property DescriptiveName() As String Implements InventorRegistration.IAddInRegistrationInfo.DescriptiveName
        Get
            DescriptiveName = m_descriptiveName
        End Get
    End Property

    Public ReadOnly Property ExecutablePath() As String Implements InventorRegistration.IAddInRegistrationInfo.ExecutablePath
        Get
            ExecutablePath = m_executablePath
        End Get
    End Property

    Public ReadOnly Property GuidString() As String Implements InventorRegistration.IAddInRegistrationInfo.GuidString
        Get
            GuidString = m_guidStr
        End Get
    End Property

    Public Sub Register() Implements InventorRegistration.IAddInRegistrationInfo.Register

        Utils.RegisterDotNetAssemblies(Path.GetDirectoryName(ExecutablePath), m_dotNetAssemblies, True, True)

    End Sub

    Public ReadOnly Property SupportedVersions() As InventorRegistration.VersionSupportRule() Implements InventorRegistration.IAddInRegistrationInfo.SupportedVersions
        Get
            SupportedVersions = m_supportedVersions
        End Get
    End Property

    Public Sub Unregister() Implements InventorRegistration.IAddInRegistrationInfo.Unregister

        Utils.RegisterDotNetAssemblies(Path.GetDirectoryName(ExecutablePath), m_dotNetAssemblies, False, True)

    End Sub

End Class


Public Module ReReg

    Public Sub Main(ByVal args As String())

        InventorRegistration.Utils.DoAddInRegistration(args, New CReReg())

    End Sub

End Module
