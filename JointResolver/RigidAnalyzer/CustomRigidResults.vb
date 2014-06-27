Imports Inventor

Public Class CustomRigidResults
    Public groups As New List(Of CustomRigidGroup)
    Public joints As New List(Of CustomRigidJoint)
    Public groupIDToCustom As New Dictionary(Of Integer, CustomRigidGroup)

    Public Sub New(ByRef results As RigidBodyResults)
        For Each group As RigidBodyGroup In results.RigidBodyGroups
            Dim tmp As CustomRigidGroup = New CustomRigidGroup(group)
            If (Not (groupIDToCustom.ContainsKey(group.GroupID))) Then
                groups.Add(tmp)
                groupIDToCustom.Add(group.GroupID, tmp)
            Else
                Console.WriteLine("GroupID Collision: " & groupIDToCustom(group.GroupID).ToString() & " and " & tmp.ToString())
            End If
        Next
        For Each joint As RigidBodyJoint In results.RigidBodyJoints
            joints.Add(New CustomRigidJoint(joint, groupIDToCustom(joint.GroupOne.GroupID), groupIDToCustom(joint.GroupTwo.GroupID)))
        Next
        RigidBodyCleaner.CleanMeaningless(Me)
    End Sub
End Class

Public Class CustomRigidGroup
    Public occurrences As New List(Of ComponentOccurrence)
    Public grounded As Boolean
    Public groupID As Integer

    Public Sub New(ByRef group As RigidBodyGroup)
        For Each comp As ComponentOccurrence In group.Occurrences
            occurrences.Add(comp)
        Next
        grounded = group.Grounded
        groupID = group.GroupID
    End Sub

    Public Overrides Function ToString() As String
        Dim res As String = "#" & Str(groupID) & " ["
        For Each occ As ComponentOccurrence In occurrences
            If res.Length() > 25 Then
                res &= "..."
                Exit For
            End If
            res &= occ.Name & ";"
        Next
        res &= "]"
        If (grounded) Then
            res &= "G"
        End If
        Return res
    End Function
End Class

Public Class CustomRigidJoint
    Public joints As New List(Of AssemblyJoint)
    Public constraints As New List(Of AssemblyConstraint)
    Public groupOne As CustomRigidGroup
    Public groupTwo As CustomRigidGroup
    Public type As RigidBodyJointTypeEnum

    Public Sub New(ByRef joint As RigidBodyJoint, ByRef groupOnez As CustomRigidGroup, ByRef groupTwoz As CustomRigidGroup)
        For Each aj As AssemblyJoint In joint.Joints
            joints.Add(aj)
        Next
        For Each cj As AssemblyConstraint In joint.Constraints
            constraints.Add(cj)
        Next
        groupOne = groupOnez
        groupTwo = groupTwoz
        type = joint.JointType
    End Sub

    Public Overrides Function ToString() As String
        Return "RigidJoint (" & [Enum].GetName(GetType(RigidBodyJointTypeEnum), type) & "): " & constraints.Count & "C, " & joints.Count & "J"
    End Function
End Class
