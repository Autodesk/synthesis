#pragma once

#include <string>
#include "XmlWriter.h"
#include "CustomJSONObject.h"
#include "Components.h"

namespace BXDJ
{
	class JointSensor : public XmlWritable, public CustomJSONObject
	{
	public:
		enum Type : char
		{
			UNKNOWN = 0,
			ENCODER = 1
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
		double conversionFactor;
		
		JointSensor(const JointSensor &);
		JointSensor(Type type = UNKNOWN);

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

	private:
		static std::string toString(Type);
		static std::string toString(Signal);

		void write(XmlWriter &) const;

	};
}
