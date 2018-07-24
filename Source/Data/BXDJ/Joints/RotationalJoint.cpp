#include "RotationalJoint.h"

using namespace BXDJ;

RotationalJoint::RotationalJoint(const RigidNode & child, core::Ptr<fusion::RevoluteJointMotion> fusionJoint) : AngularJoint(child)
{
	this->fusionJoint = fusionJoint;
}

RotationalJoint::RotationalJoint(const RotationalJoint & jointToCopy) : AngularJoint(jointToCopy)
{
	fusionJoint = jointToCopy.fusionJoint;
}

Vector3<float> RotationalJoint::getAxisOfRotation()
{
	return Vector3<float>(0, 0, 0);
}

float RotationalJoint::getCurrentAngle()
{
	return 0.0f;
}

float BXDJ::RotationalJoint::getUpperLimit()
{
	return 0.0f;
}

float BXDJ::RotationalJoint::getLowerLimit()
{
	return 0.0f;
}
