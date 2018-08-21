#pragma once

#include <Fusion/Components/BallJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	/// Allows the child RigidNode to rotate freely about a fixed point relative to the parent RigidNode.
	class BallJoint : public Joint
	{
	public:
		/// Copy constructor.
		BallJoint(const BallJoint &);

		///
		/// Creates a joint between an existing RigidNode and one that will be created.
		/// \param parent RigidNode that will serve as the parent of the joint.
		/// \param fusionJoint The Fusion joint to base the new Joint off of.
		/// \param parentOccurrence The Fusion occurrence in the Fusion joint that is owned by the parent RigidNode.
		///                         The other occurrence in the Fusion joint will become the primary occurrence for the new child RigidNode.
		///
		BallJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		///
		/// Creates a joint between an existing RigidNode and one that will be created.
		/// \param parent RigidNode that will serve as the parent of the joint.
		/// \param fusionJoint The Fusion as-built joint to base the new Joint off of.
		/// \param parentOccurrence The Fusion occurrence in the Fusion joint that is owned by the parent RigidNode.
		///                         The other occurrence in the Fusion joint will become the primary occurrence for the new child RigidNode.
		///
		BallJoint(RigidNode *, core::Ptr<fusion::AsBuiltJoint>, core::Ptr<fusion::Occurrence>);

		void applyConfig(const ConfigData &);

	private:
		core::Ptr<fusion::BallJointMotion> fusionJointMotion; ///< The ball joint in Fusion.

		void write(XmlWriter &) const;

	};
}
