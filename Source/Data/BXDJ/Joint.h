#pragma once

#include <vector>
#include "RigidNode.h"

namespace BXDJ
{	
	// Links RigidNodes together
	class Joint : public XmlWritable
	{
	public:
		Joint(const Joint &);
		Joint(const RigidNode &);

		void write(XmlWriter &) const;

	private:
		std::shared_ptr<RigidNode> child;

	};
};
