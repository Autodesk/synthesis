#pragma once

#include <Fusion/Components/Occurrence.h>
#include <Fusion/Components/Joint.h>

namespace BXDJ
{
	namespace Utility
	{
		// Gets the level of a component occurence in the heirarchy
		int levelOfOccurrence(adsk::core::Ptr<adsk::fusion::Occurrence>);
		
		// Gets the upper/lower occurence in a joint (for deterimining parent/child relationships
		adsk::core::Ptr<adsk::fusion::Occurrence> lowerOccurrence(adsk::core::Ptr<adsk::fusion::Joint>);
		adsk::core::Ptr<adsk::fusion::Occurrence> upperOccurrence(adsk::core::Ptr<adsk::fusion::Joint>);
	}
}
