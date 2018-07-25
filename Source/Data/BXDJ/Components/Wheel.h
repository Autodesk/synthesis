#pragma once

#include "../RigidNode.h"
#include "../../Vector3.h"

using namespace adsk;

namespace BXDJ
{
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
		Wheel(std::shared_ptr<RigidNode>, Type = NORMAL);

		float getRadius() const;
		float getWidth() const;
		Vector3<float> getCenter() const;

		void write(XmlWriter &) const;

	private:
		std::shared_ptr<RigidNode> node;

		static std::string toString(Type type);

	};
}
