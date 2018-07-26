#pragma once

#include "../../Vector3.h"

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
		Vector3<> getCenter() const;

		void write(XmlWriter &) const;

	private:
		double radius;
		double width;
		Vector3<> center;

		static std::string toString(Type type);

	};
}
