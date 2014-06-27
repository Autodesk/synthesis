Imports Inventor

Public Class CustomRigidResults
    Public groups As New List(Of CustomRigidGroup)
    Public joints As New List(Of CustomRigidJoint)
    Public groupIDToCustom As New Dictionary(Of String, CustomRigidGroup)

    Public Sub New(ByRef results As RigidBodyResults)
        Console.WriteLine("Building custom dataset")
        For Each group As RigidBodyGroup In results.RigidBodyGroups
            Dim tmp As CustomRigidGroup = New CustomRigidGroup(group)
            If (Not (groupIDToCustom.ContainsKey(CustomRigidGroup.GetGroupQualifier(group)))) Then
                groups.Add(tmp)
                groupIDToCustom.Add(CustomRigidGroup.GetGroupQualifier(group), tmp)
            Else
                Console.WriteLine("GroupID Collision: " & groupIDToCustom(CustomRigidGroup.GetGroupQualifier(group)).ToString() & " and " & tmp.ToString())
            End If
        Next
        For Each joint As RigidBodyJoint In results.RigidBodyJoints
            joints.Add(New CustomRigidJoint(joint, groupIDToCustom(CustomRigidGroup.GetGroupQualifier(joint.GroupOne)), groupIDToCustom(CustomRigidGroup.GetGroupQualifier(joint.GroupTwo))))
        Next
        Console.WriteLine("Built custom dataset")
        RigidBodyCleaner.CleanMeaningless(Me)
    End Sub
End Class

Public Class CustomRigidGroup
    Public occurrences As New List(Of ComponentOccurrence)
    Public grounded As Boolean

    Public fullQualifier As String

    Public Shared Function GetGroupQualifier(ByRef group As RigidBodyGroup) As String
        Return Str(group.GroupID) & "_" & group.Parent.Parent.Parent.Parent.InternalName
    End Function

    Public Sub New(ByRef group As RigidBodyGroup)
        For Each comp As ComponentOccurrence In group.Occurrences
            occurrences.Add(comp)
        Next
        grounded = group.Grounded
        fullQualifier = GetGroupQualifier(group)
    End Sub

    Public Overrides Function ToString() As String
        Dim res As String = "#" & fullQualifier & " ["
        For Each occ As ComponentOccurrence In occurrences
            If res.Length() > 100 Then
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

    Public Overrides Function Equals(obj As Object) As Boolean
        If (TypeOf obj Is CustomRigidGroup) Then
            Return fullQualifier.Equals(obj.fullQualifier)
        ElseIf (TypeOf obj Is RigidBodyGroup) Then
            Return fullQualifier.Equals(GetGroupQualifier(obj))
        Else
            Return False
        End If
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return fullQualifier.GetHashCode()
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
