'/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
' Copyright (c) Autodesk, Inc. All rights reserved 
' Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
'
' Permission to use, copy, modify, and distribute this software in
' object code form for any purpose and without fee is hereby granted, 
' provided that the above copyright notice appears in all copies and 
' that both that copyright notice and the limited warranty and
' restricted rights notice below appear in all supporting 
' documentation.
'
' AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
' AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
' MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
' DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
' UNINTERRUPTED OR ERROR FREE.
'/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Imports System.IO
Imports System.Xml
Imports System.Reflection
Imports System.Diagnostics
Imports System.Collections
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.Configuration.Install
Imports System.Runtime.InteropServices

Namespace Registry_Free_AddIn_Demo

    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ' AddinInstaller1: Registry Free Inventor Add-in installer
    '  
    ' Author: liangx
    ' Creation date: 1/24/2013 2:25:14 PM
    ' 
    '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    <RunInstaller(True)> _
    Public Class AddinInstaller1
        Inherits Installer

#Region "Base Implementation"

        Private components As System.ComponentModel.IContainer = Nothing

        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso (components IsNot Nothing) Then
                components.Dispose()
            End If

            MyBase.Dispose(disposing)
        End Sub

        Private Sub InitializeComponent()
            components = New System.ComponentModel.Container()
        End Sub

