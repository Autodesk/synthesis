#pragma once

#include "../XmlWriter.h"
#include "../CustomJSONObject.h"
#include "../../Vector3.h"

namespace BXDJ
{
	class Pneumatic : public XmlWritable, public CustomJSONObject
	{
	public:
		const static float COMMON_WIDTHS[];
		const static float COMMON_PRESSURES[];

		float widthMillimeter;
		float pressurePSI;

		Pneumatic(const Pneumatic &);
		Pneumatic(float = 5.0f, float = 40.0f);

		float getWidth() const;
		float getPressure() const;
		int getCommonWidth() const;
		int getCommonPressure() const;

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

	private:
		void write(XmlWriter &) const;

	};
}
