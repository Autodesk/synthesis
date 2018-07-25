#pragma once

#include "../Component.h"
#include "../../Vector3.h"

namespace BXDJ
{
	class Wheel : public Component
	{
	public:
		enum Type
		{
			NORMAL = 1,
			OMNI = 2,
			MECANUM = 3
		};
		Type type;

		Wheel(Driver *);

		float getRadius() const;
		float getWidth() const;
		Vector3<float> getCenter() const;

		void write(XmlWriter &) const;

	private:
		static std::string toString(Type type);

	};
}
