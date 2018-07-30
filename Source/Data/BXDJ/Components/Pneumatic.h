#pragma once

#include "../../Vector3.h"

namespace BXDJ
{
	class Pneumatic : public XmlWritable
	{
	public:
		float widthMillimeter;
		float pressurePSI;

		Pneumatic(const Pneumatic &);
		Pneumatic(float = 5.0f, float = 40.0f);

		float getWidth() const;
		float getPressure() const;

		void write(XmlWriter &) const;

	};
}
