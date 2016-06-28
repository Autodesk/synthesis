Imports Inventor
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Windows.Forms
Imports stdole
Imports System.Drawing



Namespace BrowserSample
    <ProgIdAttribute("BrowserSample.StandardAddInServer"), _
    GuidAttribute("ec113dad-f7a5-4a6a-a070-315e0fc39383")> _
    Public Class StandardAddInServer
        Implements Inventor.ApplicationAddInServer

        ' Inventor application object.
        Private m_inventorApplication As Inventor.Application
        'button of adding tree view browser
        Private WithEvents m_TreeViewBrowser As Inventor.ButtonDefinition
        'button of adding ActiveX browser
        Private WithEvents m_ActiveXBrowser As Inventor.ButtonDefinition
        'button of starting or stopping BrowserEvents
        Private WithEvents m_DoBrowserEvents As Inventor.ButtonDefinition
        'BrowserEvents
        Private WithEvents m_BrowserEvents As Inventor.BrowserPanesEvents
        'HighlightSet
        Private oHighlight As Inventor.HighlightSet

        Private m_ClientId As String
        Private m_ActiveX As UserControl1


#Region "ApplicationAddInServer Members"

        Public Sub Activate(ByVal addInSiteObject As Inventor.ApplicationAddInSite, ByVal firstTime As Boolean) Implements Inventor.ApplicationAddInServer.Activate

            ' This method is called by Inventor when it loads the AddIn.
            ' The AddInSiteObject provides access to the Inventor Application object.
            ' The FirstTime flag indicates if the AddIn is loaded for the first time.

            ' Initialize AddIn members.
            m_inventorApplication = addInSiteObject.Application

            m_ClientId = "{ec113dad-f7a5-4a6a-a070-315e0fc39383}"

            ' TODO:  Add ApplicationAddInServer.Activate implementation.
            ' e.g. event initialization, command creation etc.
            Dim largeIconSize As Integer
            If m_inventorApplication.UserInterfaceManager.InterfaceStyle = InterfaceStyleEnum.kRibbonInterface Then
                largeIconSize = 32
            Else
                largeIconSize = 24
            End If

            Dim controlDefs As ControlDefinitions = m_inventorApplication.CommandManager.ControlDefinitions
            Dim smallPicture1 As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.Icon1.ToBitmap())
            Dim largePicture1 As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.Icon1.ToBitmap())
            m_TreeViewBrowser = controlDefs.AddButtonDefinition("HierarchyPane", "BrowserSample:HierarchyPane", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, , , smallPicture1, largePicture1)

            Dim smallPicture2 As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.Icon2.ToBitmap())
            Dim largePicture2 As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.Icon2.ToBitmap())
            m_ActiveXBrowser = controlDefs.AddButtonDefinition("ActiveXPane", "BrowserSample:ActiveXPane", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, , , smallPicture2, largePicture2)


            Dim smallPicture3 As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.Icon3.ToBitmap())
            Dim largePicture3 As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.Icon3.ToBitmap())
            m_DoBrowserEvents = controlDefs.AddButtonDefinition("DoBrowserEvents", "BrowserSample:DoBrowserEvents", CommandTypesEnum.kNonShapeEditCmdType, m_ClientId, , , smallPicture3, largePicture3)


            If firstTime Then
                ' Get the assembly ribbon.
                Dim partRibbon As Inventor.Ribbon = m_inventorApplication.UserInterfaceManager.Ribbons.Item("Part")
                ' Get the "Part" tab.
                Dim partTab As Inventor.RibbonTab = partRibbon.RibbonTabs(1)
                Dim partPanel As Inventor.RibbonPanel = partTab.RibbonPanels(1)
                partPanel.CommandControls.AddButton(m_TreeViewBrowser, True)
                partPanel.CommandControls.AddButton(m_DoBrowserEvents)
                partPanel.CommandControls.AddButton(m_ActiveXBrowser)

            End If

        End Sub

        Public Sub Deactivate() Implements Inventor.ApplicationAddInServer.Deactivate

            ' This method is called by Inventor when the AddIn is unloaded.
            ' The AddIn will be unloaded either manually by the user or
            ' when the Inventor session is terminated.

            ' TODO:  Add ApplicationAddInServer.Deactivate implementation
            Marshal.ReleaseComObject(m_TreeViewBrowser)
            m_TreeViewBrowser = Nothing

            Marshal.ReleaseComObject(m_ActiveXBrowser)
            m_ActiveXBrowser = Nothing

            Marshal.ReleaseComObject(m_DoBrowserEvents)
            m_DoBrowserEvents = Nothing

            Marshal.ReleaseComObject(oHighlight)
            oHighlight = Nothing

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

 

        ''' <summary>
        ''' When [HierarchicalBrowser] button is clicked
        ''' </summary>
        ''' <param name="Context"></param>
        ''' <remarks></remarks>
        Private Sub m_HierarchicalBrowser_OnExecute(ByVal Context As Inventor.NameValueMap) Handles m_TreeViewBrowser.OnExecute

            Dim oDoc As Document
            oDoc = m_inventorApplication.ActiveDocument

            Dim oPanes As BrowserPanes
            oPanes = oDoc.BrowserPanes

            'Create a standard Microsoft Windows IPictureDisp referencing an icon (.bmp) bitmap file.
            'Change the file referenced here as appropriate - here the code references test.bmp.
            'This is the icon that will be displayed at this node. Add the IPictureDisp to the client node resource.

            Dim oRscs As ClientNodeResources
            oRscs = oPanes.ClientNodeResources

            Dim clientNodeIcon As stdole.IPictureDisp = AxHostConverter.ImageToPictureDisp(My.Resources.test)

            Dim oRsc As ClientNodeResource
            oRsc = oRscs.Add(m_ClientId, 1, clientNodeIcon)

            Dim oDef As BrowserNodeDefinition
            oDef = oPanes.CreateBrowserNodeDefinition("Top Node", 3, oRsc)

            'adding a new pane tab to the panes collection, define the top node the pane will contain.
            Dim oPane As Inventor.BrowserPane
            oPane = oPanes.AddTreeBrowserPane("My Pane", m_ClientId, oDef)

            'Add two child nodes to the tree, labeled Node 2 and Node 3.
            Dim oDef1 As BrowserNodeDefinition
            Dim oNode1 As BrowserNode

            oDef1 = oPanes.CreateBrowserNodeDefinition("Node2", 5, oRsc)
            oNode1 = oPane.TopNode.AddChild(oDef1)

            Dim oDef2 As BrowserNodeDefinition
            Dim oNode2 As BrowserNode

            oDef2 = oPanes.CreateBrowserNodeDefinition("Node3", 6, oRsc)
            oNode2 = oPane.TopNode.AddChild(oDef2)

            'Add the native node (from root)  of "Model" pane to the tree
            Dim oNativeRootNode As BrowserNode
            oNativeRootNode = oDoc.BrowserPanes("Model").TopNode

            Call oPane.TopNode.AddChild(oNativeRootNode.BrowserNodeDefinition)
        End Sub

        ''' <summary>
        ''' when [ActiveXBrowser] button is clicked
        ''' </summary>
        ''' <param name="Context"></param>
        ''' <remarks></remarks>
        Private Sub m_ActiveXBrowser_OnExecute(ByVal Context As Inventor.NameValueMap) Handles m_ActiveXBrowser.OnExecute

            'get active document
            Dim oDoc As Document
            oDoc = m_inventorApplication.ActiveDocument

            'get the BrowserPanes
            Dim oPanes As BrowserPanes
            oPanes = oDoc.BrowserPanes

            'add the BrowserPane with the control
            Dim oPane As BrowserPane
            oPane = oPanes.Add("MyActiveXPane", "BrowserSample.UserControl1")

            'get the control
            m_ActiveX = oPane.Control

            'call a method of the control
            m_ActiveX.DrawASketchRectangle(m_inventorApplication)

            'activate the BrowserPane
            oPane.Activate()

        End Sub

        ''' <summary>
        ''' when [DoBrowserEvents] button is clicked.
        ''' </summary>
        ''' <param name="Context"></param>
        ''' <remarks></remarks>
        Private Sub m_DoBrowserEvents_OnExecute(ByVal Context As Inventor.NameValueMap) Handles m_DoBrowserEvents.OnExecute
            If m_DoBrowserEvents.Pressed = False Then
                MsgBox("BrowserEvents Starts!")

                m_DoBrowserEvents.Pressed = True
 
                m_BrowserEvents = m_inventorApplication.ActiveDocument.BrowserPanes.BrowserPanesEvents

            Else
            MsgBox("BrowserEvents Stops!")

            m_DoBrowserEvents.Pressed = False

            m_BrowserEvents = Nothing
            End If
        End Sub

        ''' <summary>
        ''' fire when custom node  is activated
        ''' </summary>
        ''' <param name="BrowserNodeDefinition"></param>
        ''' <param name="Context"></param>
        ''' <param name="HandlingCode"></param>
        ''' <remarks></remarks>
        Private Sub m_BrowserEvents_OnBrowserNodeActivate(ByVal BrowserNodeDefinition As Object, ByVal Context As Inventor.NameValueMap, ByRef HandlingCode As Inventor.HandlingCodeEnum) Handles m_BrowserEvents.OnBrowserNodeActivate
            MsgBox("OnBrowserNodeActivate")
        End Sub

        ''' <summary>
        ''' delete custom nodes
        ''' </summary>
        ''' <param name="BrowserNodeDefinition"></param>
        ''' <param name="BeforeOrAfter"></param>
        ''' <param name="Context"></param>
        ''' <param name="HandlingCode"></param>
        ''' <remarks></remarks>
        Private Sub m_BrowserEvents_OnBrowserNodeDeleteEntry(ByVal BrowserNodeDefinition As Object, ByVal BeforeOrAfter As Inventor.EventTimingEnum, ByVal Context As Inventor.NameValueMap, ByRef HandlingCode As Inventor.HandlingCodeEnum) Handles m_BrowserEvents.OnBrowserNodeDeleteEntry
            MsgBox("OnBrowserNodeDeleteEntry")
            'do deletion by the client
            If BeforeOrAfter = EventTimingEnum.kAfter Then

                Dim oBND As ClientBrowserNodeDefinition
                oBND = BrowserNodeDefinition
                oBND.Delete()

            End If
        End Sub

        Private Sub m_BrowserEvents_OnBrowserNodeGetDisplayObjects(ByVal BrowserNodeDefinition As Object, ByRef Objects As Inventor.ObjectCollection, ByVal Context As Inventor.NameValueMap, ByRef HandlingCode As Inventor.HandlingCodeEnum) Handles m_BrowserEvents.OnBrowserNodeGetDisplayObjects

            Dim oPartDocument As PartDocument = m_inventorApplication.ActiveDocument
            Dim oPartDef As PartComponentDefinition = oPartDocument.ComponentDefinition

            If oHighlight Is Nothing Then
                oHighlight = oPartDocument.CreateHighlightSet
            Else
                oHighlight.Clear()
            End If

            Dim oColor As Inventor.Color
            oColor = m_inventorApplication.TransientObjects.CreateColor(128, 22, 22)

            ' Set the opacity
            oColor.Opacity = 0.8
            oHighlight.Color = oColor

            If TypeOf BrowserNodeDefinition Is ClientBrowserNodeDefinition Then
                Dim oClientB As ClientBrowserNodeDefinition = BrowserNodeDefinition
                'highlight all ExtrudeFeature
                If oClientB.Label = "Node2" Then
                    Dim oExtrudeF As ExtrudeFeature
                    For Each oExtrudeF In oPartDef.Features.ExtrudeFeatures
                        oHighlight.AddItem(oExtrudeF)
                    Next
                    'highlight all RevolveFeature
                ElseIf oClientB.Label = "Node3" Then
                    Dim oRevolveF As RevolveFeature
                    For Each oRevolveF In oPartDef.Features.RevolveFeatures
                        oHighlight.AddItem(oRevolveF)
                    Next
                End If
            End If

        End Sub

        Private Sub m_BrowserEvents_OnBrowserNodeLabelEdit(ByVal BrowserNodeDefinition As Object, ByVal NewLabel As String, ByVal BeforeOrAfter As Inventor.EventTimingEnum, ByVal Context As Inventor.NameValueMap, ByRef HandlingCode As Inventor.HandlingCodeEnum) Handles m_BrowserEvents.OnBrowserNodeLabelEdit
            MsgBox("OnBrowserNodeLabelEdit")
        End Sub
    End Class


    ''from http://blogs.msdn.com/b/andreww/archive/2007/07/30/converting-between-ipicturedisp-and-system-drawing-image.aspx

    Friend Class AxHostConverter
        Inherits AxHost
        Private Sub New()
            MyBase.New("")
        End Sub


        Public Shared Function ImageToPictureDisp(image As Image) As stdole.IPictureDisp
            Return DirectCast(GetIPictureDispFromPicture(image), stdole.IPictureDisp)
        End Function


        Public Shared Function PictureDispToImage(pictureDisp As stdole.IPictureDisp) As Image
            Return GetPictureFromIPicture(pictureDisp)
        End Function
    End Class

End Namespace

