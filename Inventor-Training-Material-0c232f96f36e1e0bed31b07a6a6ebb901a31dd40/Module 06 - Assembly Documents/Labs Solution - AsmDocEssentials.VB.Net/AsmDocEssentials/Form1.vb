
Imports Inventor

Public Class Form1

    Dim mApp As Inventor.Application
    Dim mAngleBox As DigitTextBox

    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mAngleBox = New DigitTextBox
        mAngleBox.Name = "AngleBox"
        mAngleBox.Location = New System.Drawing.Point(80, 190)
        mAngleBox.Size = New System.Drawing.Size(48, 30)
        mAngleBox.TabIndex = 3
        mAngleBox.Text = ""

        Me.Controls.Add(mAngleBox)

        mApp = oApp

    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        VectorControl1.SetApp = mApp
        VectorControl2.SetApp = mApp

    End Sub

    'Create new assembly document
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim oAsm As AssemblyDocument = _
            mApp.Documents.Add( _
            DocumentTypeEnum.kAssemblyDocumentObject, _
            mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kAssemblyDocumentObject), _
            True)

    End Sub

 


    'Add an occurrence, prompts user to select an existing Inventor Part or Assembly
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click

        If (Not mApp.ActiveDocument Is Nothing) Then

            If (mApp.ActiveDocument.DocumentType = DocumentTypeEnum.kAssemblyDocumentObject) Then

                Dim oAsm As AssemblyDocument = mApp.ActiveDocument

                Dim oDLG As FileDialog = Nothing
                mApp.CreateFileDialog(oDLG)

                'oDLG.FileName = "C:\Temp\"
                oDLG.Filter = "Inventor Files (*.iam;*.ipt)|*.iam;*.ipt"
                oDLG.DialogTitle = "Insert occurrence"

                oDLG.ShowOpen()

                If (oDLG.FileName <> "") Then

                    Dim pos As Matrix = mApp.TransientGeometry.CreateMatrix
                    Dim oNewOcc = oAsm.ComponentDefinition.Occurrences.Add(oDLG.FileName, pos)

                    mApp.ActiveView.Update()

                End If

            Else
                System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error")
                Exit Sub
            End If

        Else
            System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error")
            Exit Sub
        End If

    End Sub

    'Transform an occurrence. Occurrence needs to be selected first through the UI
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        If (Not mApp.ActiveDocument Is Nothing) Then

            If (mApp.ActiveDocument.DocumentType = DocumentTypeEnum.kAssemblyDocumentObject) Then

                If (mApp.ActiveDocument.SelectSet.Count = 1) Then

                    If (TypeOf (mApp.ActiveDocument.SelectSet(1)) Is ComponentOccurrence) Then

                        Dim oCompOccurrence As ComponentOccurrence = mApp.ActiveDocument.SelectSet(1)

                        Dim oTransfo As Matrix = mApp.TransientGeometry.CreateMatrix()

                        If (mAngleBox.Text.Length = 0) Then
                            mAngleBox.Text = "0"
                        End If

                        Dim angle As Double = System.Double.Parse(mAngleBox.Text)

                        Dim trans As Vector = VectorControl1.Vector
                        Dim axis As Vector = VectorControl2.Vector

                        If (axis.Length = 0) Then
                            System.Windows.Forms.MessageBox.Show("Rotation Axis cannot be null", "Error")
                            Exit Sub
                        End If

                        oTransfo.SetToRotation(angle * Math.Atan(1) * 4 / 180.0, axis, oCompOccurrence.MassProperties.CenterOfMass)

                        Dim oFinalTx As Vector = oTransfo.Translation
                        oFinalTx.AddVector(trans)

                        oTransfo.SetTranslation(oFinalTx)

                        Dim oNewTransfo As Matrix = oCompOccurrence.Transformation

                        oNewTransfo.TransformBy(oTransfo)

                        oCompOccurrence.Transformation = oNewTransfo

                        mApp.ActiveView.Update()

                    Else
                        System.Windows.Forms.MessageBox.Show("Not an occurrence...", "Error")
                        Exit Sub
                    End If

                Else
                    System.Windows.Forms.MessageBox.Show("A single occurrence must be selected...", "Error")
                    Exit Sub
                End If

            Else
                System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error")
                Exit Sub
            End If

        Else
            System.Windows.Forms.MessageBox.Show("An Assembly document must be active...", "Error")
            Exit Sub
        End If

    End Sub

    #Region "Lab Demo - Constraint"
    Private sub  CreatePart1()  
        
        ' create a new part
        Dim oDoc As PartDocument = mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject)
        Dim oDef As PartComponentDefinition = oDoc.ComponentDefinition

        Dim oTG As TransientGeometry = mApp.TransientGeometry 

        ' create sketch elements
        Dim oSketch As PlanarSketch = oDef.Sketches.Add(oDef.WorkPlanes(3))
        Dim oCircle As SketchCircle = 
            oSketch.SketchCircles.AddByCenterRadius(oTG.CreatePoint2d(0,0),1)

        Dim oProfile As Profile = oSketch.Profiles.AddForSolid()

        ' create a cylinder feature
        Dim oExtrudDef As ExtrudeDefinition = oDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile,PartFeatureOperationEnum.kJoinOperation)
        oExtrudDef.SetDistanceExtent(5,PartFeatureExtentDirectionEnum.kPositiveExtentDirection) 
        Dim oExtrudeF As ExtrudeFeature =  oDef.Features.ExtrudeFeatures.Add(oExtrudDef) 

        'add an attribute to cylinder face         
        Dim oFace As Face = oExtrudeF.SideFaces(1)

        Dim oAttSet As AttributeSet
        Dim oAtt As Attribute 
        oAttSet = oFace.AttributeSets.Add("demoAttset")
        oAtt = oAttSet.Add("demoAtt", ValueTypeEnum.kStringType, "namedEdge")
        If System.IO.File.Exists("c:\temp\test1.ipt") Then
            System.IO.File.Delete("c:\temp\test1.ipt")
        End If
        oDoc.SaveAs("c:\temp\test1.ipt", False)


    End Sub

    Private sub CreatePart2()
            
        ' create a new part
        Dim oDoc As PartDocument = mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject)
        Dim oDef As PartComponentDefinition = oDoc.ComponentDefinition

        Dim oTG As TransientGeometry = mApp.TransientGeometry 

        ' create sketch elements
        Dim oSketch As PlanarSketch = oDef.Sketches.Add(oDef.WorkPlanes(3))  
        oSketch.SketchLines.AddAsTwoPointRectangle(oTG.CreatePoint2d(-5,-5),oTG.CreatePoint2d(5,5))
         
        Dim oSketchPt As SketchPoint = oSketch.SketchPoints.Add(oTG.CreatePoint2d(0,0))      
         
        Dim oProfile As Profile = oSketch.Profiles.AddForSolid()
           ' create a plate with a hole feature
        Dim oExtrudDef As ExtrudeDefinition = oDef.Features.ExtrudeFeatures.CreateExtrudeDefinition(oProfile,PartFeatureOperationEnum.kJoinOperation)
        oExtrudDef.SetDistanceExtent(1,PartFeatureExtentDirectionEnum.kPositiveExtentDirection) 
        Dim oExtrudeF As ExtrudeFeature =  oDef.Features.ExtrudeFeatures.Add(oExtrudDef) 
        
          ' Create an object collection for the hole center points. 
        Dim oHoleCenters As ObjectCollection 
        oHoleCenters = mApp.TransientObjects.CreateObjectCollection 
         
        oHoleCenters.Add(oSketchPt) 

        ' create hole feature
        Dim  oHPdef As HolePlacementDefinition = oDef.Features.HoleFeatures.CreateSketchPlacementDefinition(oHoleCenters)
       
         Dim oHoleF As HoleFeature   = oDef.Features.HoleFeatures.AddDrilledByThroughAllExtent（ _ 
