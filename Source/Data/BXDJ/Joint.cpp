#include "Joint.h"

using namespace BXDJ;

Joint::Joint(const Joint & jointToCopy)
{
	child = jointToCopy.child;
}

Joint::Joint(const RigidNode & child)
{
	this->child = std::make_shared<RigidNode>(child);
}
