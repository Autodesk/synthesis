#pragma once

#include "../XmlWriter.h"
#include "../CustomJSONObject.h"
#include "../../Vector3.h"

namespace BXDJ
{
	/// Stores pneumatic information about a Driver.
	class Pneumatic : public XmlWritable, public CustomJSONObject
	{
	public:
		const static float COMMON_WIDTHS[]; ///< Commonly used pneumatic widths.
		const static float COMMON_PRESSURES[]; ///< Commonly used pressures.

		float widthMillimeter; ///< Width of the pneumatic cylinder.
		float pressurePSI; ///< Pressure of the pneumatic system.

		/// Copy constructor.
		Pneumatic(const Pneumatic &);

		///
		/// Constructs a pneumatic configuration with the given width and pressure.
		/// \param widthMillimeter The width of the pneumatic cylinder (mm).
		/// \param pressurePSI The pressure of the pneumatic system (PSI).
		///
		Pneumatic(float = 5.0f, float = 40.0f);

		float getWidth() const; ///< \return The width of the pneumatic cylinder.
		float getPressure() const; ///< \return The pressure of the pneumatic system.
		int getCommonWidth() const; ///< \return The index of the nearest common width in COMMON_WIDTHS.
		int getCommonPressure() const; ///< \return The index of the nearest common pressure in COMMON_PRESSURES.

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

	private:
		void write(XmlWriter &) const;

	};
}
