#pragma once

#include <Fusion/Components/JointLimits.h>
#include <vector>
#include <limits>
#include "RigidNode.h"

namespace BXDJ
{	
	// Links RigidNodes together
	class Joint : public XmlWritable
	{
	public:
		Joint(const Joint &);
		Joint(const RigidNode &, RigidNode *);

		RigidNode * getParent();
		std::shared_ptr<RigidNode> getChild();
		virtual Vector3<float> getBasePoint() const = 0;

		virtual void write(XmlWriter &) const;

	private:
		std::shared_ptr<RigidNode> child;
		RigidNode * parent;

	};
};
