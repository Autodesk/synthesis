#pragma once

#include "../../Vector3.h"

namespace BXDJ
{
	class Pneumatic : public XmlWritable
	{
	public:
		static const float COMMON_WIDTHS[3];
		static const float COMMON_PRESSURES[3];

		float widthMillimeter;
		float pressurePSI;

		Pneumatic(const Pneumatic &);
		Pneumatic(float = 5.0f, float = 40.0f);

		float getWidth() const;
		float getPressure() const;

		void write(XmlWriter &) const;

	};

	const float Pneumatic::COMMON_WIDTHS[] = { 2.5f, 5.0f, 10.0f };
	const float Pneumatic::COMMON_PRESSURES[] = { 10.0f, 20.0f, 60.0f };
}
