Imports Inventor

Module Main
    Public invApplication As Application
    Public Sub Main()
        invApplication = System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application")
        AnalyzeRigidResults()
        Console.ReadLine()
    End Sub

    Public Sub AnalyzeRigidResults()
        Dim asmDoc As AssemblyDocument = invApplication.ActiveDocument
        Dim rigidResults As RigidBodyResults = asmDoc.ComponentDefinition.RigidBodyAnalysis(invApplication.TransientObjects.CreateNameValueMap())
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
        Console.WriteLine(baseNode.ToString())

        ' TODO
        ' [X] Merge all grounded groups
        ' [~] Drop all joints with only constraints and merge any rigid groups separated only by constraint joints.
        ' [X] Determine heirarchy based on the first observed grounded rigid group.  Work up from there along the rigid joints.
        ' Determine pivot structure for each type of RigidBodyJoint
        ' FBX Exporter thing!  C++ program with VB binding
    End Sub
End Module
