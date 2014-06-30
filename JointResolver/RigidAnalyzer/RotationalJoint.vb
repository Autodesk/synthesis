Imports Inventor

Module RotationalJoint
    Public Function isRotationalJoint(ByRef jointI As CustomRigidJoint) As Boolean
        If jointI.joints.Count = 1 Then
            Dim joint As AssemblyJointDefinition = jointI.joints.Item(0).Definition
            Return joint.JointType = AssemblyJointTypeEnum.kRotationalJointType OrElse _
                (joint.JointType = AssemblyJointTypeEnum.kCylindricalJointType AndAlso _
                 (Not (joint.HasAngularPositionLimits) OrElse joint.AngularPositionEndLimit.Value <> joint.AngularPositionStartLimit.Value))
        End If
        Return False
    End Function

    Public Function getInfo(ByRef jointI As CustomRigidJoint, ByRef parent As CustomRigidGroup)
        If Not (isRotationalJoint(jointI)) Then Return Nothing

        Dim joint As AssemblyJointDefinition = jointI.joints.Item(0).Definition
        Dim child As CustomRigidGroup = Nothing
        Dim first As Boolean = False
        If (jointI.groupOne.Equals(parent)) Then
            child = jointI.groupTwo
            first = True
        ElseIf jointI.groupTwo.Equals(parent) Then
            child = jointI.groupOne
        End If
        If IsNothing(child) Then Return Nothing

        Dim groupANormal, groupBNormal As UnitVector
        Dim groupABase, groupBBase As Point

        If (joint.JointType = AssemblyJointTypeEnum.kCylindricalJointType) Then
            groupANormal = joint.OriginOne.Geometry.Geometry.AxisVector
            groupABase = joint.OriginOne.Geometry.Geometry.BasePoint
            groupBNormal = joint.OriginTwo.Geometry.Geometry.AxisVector
            groupBBase = joint.OriginTwo.Geometry.Geometry.BasePoint
        Else
            groupANormal = joint.OriginOne.Geometry.Geometry.Normal
            groupABase = joint.OriginOne.Geometry.Geometry.Center
            groupBNormal = joint.OriginTwo.Geometry.Geometry.Normal
            groupBBase = joint.OriginTwo.Geometry.Geometry.Center
        End If

        Dim currentAngularPosition As Double = If(Not (IsNothing(joint.AngularPosition)), joint.AngularPosition.Value, 0)
        Dim hasAngularLimit As Boolean = joint.HasAngularPositionLimits
        Dim angularLimitLow, angularLimitHigh As Double
        If (hasAngularLimit) Then
            angularLimitLow = joint.AngularPositionStartLimit.Value
            angularLimitHigh = joint.AngularPositionEndLimit.Value
        End If
        Return Main.printVector(If(first, groupABase, groupBBase)) & ":" & Main.printVector(If(first, groupANormal, groupBNormal)) & ":" & Main.printVector(If(first, groupBBase, groupABase)) & ":" & Main.printVector(If(first, groupBNormal, groupANormal))
    End Function
End Module
