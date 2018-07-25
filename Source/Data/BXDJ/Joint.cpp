#include "Joint.h"

using namespace BXDJ;

Joint::Joint(const Joint & jointToCopy)
{
	parentIsOccOne = jointToCopy.parentIsOccOne;
	fusionJoint = jointToCopy.fusionJoint;
	parent = jointToCopy.parent;
	child = jointToCopy.child;
}

Joint::Joint(RigidNode * parent, core::Ptr<fusion::Joint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence)
{
	parentIsOccOne = (fusionJoint->occurrenceOne() == parentOccurrence);

	this->fusionJoint = fusionJoint;

	if (parent == NULL)
		throw "Parent node cannot be NULL!";

	this->parent = parent;
	this->child = std::make_shared<RigidNode>((parentIsOccOne ? fusionJoint->occurrenceTwo() : fusionJoint->occurrenceOne()), this);
}

RigidNode * Joint::getParent()
{
	return parent;
}

std::shared_ptr<RigidNode> Joint::getChild()
{
	return child;
}

Vector3<float> Joint::getParentBasePoint() const
{
	core::Ptr<fusion::JointGeometry> geometry = (parentIsOccOne ? fusionJoint->geometryOrOriginOne() : fusionJoint->geometryOrOriginTwo());
	return Vector3<float>((float)geometry->origin()->x(),
						  (float)geometry->origin()->y(),
						  (float)geometry->origin()->z());
}

Vector3<float> Joint::getChildBasePoint() const
{
	core::Ptr<fusion::JointGeometry> geometry = (parentIsOccOne ? fusionJoint->geometryOrOriginTwo() : fusionJoint->geometryOrOriginOne());
	return Vector3<float>((float)geometry->origin()->x(),
						  (float)geometry->origin()->y(),
						  (float)geometry->origin()->z());
}

void Joint::write(XmlWriter & output) const
{
	// Write driver information
}
