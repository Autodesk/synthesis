#pragma once

#include <string>
#include "XmlWriter.h"
#include "CustomJSONObject.h"
#include "Components.h"
#include <nlohmann/json.hpp>

namespace BXDJ
{
	/// A sensor which measures information relative to a joint. This can include encoders and limit switches. Accelerometers and gyros are relative to a RigidNode.
	class JointSensor : public XmlWritable, public CustomJSONObject
	{
	public:
		/// A type of JointSensor.
		enum Type : char
		{
			UNKNOWN = 0, ///< Unknown sensor type.
			ENCODER = 1 ///< Measures the rotation of a rotational joint.
		};

		Type type; ///< The type of the sensor.

		/// A type of signal that a sensor accepts.
		enum Signal : char
		{
			DIO = 1, ///< Digital Input/Output ports.
			ANALOG = 2 ///< Analog ports.
		};

		Signal portSignal; ///< The signal that the sensor accepts.
		int portA; ///< Port A of the sensor.
		int portB; ///< Port B of the sensor.
		double conversionFactor; ///< The multiplier applied to the raw value of the sensor (i.e. ticks per revolution).
		
		/// Copy constructor.
		JointSensor(const JointSensor &);

		///
		/// Constructs a JointSensor of the given type.
		/// \param type The type of the sensor.
		///
		JointSensor(Type = UNKNOWN);

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);
		nlohmann::json GetExportJSON();

	private:
		static std::string toString(Type); ///< \return The name of the sensor type.
		static std::string toString(Signal); ///< \return The name of the signal type.

		void write(XmlWriter &) const;

	};
}
