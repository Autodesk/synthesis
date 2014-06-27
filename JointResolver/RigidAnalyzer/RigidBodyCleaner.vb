Imports Inventor

Module RigidBodyCleaner
    Public Sub CleanMeaningless(ByRef results As CustomRigidResults)
        results.joints.RemoveAll(Function(item) item.groupOne.Equals(item.groupTwo) OrElse item.groupOne.occurrences.Count <= 0 OrElse item.groupTwo.occurrences.Count <= 0)
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
        Dim jointCount As New Dictionary(Of String, String)
        Dim constraintCount As New Dictionary(Of String, String)
        For Each joint As CustomRigidJoint In results.joints
            If joint.joints.Count > 0 Then
                ' Moving joint
                Dim val As Integer
                If Not (joint.groupOne.grounded) Then
                    jointCount.TryGetValue(joint.groupOne.fullQualifier, val)
                    jointCount(joint.groupOne.fullQualifier) = val + 1
                End If
                If Not (joint.groupTwo.grounded) Then
                    jointCount.TryGetValue(joint.groupTwo.fullQualifier, val)
                    jointCount(joint.groupTwo.fullQualifier) = val + 1
                End If
            End If
            If joint.constraints.Count > 0 Then
                ' Moving joint
                Dim val As Integer
                constraintCount.TryGetValue(joint.groupOne.fullQualifier, val)
                constraintCount(joint.groupOne.fullQualifier) = val + 1
                constraintCount.TryGetValue(joint.groupTwo.fullQualifier, val)
                constraintCount(joint.groupTwo.fullQualifier) = val + 1
            End If
        Next joint

        ' Determine first level merge intention
        Dim mergeIntents As New Dictionary(Of String, String) 'Merge key into value
        For Each joint As CustomRigidJoint In results.joints
            If joint.joints.Count > 0 Then Continue For
            Dim oneJointed As Boolean, twoJointed As Boolean
            Dim val As Integer
            jointCount.TryGetValue(joint.groupOne.fullQualifier, val)
            oneJointed = val > 0
            jointCount.TryGetValue(joint.groupTwo.fullQualifier, val)
            twoJointed = val > 0
            If oneJointed AndAlso twoJointed Then Continue For
            If Not (oneJointed) AndAlso Not (twoJointed) Then
                ' Determine best thing to fit to
                Dim groupOneVolume As Double, groupTwoVolume As Double
                For Each component In joint.groupOne.occurrences
                    groupOneVolume += component.MassProperties.Volume
                Next component
                For Each component In joint.groupTwo.occurrences
                    groupTwoVolume += component.MassProperties.Volume
                Next component
                oneJointed = Not (joint.groupTwo.grounded) AndAlso (groupOneVolume > groupTwoVolume OrElse mergeIntents.ContainsKey(joint.groupOne.fullQualifier) OrElse joint.groupOne.grounded) AndAlso Not (mergeIntents.ContainsKey(joint.groupTwo.fullQualifier))
                twoJointed = Not (oneJointed) AndAlso Not (joint.groupOne.grounded)
            End If
            If oneJointed Then
                If (mergeIntents.ContainsKey(joint.groupTwo.fullQualifier)) Then
                    If Not (mergeIntents(joint.groupTwo.fullQualifier).Equals(joint.groupOne.fullQualifier)) Then Console.WriteLine("[WARN] Double jointed on " & _
                        joint.groupTwo.ToString() & vbNewLine & vbTab & _
                        "CURRENT: " & joint.groupOne.ToString() & vbNewLine & vbTab & _
                        "REPLACE: " & results.groupIDToCustom(mergeIntents(joint.groupTwo.fullQualifier)).ToString())
                    Continue For
                End If
                Dim cOVal As String = Nothing
                If mergeIntents.TryGetValue(joint.groupOne.fullQualifier, cOVal) AndAlso cOVal.Equals(joint.groupTwo.fullQualifier) Then Continue For
                mergeIntents(joint.groupTwo.fullQualifier) = joint.groupOne.fullQualifier

                Console.WriteLine("PreMerge " & joint.groupTwo.ToString() & " into " & joint.groupOne.ToString() & " because " & joint.ToString())
            ElseIf twoJointed Then
                If (mergeIntents.ContainsKey(joint.groupOne.fullQualifier)) Then
                    If Not (mergeIntents(joint.groupOne.fullQualifier).Equals(joint.groupTwo.fullQualifier)) Then Console.WriteLine("[WARN] Double jointed on " & _
                        joint.groupOne.ToString() & vbNewLine & vbTab & _
                        "CURRENT: " & joint.groupTwo.ToString() & vbNewLine & vbTab & _
                        "REPLACE: " & results.groupIDToCustom(mergeIntents(joint.groupOne.fullQualifier)).ToString())
                    Continue For
                End If
                Dim cOVal As String = Nothing
                If mergeIntents.TryGetValue(joint.groupTwo.fullQualifier, cOVal) AndAlso cOVal.Equals(joint.groupOne.fullQualifier) Then Continue For
                mergeIntents(joint.groupOne.fullQualifier) = joint.groupTwo.fullQualifier

                Console.WriteLine("PreMerge " & joint.groupOne.ToString() & " into " & joint.groupTwo.ToString() & " because " & joint.ToString())
            End If
        Next joint

        ' Resolve merges and preform merge
        Dim currentKeys(mergeIntents.Keys.Count - 1) As String
        mergeIntents.Keys.CopyTo(currentKeys, 0)
        For Each key As String In currentKeys
            Dim myGroup As CustomRigidGroup = results.groupIDToCustom(key)

            Dim currentMerge As String = mergeIntents(key)
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
            If joint.groupOne.occurrences.Count > 0 AndAlso joint.groupTwo.occurrences.Count > 0 Then Continue For

            If joint.groupTwo.occurrences.Count = 0 Then
                ' Merged joint?
                Dim newOccurrence As String = Nothing
                If (mergeIntents.TryGetValue(joint.groupTwo.fullQualifier, newOccurrence)) Then
                    results.groupIDToCustom.TryGetValue(newOccurrence, joint.groupTwo)
                End If
            End If
            If joint.groupOne.occurrences.Count = 0 Then
                ' Merged joint?
                Dim newOccurrence As String = Nothing
                If (mergeIntents.TryGetValue(joint.groupOne.fullQualifier, newOccurrence)) Then
                    results.groupIDToCustom.TryGetValue(newOccurrence, joint.groupOne)
                End If
            End If
            'Otherwise dispose the joint as needed
        Next
        CleanMeaningless(results)
    End Sub
End Module
