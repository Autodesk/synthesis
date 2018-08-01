#pragma once

#include "../../Vector3.h"

namespace BXDJ
{
	class LinearJoint
	{
	public:
		virtual Vector3<> getAxisOfTranslation() const = 0;
		virtual float getCurrentTranslation() const = 0;
		virtual float getMinTranslation() const = 0;
		virtual float getMaxTranslation() const = 0;
	};
}
