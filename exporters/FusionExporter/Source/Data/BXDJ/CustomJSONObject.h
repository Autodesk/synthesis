#pragma once

#include <rapidjson/document.h>

namespace BXDJ
{
	/// An object that can be written to and read from JSON.
	class CustomJSONObject
	{
	public:
		///
		/// Generate a JSON string from the instance.
		///
		std::string toJSONString() const;

		///
		/// Replace the contents of the instance with the values stored in a JSON string.
		///
		void fromJSONString(std::string);

		///
		/// Generate a JSON object from the instance.
		///
		virtual rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const = 0;

		///
		/// Replace the contents of the instance with the values stored in a JSON object.
		///
		virtual void loadJSONObject(const rapidjson::Value&) = 0;

	};
}
