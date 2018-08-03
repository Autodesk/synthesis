#pragma once

#include <string>
#include "XmlWriter.h"
#include "CustomJSONObject.h"
#include "Components.h"

namespace BXDJ
{
	class Driver : public XmlWritable, public CustomJSONObject
	{
	public:
		enum Type : char
		{
			UNKNOWN = 0,
			MOTOR = 1,
			SERVO = 2,
			WORM_SCREW = 3,
			BUMPER_PNEUMATIC = 4,
			RELAY_PNEUMATIC = 5,
			DUAL_MOTOR = 6,
			ELEVATOR = 7
		};

		Type type;

		enum Signal : char
		{
			PWM = 1,
			CAN = 2
		};

		Signal portSignal;
		int portA;
		int portB;
		float inputGear;
		float outputGear;
		
		Driver(const Driver &);
		Driver(Type type = UNKNOWN);

		// Component Functions
		void removeComponents();
		void setComponent(Wheel);
		void setComponent(Pneumatic);
		std::unique_ptr<Wheel> getWheel();
		std::unique_ptr<Pneumatic> getPneumatic();

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

	private:
		std::unique_ptr<Wheel> wheel;
		std::unique_ptr<Pneumatic> pneumatic;

		static std::string toString(Type);
		static std::string toString(Signal);

		void write(XmlWriter &) const;

	};
}
