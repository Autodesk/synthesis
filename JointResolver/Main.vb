Imports Inventor
Imports System.IO

Module Main
    Public invApplication As Application
    Private Const MAX_VERTICIES As Integer = 8192
    Public Sub Main()
        invApplication = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")
        AnalyzeRigidResults()

        'Dim asmDoc As AssemblyDocument = invApplication.ActiveDocument
        'Dim surfs As New SurfaceExporter
        'surfs.ExportAll(asmDoc.ComponentDefinition.Occurrences)

        'Write as stl
        'surfs.WriteSTL("C:\Users\t_millw\Downloads\asm.stl")
        'Console.WriteLine("Finished")
        'Console.ReadLine()
    End Sub

    Public Function WorldTransformation(ByRef comp As ComponentOccurrence) As Matrix
        Dim trans As Matrix = invApplication.TransientGeometry.CreateMatrix()
        trans.SetToIdentity()
        If Not (IsNothing(comp.ParentOccurrence)) Then
            trans.TransformBy(WorldTransformation(comp.ParentOccurrence))
        End If
        trans.TransformBy(comp.Transformation)
        Return trans
    End Function

    Public Sub AnalyzeRigidResults()
        Dim asmDoc As AssemblyDocument = invApplication.ActiveDocument
        Console.WriteLine("Get rigid info...")
        Dim rigidResults As RigidBodyResults = asmDoc.ComponentDefinition.RigidBodyAnalysis(invApplication.TransientObjects.CreateNameValueMap())
        Console.WriteLine("Got rigid info...")
        Dim customRigid As New CustomRigidResults(rigidResults)

        Console.WriteLine("Built model...")
        RigidBodyCleaner.CleanGroundedBodies(customRigid)
        Console.WriteLine("Cleaned grounded; " & Str(customRigid.groups.Count) & " groups and " & Str(customRigid.joints.Count) & " joints remain.")
        RigidBodyCleaner.CleanConstraintOnly(customRigid)
        Console.WriteLine("Clean constraints 1; " & Str(customRigid.groups.Count) & " groups and " & Str(customRigid.joints.Count) & " joints remain.")
        RigidBodyCleaner.CleanConstraintOnly(customRigid) ' Repeat gives a better join
        Console.WriteLine("Clean constraints 2; " & Str(customRigid.groups.Count) & " groups and " & Str(customRigid.joints.Count) & " joints remain.")

        For Each group As CustomRigidGroup In customRigid.groups
            Console.WriteLine("RigidGroup " & group.ToString())
        Next

        Console.WriteLine("")

        'For Each joint As CustomRigidJoint In customRigid.joints
        ' Console.WriteLine("RigidJoint (" & [Enum].GetName(GetType(RigidBodyJointTypeEnum), joint.type) & "): " & joint.constraints.Count & " constraints, " & joint.joints.Count & " joints between " & NameRigidGroup(joint.groupOne) & " and " & NameRigidGroup(joint.groupTwo))
        ' Next
        Console.WriteLine(Str(customRigid.joints.Count) & " joints remain")

        Dim baseNode As RigidNode = RigidNode.generateNodeTree(customRigid)

        ' Build the file structure
        Console.WriteLine(baseNode.ToString())
        Dim nodes As New List(Of RigidNode)
        baseNode.listAllNodes(nodes)
        Dim pathBase As String = "C:\Users\t_millw\Downloads\skele\"
        Dim surfs As New SurfaceExporter
        Dim writer As StreamWriter = New StreamWriter(pathBase + "meta.txt")
        For i As Integer = 0 To nodes.Count - 1
            writer.Write(Str(i) & ":")
            Dim path As String = nodes.Item(i).group.occurrences.Item(0).Name.Replace(":", "_") & ".stl"
            writer.Write(path & ":")
            surfs.Reset()
            surfs.ExportAll(nodes.Item(i).group.occurrences)
            surfs.WriteSTL(pathBase & path)
            If IsNothing(nodes.Item(i).parent) Then
                writer.Write("-1,")
            Else
                writer.Write(Str(nodes.IndexOf(nodes.Item(i).parent)) & ":")
                Dim joint As CustomRigidJoint = nodes.Item(i).parentConnection
                If RotationalJoint.isRotationalJoint(joint) Then
                    writer.Write(RotationalJoint.getInfo(joint, nodes.Item(i).parent.group))
                End If
            End If
            writer.WriteLine()
        Next i
        writer.Close()

        'Dim comp As ComponentOccurrence = Nothing
        'comp()

        ' TODO
        ' [X] Merge all grounded groups
        ' [~] Drop all joints with only constraints and merge any rigid groups separated only by constraint joints.
        ' [X] Determine heirarchy based on the first observed grounded rigid group.  Work up from there along the rigid joints.
        ' Determine pivot structure for each type of RigidBodyJoint
        ' FBX Exporter thing!  C++ program with VB binding
    End Sub

    Public Function printVector(p As Object) As String
        printVector = (Str(p.X) & "," & Str(p.Y) & "," & Str(p.Z))
    End Function
End Module
