#pragma once

#include <rapidjson/document.h>

namespace BXDJ
{
	class CustomJSONObject
	{
	public:
		virtual rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const = 0;
		virtual void loadJSONObject(const rapidjson::Value&) = 0;

		std::string toJSONString() const;
		void fromJSONString(std::string);

	};
}
