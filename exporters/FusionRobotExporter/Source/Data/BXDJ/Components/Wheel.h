#pragma once

#include "../XmlWriter.h"
#include "../CustomJSONObject.h"
#include "../../Vector3.h"

namespace BXDJ
{
	class RotationalJoint;

	/// Stores wheel information about a Driver.
	class Wheel : public XmlWritable, public CustomJSONObject
	{
	public:
		///
		/// Various types of wheels.
		///
		enum Type
		{
			NORMAL = 1, ///< Typical rubber wheel.
			OMNI = 2, ///< Omni wheel.
			MECANUM = 3 ///< Mecanum wheel.
		};
		Type type; ///< Type of the wheel.

		///
		/// Predetermined degrees of wheel friction.
		///
		enum FrictionLevel
		{
			LOW = 1, ///< Low friction.
			MEDIUM = 2, ///< Medium friction.
			HIGH = 3 ///< High friction.
		};
		FrictionLevel frictionLevel; ///< The friction level of the wheel.

		bool isDriveWheel; ///< True if the wheel is used in the robot's drivetrain.

		/// Copy constructor.
		Wheel(const Wheel &);

		///
		/// Creates a wheel.
		/// \param type The type of wheel.
		/// \param frictionLevel The friction level of the wheel.
		/// \param isDriveWheel Whether or not the wheel is used in the drivetrain.
		///
		Wheel(Type = NORMAL, FrictionLevel = MEDIUM, bool = false);

		///
		/// Copies an existing wheel configuration while recalculating wheel radius and width based on the child of a joint.
		/// \param wheel Wheel to copy.
		/// \param joint Joint to calculate wheel dimensions from.
		///
		Wheel(const Wheel &, const RotationalJoint &);

		double getRadius() const; ///< \return The radius of the wheel. Calculated based on the mesh of the wheel RigidNode.
		double getWidth() const; ///< \return The width of the wheel. Calculated based on the mesh of the wheel RigidNode.
		Vector3<> getCenter() const; ///< \return The center point of the wheel. Calculated based on the mesh of the wheel RigidNode.

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
		double radius; ///< Radius of the wheel.
		double width; ///< Width of the wheel.
		Vector3<> center; ///< Center point of the wheel in Fusion space.

		void write(XmlWriter &) const;

		/// \param type A type of wheel.
		/// \return The name of the type of wheel.
		static std::string toString(Type type);

	};
}
