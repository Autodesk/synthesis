#pragma once

#include <Fusion/Components/CylindricalJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	/// Allows the child RigidNode to slide and rotate along an axis.
	class CylindricalJoint : public Joint
	{
	public:
		CylindricalJoint(const CylindricalJoint &);

		///
		/// Creates a joint between an existing RigidNode and one that will be created.
		/// \param parent RigidNode that will serve as the parent of the joint.
		/// \param fusionJoint The Fusion joint to base the new Joint off of.
		/// \param parentOccurrence The Fusion occurrence in the Fusion joint that is owned by the parent RigidNode.
		///                         The other occurrence in the Fusion joint will become the primary occurrence for the new child RigidNode.
		///
		CylindricalJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		///
		/// Creates a joint between an existing RigidNode and one that will be created.
		/// \param parent RigidNode that will serve as the parent of the joint.
		/// \param fusionJoint The Fusion as-built joint to base the new Joint off of.
		/// \param parentOccurrence The Fusion occurrence in the Fusion joint that is owned by the parent RigidNode.
		///                         The other occurrence in the Fusion joint will become the primary occurrence for the new child RigidNode.
		///
		CylindricalJoint(RigidNode *, core::Ptr<fusion::AsBuiltJoint>, core::Ptr<fusion::Occurrence>);

		Vector3<> getAxis() const; ///< \return The axis along which the child may move and rotate.

		float getCurrentAngle() const; ///< \return The current angle of the child.
		bool hasLimits() const; ///< \return Whether or not rotation is limited.
		float getMinAngle() const; ///< \return The minimum angle the child may rotate to.
		float getMaxAngle() const; ///< \return The maximum angle the child may rotate to.

		float getCurrentTranslation() const; ///< \return The current position of the child along the axis.
		float getMinTranslation() const; ///< \return The minimum position of the child.
		float getMaxTranslation() const; ///< \return The maximum position of the child.

		void applyConfig(const ConfigData &);

	private:
		core::Ptr<fusion::CylindricalJointMotion> fusionJointMotion; ///< The cylindrical joint in Fusion.

		void write(XmlWriter &) const;

	};
}
