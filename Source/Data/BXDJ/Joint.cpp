#include "RigidNode.h"

using namespace BXDJ;

Joint::Joint(const RigidNode & child)
{
	this->child = new RigidNode(child);
}

Joint::~Joint()
{
	delete child;
}
