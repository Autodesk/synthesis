#pragma once

#include <vector>
#include "RigidNode.h"

namespace BXDJ
{	
	// Links RigidNodes together
	class Joint
	{
	public:
		Joint(const Joint &);
		Joint(const RigidNode &);
		~Joint();

	private:
		RigidNode * child;

	};
};
