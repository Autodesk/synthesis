#pragma once

#include <Fusion/Components/SliderJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	/// Allows the child RigidNode to slide along an axis.
	class SliderJoint : public Joint
	{
	public:
		SliderJoint(const SliderJoint &);

		///
		/// Creates a joint between an existing RigidNode and one that will be created.
		/// \param parent RigidNode that will serve as the parent of the joint.
		/// \param fusionJoint The Fusion joint to base the new Joint off of.
		/// \param parentOccurrence The Fusion occurrence in the Fusion joint that is owned by the parent RigidNode.
		///                         The other occurrence in the Fusion joint will become the primary occurrence for the new child RigidNode.
		///
		SliderJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		///
		/// Creates a joint between an existing RigidNode and one that will be created.
		/// \param parent RigidNode that will serve as the parent of the joint.
		/// \param fusionJoint The Fusion as-built joint to base the new Joint off of.
		/// \param parentOccurrence The Fusion occurrence in the Fusion joint that is owned by the parent RigidNode.
		///                         The other occurrence in the Fusion joint will become the primary occurrence for the new child RigidNode.
		///
		SliderJoint(RigidNode *, core::Ptr<fusion::AsBuiltJoint>, core::Ptr<fusion::Occurrence>);

		Vector3<> getAxisOfTranslation() const; ///< \return The axis along which the child may move.
		float getCurrentTranslation() const; ///< \return The current position of the child along the axis.
		float getMinTranslation() const; ///< \return The minimum position of the child.
		float getMaxTranslation() const; ///< \return The maximum position of the child.

		void applyConfig(const ConfigData &);
		nlohmann::json GetJson();

	private:
		core::Ptr<fusion::SliderJointMotion> fusionJointMotion; ///< The slider joint in Fusion.

		void write(XmlWriter &) const;

	};
}