#End Region

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Overrides Sub Install(ByVal stateSaver As IDictionary)
            Try
                MyBase.Install(stateSaver)

                Dim installMode As InstallUtils.InstallModeEnum = InstallUtils.InstallModeEnum.kRegistryFree

                Dim Asm As Assembly = Assembly.GetExecutingAssembly()
                Dim asmFile As New FileInfo(Asm.Location)

                stateSaver.Add("InstallMode", CInt(installMode))


                ' Example for version dependent

                'stateSaver.Add("PathToAddinFile", _
                '    InstallUtils.InstallRegistryFree( _
                '        stateSaver, _
                '        Asm, _
                '        InstallUtils.RegFreeModeEnum.kVersionDep, _
                '        "Inventor 2012"))

                stateSaver.Add("PathToAddinFile", _
                               InstallUtils.InstallRegistryFree( _
                                   stateSaver, _
                                   Asm, _
                                   InstallUtils.RegFreeModeEnum.kVersionIndep, _
                                   String.Empty))

            Catch ex As InstallException
                Throw New InstallException(ex.Message)
            Catch
                Throw New InstallException("Error installing addin!")
            End Try
        End Sub

        Public Overrides Sub Uninstall(ByVal savedState As IDictionary)
            Try
                Dim installMode As InstallUtils.InstallModeEnum =
                    CType(savedState("InstallMode"), InstallUtils.InstallModeEnum)

                Dim pathToAddinFile As String = DirectCast(savedState("PathToAddinFile"), String)
                InstallUtils.UninstallRegistryFree(savedState, pathToAddinFile)

            Catch
            End Try

            MyBase.Uninstall(savedState)
        End Sub
    End Class


    Public Class CommonUtils

        Public Shared Function GetOsVersion(ByVal checkSP As Boolean) As String
            Dim os As OperatingSystem = Environment.OSVersion

            Dim operatingSystem As String = "Unknown"

            If os.Platform = PlatformID.Win32Windows Then
                'This is a pre-NT version of Windows 
                Select Case os.Version.Minor
                    Case 0
                        operatingSystem = "95"
                        Exit Select
                    Case 10
                        If os.Version.Revision.ToString() = "2222A" Then
                            operatingSystem = "98SE"
                        Else
                            operatingSystem = "98"
                        End If
                        Exit Select
                    Case 90
                        operatingSystem = "Me"
                        Exit Select
                    Case Else
                        Exit Select
                End Select

            ElseIf os.Platform = PlatformID.Win32NT Then
                Select Case os.Version.Major
                    Case 3
                        operatingSystem = "NT 3.51"
                        Exit Select
                    Case 4
                        operatingSystem = "NT 4.0"
                        Exit Select
                    Case 5
                        If os.Version.Minor = 0 Then
                            operatingSystem = "2000"
                        Else
                            operatingSystem = "XP"
                        End If
                        Exit Select
                    Case 6
                        If os.Version.Minor = 0 Then
                            operatingSystem = "Vista"
                        Else
                            operatingSystem = "7"
                        End If
                        Exit Select
                    Case Else
                        Exit Select
                End Select
            End If

            'Make sure we actually got something in our OS check 
            'We don't want to just return " Service Pack 2" or " 32-bit" 
            If operatingSystem <> "" Then
                operatingSystem = "Windows " & operatingSystem

                If os.ServicePack <> "" AndAlso checkSP Then
                    operatingSystem += " " & os.ServicePack
                End If
            End If

            Return operatingSystem
        End Function

    End Class

    Public Class InstallUtils

        '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        'Use: Enums for install options
        '
        '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Public Enum InstallModeEnum
            kRegistryFree = 0
            kRegistry = 1
            kBoth = 2
        End Enum

        Public Enum RegFreeModeEnum
            kVersionDep
            kVersionIndep
            kUserOverride
        End Enum

        '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        'Use: Generates path for .addin file
        '
        '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Public Shared Function GenerateAddinFileLocation(ByVal mode As RegFreeModeEnum, ByVal version As String) As String

            Dim user As String = System.Environment.UserName

            Dim os As String = CommonUtils.GetOsVersion(False)

            Select Case mode

                Case RegFreeModeEnum.kVersionIndep

                    Select Case os
                        Case "Windows XP"
                            Return "C:\Documents and Settings\All Users\Application Data\Autodesk\Inventor Addins\"

                        Case "Windows Vista", "Windows 7"
                            Return "C:\ProgramData\Autodesk\Inventor Addins\"
                        Case Else

                            Exit Select
                    End Select
                    Exit Select

                Case RegFreeModeEnum.kVersionDep

                    Select Case os
                        Case "Windows XP"
                            Return "C:\Documents and Settings\All Users\Application Data\Autodesk\" & version & "\Addins\"

                        Case "Windows Vista", "Windows 7"
                            Return "C:\ProgramData\Autodesk\" & version & "\Addins\"
                        Case Else

                            Exit Select
                    End Select
                    Exit Select

                Case RegFreeModeEnum.kUserOverride
                    Select Case os
                        Case "Windows XP"
                            Return "C:\Documents and Settings\" & user & "\Application Data\Autodesk\" & version & "\Addins\"

                        Case "Windows Vista", "Windows 7"
                            Return "C:\Users\" & user & "\AppData\Roaming\Autodesk\" & version & "\Addins\"
                        Case Else

                            Exit Select
                    End Select
                    Exit Select
                Case Else

                    Exit Select
            End Select

            Throw New InstallException()
        End Function

        '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        'Use: Registry-Free Install / Uninstall
        '
        '/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        Public Shared Function InstallRegistryFree(ByVal stateSaver As IDictionary, ByVal mode As RegFreeModeEnum, ByVal version As String) As String
            Return InstallRegistryFree(stateSaver, Assembly.GetExecutingAssembly(), mode, version)
        End Function

        Public Shared Function InstallRegistryFree( _
            ByVal stateSaver As IDictionary, _
            ByVal Asm As Assembly, _
            ByVal mode As RegFreeModeEnum, _
            ByVal version As String) As String

            Try
                Dim asmFile As New FileInfo(Asm.Location)

                Dim addinFile As FileInfo = Nothing

                For Each fileInfo As FileInfo In asmFile.Directory.GetFiles()
                    If fileInfo.Extension.ToLower() = ".addin" Then
                        addinFile = fileInfo
                        Exit For
                    End If
                Next

                If addinFile Is Nothing Then
                    Throw New InstallException()
                End If

                Dim xmldoc As New XmlDocument()
                xmldoc.Load(addinFile.FullName)

                Dim node As XmlNode = xmldoc.GetElementsByTagName("Assembly")(0)

                If node Is Nothing Then
                    Throw New InstallException()
                End If

                node.InnerText = asmFile.FullName

                Dim addinFilenameDest As String = GenerateAddinFileLocation(mode, version)

                If Not Directory.Exists(addinFilenameDest) Then
                    Directory.CreateDirectory(addinFilenameDest)
                End If

                addinFilenameDest += addinFile.Name

                If File.Exists(addinFilenameDest) Then
                    File.Delete(addinFilenameDest)
                End If

                ' copy the addin to target folder according to OS type and all users/separate users
                xmldoc.Save(addinFilenameDest)

                addinFile.Delete()

                Return addinFilenameDest
            Catch
                Throw New InstallException("Error installing .addin file!")
            End Try
        End Function

        Public Shared Sub UninstallRegistryFree(ByVal savedState As IDictionary, ByVal pathToAddinFile As String)
            If File.Exists(pathToAddinFile) Then
                File.Delete(pathToAddinFile)
            End If
        End Sub
    End Class

End Namespace
