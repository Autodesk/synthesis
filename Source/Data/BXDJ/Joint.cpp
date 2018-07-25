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

RigidNode * BXDJ::Joint::getParent()
{
	return parent;
}

std::shared_ptr<RigidNode> BXDJ::Joint::getChild()
{
	return child;
}

Vector3<float> BXDJ::Joint::getParentBasePoint() const
{
	core::Ptr<fusion::JointGeometry> geometry = (parentIsOccOne ? fusionJoint->geometryOrOriginOne() : fusionJoint->geometryOrOriginTwo());
	return Vector3<float>(geometry->origin()->x, geometry->origin()->y, geometry->origin()->z);
}

void Joint::write(XmlWriter & output) const
{
	// Write driver information
}
