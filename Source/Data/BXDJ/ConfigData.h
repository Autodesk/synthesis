#pragma once

#include <map>
#include <Fusion/Components/Joint.h>

namespace BXDJ
{
	struct ConfigData
	{
		ConfigData() {}

		ConfigData(const ConfigData & other)
		{
			for (auto i = other.joints.begin(); i != other.joints.end(); i++)
				joints[i->first] = std::make_unique<int>(*i->second);
		}

		std::map<adsk::core::Ptr<adsk::fusion::Joint>, std::unique_ptr<int>> joints;
	};
}
