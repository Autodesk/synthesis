#pragma once

#include <string>
#include "XmlWriter.h"
#include "CustomJSONObject.h"
#include "Components.h"

namespace BXDJ
{
	class Sensor : public XmlWritable
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
		
		Sensor(const Sensor &);
		Sensor(Type type = UNKNOWN);

		void write(XmlWriter &) const;

	private:
		static std::string toString(Type);
		static std::string toString(Signal);

	};
}
