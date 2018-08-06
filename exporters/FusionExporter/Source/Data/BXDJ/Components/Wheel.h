#pragma once

#include "../XmlWriter.h"
#include "../CustomJSONObject.h"
#include "../../Vector3.h"

namespace BXDJ
{
	class RotationalJoint;

	class Wheel : public XmlWritable, public CustomJSONObject
	{
	public:
		enum Type
		{
			NORMAL = 1,
			OMNI = 2,
			MECANUM = 3
		};
		Type type;

		enum FrictionLevel
		{
			LOW = 1,
			MEDIUM = 2,
			HIGH = 3
		};
		FrictionLevel frictionLevel;

		bool isDriveWheel;

		Wheel(const Wheel &);
		Wheel(Type = NORMAL, FrictionLevel = MEDIUM, bool isDriveWheel = false);
		Wheel(const Wheel &, const RotationalJoint &);

		double getRadius() const;
		double getWidth() const;
		Vector3<> getCenter() const;

		float getForwardAsympSlip() const;
		float getForwardAsympValue() const;
		float getForwardExtremeSlip() const;
		float getForwardExtremeValue() const;
		float getSideAsympSlip() const;
		float getSideAsympValue() const;
		float getSideExtremeSlip() const;
		float getSideExtremeValue() const;

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

	private:
		double radius;
		double width;
		Vector3<> center;

		void write(XmlWriter &) const;
		static std::string toString(Type type);

	};
}
