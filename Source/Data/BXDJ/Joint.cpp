#include "Joint.h"

using namespace BXDJ;

Joint::Joint(const Joint & jointToCopy)
{
	child = new RigidNode(*jointToCopy.child);
}

Joint::Joint(const RigidNode & child)
{
	this->child = new RigidNode(child);
}

Joint::~Joint()
{
	delete child;
}