oHPdef, "2",PartFeatureExtentDirectionEnum.kNegativeExtentDirection )  
        
          'add an attribute to cylinder face of the hole         
        Dim oFace As Face = oHoleF.SideFaces(1)
         Dim oAttSet As AttributeSet
        Dim oAtt As Attribute 
        oAttSet = oFace.AttributeSets.Add("demoAttset")
        oAtt = oAttSet.Add("demoAtt",ValueTypeEnum.kStringType,"namedEdge") 
      
        If System.IO.File.Exists("c:\temp\test2.ipt") Then
            System.IO.File.Delete("c:\temp\test2.ipt")
        End If
        oDoc.SaveAs("c:\temp\test2.ipt",False) 

    End sub


    Private Sub insertPartsAndMateEdges()
        
        ' create an assembly
        Dim oAssDoc As AssemblyDocument = mApp.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject)
        Dim oAssDef As AssemblyComponentDefinition = oAssDoc.ComponentDefinition

        Dim oM As Matrix = mApp.TransientGeometry.CreateMatrix()

        'place the two parts
        Dim oOcc1 As ComponentOccurrence = oAssDef.Occurrences.Add("c:\temp\test1.ipt",oM)
        Dim oOcc2 As ComponentOccurrence = oAssDef.Occurrences.Add("c:\temp\test2.ipt",oM)

        ' find the two faces to mate
        Dim oDoc1 As PartDocument = oOcc1.Definition.Document
        Dim oObjCollection As ObjectCollection = oDoc1.AttributeManager.FindObjects("demoAttset","demoAtt")

        Dim oFace1 As face  
        If TypeOf oObjCollection(1) is face then
             oFace1 = oObjCollection(1)
        End If

        oObjCollection = oOcc2.Definition.Document.AttributeManager.FindObjects("demoAttset","demoAtt")

        Dim oFace2 As face 
        If TypeOf oObjCollection(1) is face then
             oFace2 = oObjCollection(1)
        End If
        
        'create the proxy objects for the two faces
        Dim oAsmProxyFace1 As faceProxy 
        Call oOcc1.CreateGeometryProxy(oFace1, oAsmProxyFace1)
 
        Dim oAsmProxyFace2 As faceProxy 
        Call oOcc2.CreateGeometryProxy(oFace2, oAsmProxyFace2)

        ' add the mate constraint
          Call oAssDef.Constraints.AddMateConstraint(oAsmProxyFace1, oAsmProxyFace2, 0)

    End Sub
Private Sub Button4_Click( ByVal sender As System.Object,  ByVal e As System.EventArgs) Handles Button4.Click
        
        ' create part1
        CreatePart1()
        ' create part2
        CreatePart2()
        ' create assembly, place the two parts and mate two faces
        insertPartsAndMateEdges()
End Sub
    #End Region

End Class
