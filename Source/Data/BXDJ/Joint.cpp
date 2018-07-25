#include "Joint.h"

using namespace BXDJ;

Joint::Joint(const Joint & jointToCopy)
{
	child = jointToCopy.child;
}

Joint::Joint(const RigidNode & child, RigidNode * parent)
{
	this->child = std::make_shared<RigidNode>(child);
	this->child->connectToJoint(this);
	this->parent = parent;
}

RigidNode * BXDJ::Joint::getParent()
{
	return parent;
}

void Joint::write(XmlWriter & output) const
{
	output.write(*child);
}
