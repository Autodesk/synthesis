#pragma once

#include <string>
#include "XmlWriter.h"
#include "CustomJSONObject.h"
#include "Components.h"

namespace BXDJ
{
	/// An object that applies force to a Joint.
	class Driver : public XmlWritable, public CustomJSONObject
	{
	public:
		/// Type of Driver.
		enum Type : char
		{
			UNKNOWN = 0, ///< Unknown driver type.
			MOTOR = 1, ///< Standard rotational motor.
			SERVO = 2, ///< Attempts to move joint to specific angle.
			WORM_SCREW = 3, ///< Applies linear force to a joint.
			BUMPER_PNEUMATIC = 4, ///< Controlled by PCM.
			RELAY_PNEUMATIC = 5, ///< Controlled by Spikes.
			DUAL_MOTOR = 6, ///< Two motors acting on same joint.
			ELEVATOR = 7 ///< Used for linear motion with stages (typically vertical).
		};

		Type type; ///< The type of the Driver.

		/// Signal for communicating with a Driver.
		enum Signal : char
		{
			PWM = 1, ///< Pulse Width Modulation
			CAN = 2 ///< Controller Area Network
		};

		Signal portSignal; ///< The signal used for controlling the Driver.
		int portOne;
		int portTwo;
		float inputGear;
		float outputGear;
		
		/// Copy constructor.
		Driver(const Driver &);

		///
		/// Constructs a driver with the given type.
		/// \param type The type of Driver.
		///
		Driver(Type = UNKNOWN);

		// Component Functions

		void removeComponents(); ///< Removes all components from the Driver.
		void setComponent(Wheel); ///< Applies a Wheel to the Driver.
		void setComponent(Pneumatic); ///< Applies a Pneumatic to the Driver.

		std::unique_ptr<Wheel> getWheel(); ///< Gets any wheel configuration from the Driver. If the Driver has no Wheel, returns nullptr.
		std::unique_ptr<Pneumatic> getPneumatic(); ///< Gets any pneumatic configuration from the Driver. If the Driver has no Pneumatic, returns nullptr.

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

	private:
		std::unique_ptr<Wheel> wheel; ///< Wheel attached to the Driver.
		std::unique_ptr<Pneumatic> pneumatic; ///< Pneumatic attached to the Driver.

		static std::string toString(Type); ///< \return Name of the Driver Type.
		static std::string toString(Signal); ///< \return Name of the Signal.

		void write(XmlWriter &) const;

	};
}
