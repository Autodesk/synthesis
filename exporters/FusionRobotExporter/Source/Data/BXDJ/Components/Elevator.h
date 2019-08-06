#pragma once

#include "../XmlWriter.h"
#include "../CustomJSONObject.h"

namespace BXDJ
{
	/// Stores information about an elevator. This is currently the only functional linear joint in Synthesis.
	class Elevator : public XmlWritable, public CustomJSONObject
	{
	public:
		///
		/// Various types of elevators.
		///
		enum Type : int
		{
			SINGLE = 0, ///< Single stage elevator. This is most FRC elevators.
			CASCADING_STAGE_1 = 1, ///< First stage of a cascading elevator.
			CASCADING_STAGE_2 = 2, ///< Second stage of a cascading elevator.
			CONTINUOUS_STAGE_1 = 3, ///< First stage of a continuous elevator.
			CONTINUOUS_STAGE_2 = 4 ///< Second stage of a continuous elevator.
		};
		Type type; ///< The type of elevator system being described.

		/// Copy constructor.
		Elevator(const Elevator &);

		///
		/// Constructs a elevator configuration of the given type.
		/// \param type The type of elevator system being described.
		///
		Elevator(Type = SINGLE);

		nlohmann::json getJSONObject() const;
		void loadJSONObject(nlohmann::json);

	private:
		void write(XmlWriter &) const;

		/// \param type A type of elevator.
		/// \return The name of the type of elevator.
		static std::string toString(Type type);

	};
}
