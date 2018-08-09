#pragma once

#include <Core/Memory.h>

namespace BXDJ
{
	namespace Utility
	{
		// Gets the level of a component occurrence in the heirarchy
		int levelOfOccurrence(adsk::core::Ptr<adsk::fusion::Occurrence>);
		
		// Gets the upper/lower occurrence in a joint (for deterimining parent/child relationships
		adsk::core::Ptr<adsk::fusion::Occurrence> lowerOccurrence(adsk::core::Ptr<adsk::fusion::Joint>);
		adsk::core::Ptr<adsk::fusion::Occurrence> upperOccurrence(adsk::core::Ptr<adsk::fusion::Joint>);

		// Gets a string that is unique to a specific joint
		std::string getUniqueJointID(adsk::core::Ptr<adsk::fusion::Joint>);
	}
}
