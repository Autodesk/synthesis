#pragma once

#include "../RigidNode.h"
#include "../../Vector3.h"

using namespace adsk;

namespace BXDJ
{
	class RotationalJoint;

	class Wheel : public XmlWritable
	{
	public:
		enum Type
		{
			NORMAL = 1,
			OMNI = 2,
			MECANUM = 3
		};
		Type type;

		Wheel(const Wheel &);
		Wheel(const RotationalJoint &, Type = NORMAL);

		double getRadius() const;
		double getWidth() const;
		Vector3<float> getCenter() const;

		void write(XmlWriter &) const;

	private:
		double radius;
		double width;

		static std::string toString(Type type);

	};
}
