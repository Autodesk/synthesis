#include "RigidNode.h"

using namespace BXDJ;

RigidNode::RigidNode()
{
	mesh = NULL;
}

RigidNode::~RigidNode()
{
	for (Joint * j : childrenJoints)
		delete j;

	if (mesh != NULL)
		delete mesh;
}

RigidNode::RigidNode(const BXDA::Mesh & mesh)
{
	this->mesh = new BXDA::Mesh(mesh);
}

void RigidNode::AddMesh(const BXDA::Mesh & mesh)
{
	if (this->mesh != NULL)
		delete this->mesh;

	this->mesh = new BXDA::Mesh(mesh);
}
