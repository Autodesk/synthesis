Imports Inventor

Module RigidBodyCleaner
    Public Sub CleanMeaningless(ByRef results As CustomRigidResults)
        results.joints.RemoveAll(Function(item) item.groupOne.Equals(item.groupTwo) Or item.groupOne.occurrences.Count <= 0 Or item.groupTwo.occurrences.Count <= 0)
        results.groups.RemoveAll(Function(item) item.occurrences.Count <= 0)
    End Sub

    Public Sub CleanGroundedBodies(ByRef results As CustomRigidResults)
        Dim firstRoot As CustomRigidGroup = Nothing
        For Each group As CustomRigidGroup In results.groups
            If group.grounded Then
                If IsNothing(firstRoot) Then
                    firstRoot = group
                Else
                    firstRoot.occurrences.AddRange(group.occurrences)
                    group.occurrences.Clear()
                End If
            End If
        Next
        If Not (IsNothing(firstRoot)) Then
            For Each joint As CustomRigidJoint In results.joints
                If joint.groupOne.occurrences.Count = 0 Then
                    joint.groupOne = firstRoot
                End If
                If joint.groupTwo.occurrences.Count = 0 Then
                    joint.groupTwo = firstRoot
                End If
            Next
        End If
        CleanMeaningless(results)
    End Sub

    Public Sub CleanConstraintOnly(ByRef results As CustomRigidResults)
        ' Determine what groups move
        Dim jointCount As New Dictionary(Of Integer, Integer)
        Dim constraintCount As New Dictionary(Of Integer, Integer)
        For Each joint As CustomRigidJoint In results.joints
            If joint.joints.Count > 0 Then
                ' Moving joint
                Dim val As Integer
                If Not (joint.groupOne.grounded) Then
                    jointCount.TryGetValue(joint.groupOne.groupID, val)
                    jointCount(joint.groupOne.groupID) = val + 1
                End If
                If Not (joint.groupTwo.grounded) Then
                    jointCount.TryGetValue(joint.groupTwo.groupID, val)
                    jointCount(joint.groupTwo.groupID) = val + 1
                End If
            End If
            If joint.constraints.Count > 0 Then
                ' Moving joint
                Dim val As Integer
                constraintCount.TryGetValue(joint.groupOne.groupID, val)
                constraintCount(joint.groupOne.groupID) = val + 1
                constraintCount.TryGetValue(joint.groupTwo.groupID, val)
                constraintCount(joint.groupTwo.groupID) = val + 1
            End If
        Next joint

        ' Determine first level merge intention
        Dim mergeIntents As New Dictionary(Of Integer, Integer) 'Merge key into value
        For Each joint As CustomRigidJoint In results.joints
            If joint.joints.Count > 0 Then Continue For
            Dim oneJointed As Boolean, twoJointed As Boolean
            Dim val As Integer
            jointCount.TryGetValue(joint.groupOne.groupID, val)
            oneJointed = val > 0
            jointCount.TryGetValue(joint.groupTwo.groupID, val)
            twoJointed = val > 0
            If oneJointed And twoJointed Then Continue For
            If Not (oneJointed) And Not (twoJointed) Then
                ' Determine best thing to fit to
                Dim groupOneVolume As Double, groupTwoVolume As Double
                For Each component In joint.groupOne.occurrences
                    groupOneVolume += component.MassProperties.Volume
                Next component
                For Each component In joint.groupTwo.occurrences
                    groupTwoVolume += component.MassProperties.Volume
                Next component
                oneJointed = Not (joint.groupTwo.grounded) And (groupOneVolume > groupTwoVolume Or mergeIntents.ContainsKey(joint.groupOne.groupID) Or joint.groupOne.grounded) And Not (mergeIntents.ContainsKey(joint.groupTwo.groupID))
                twoJointed = Not (oneJointed) And Not (joint.groupOne.grounded)
            End If
            If oneJointed Then
                If (mergeIntents.ContainsKey(joint.groupTwo.groupID)) Then
                    If mergeIntents(joint.groupTwo.groupID) <> joint.groupOne.groupID Then Console.WriteLine("[WARN] Double jointed on " & _
                        joint.groupTwo.ToString() & vbNewLine & vbTab & _
                        "CURRENT: " & joint.groupOne.ToString() & vbNewLine & vbTab & _
                        "REPLACE: " & results.groupIDToCustom(mergeIntents(joint.groupTwo.groupID)).ToString())
                    Continue For
                End If
                Dim cOVal As Integer
                If mergeIntents.TryGetValue(joint.groupOne.groupID, cOVal) Then If cOVal = joint.groupTwo.groupID Then Continue For
                mergeIntents(joint.groupTwo.groupID) = joint.groupOne.groupID

                Console.WriteLine("PreMerge " & joint.groupTwo.ToString() & " into " & joint.groupOne.ToString() & " because " & joint.ToString())
            ElseIf twoJointed Then
                If (mergeIntents.ContainsKey(joint.groupOne.groupID)) Then
                    If mergeIntents(joint.groupOne.groupID) <> joint.groupTwo.groupID Then Console.WriteLine("[WARN] Double jointed on " & _
                        joint.groupOne.ToString() & vbNewLine & vbTab & _
                        "CURRENT: " & joint.groupTwo.ToString() & vbNewLine & vbTab & _
                        "REPLACE: " & results.groupIDToCustom(mergeIntents(joint.groupOne.groupID)).ToString())
                    Continue For
                End If
                Dim cOVal As Integer
                If mergeIntents.TryGetValue(joint.groupTwo.groupID, cOVal) Then If cOVal = joint.groupOne.groupID Then Continue For
                mergeIntents(joint.groupOne.groupID) = joint.groupTwo.groupID

                Console.WriteLine("PreMerge " & joint.groupOne.ToString() & " into " & joint.groupTwo.ToString() & " because " & joint.ToString())
            End If
        Next joint

        ' Resolve merges and preform merge
        Dim currentKeys(mergeIntents.Keys.Count - 1) As Integer
        mergeIntents.Keys.CopyTo(currentKeys, 0)
        For Each key As Integer In currentKeys
            Dim myGroup As CustomRigidGroup = results.groupIDToCustom(key)

            Dim currentMerge As Integer = mergeIntents(key)
            While mergeIntents.ContainsKey(currentMerge)
                currentMerge = mergeIntents(currentMerge)
            End While
            mergeIntents(key) = currentMerge

            Dim mergeInto As CustomRigidGroup = results.groupIDToCustom(mergeIntents(key))
            Console.WriteLine("FinalMerge " & myGroup.ToString() & " into " & mergeInto.ToString())
            Console.WriteLine()

            mergeInto.occurrences.AddRange(myGroup.occurrences)
            myGroup.occurrences.Clear()
        Next key

        ' Clean up joints to use new groups
        For Each joint As CustomRigidJoint In results.joints
            If joint.groupOne.occurrences.Count > 0 And joint.groupTwo.occurrences.Count > 0 Then Continue For

            If joint.groupTwo.occurrences.Count = 0 Then
                ' Merged joint?
                Dim newOccurrence As Integer
                If (mergeIntents.TryGetValue(joint.groupTwo.groupID, newOccurrence)) Then
                    results.groupIDToCustom.TryGetValue(newOccurrence, joint.groupTwo)
                End If
            End If
            If joint.groupOne.occurrences.Count = 0 Then
                ' Merged joint?
                Dim newOccurrence As Integer
                If (mergeIntents.TryGetValue(joint.groupOne.groupID, newOccurrence)) Then
                    results.groupIDToCustom.TryGetValue(newOccurrence, joint.groupOne)
                End If
            End If
            'Otherwise dispose the joint as needed
        Next
        CleanMeaningless(results)
    End Sub
End Module
