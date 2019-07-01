#pragma once

#include <vector>
#include <map>
#include <functional>
#include <Fusion/Components/Component.h>
#include <Fusion/Components/Occurrence.h>
#include "XmlWriter.h"
#include "../Guid.h"

using namespace adsk;

namespace BXDA
{
	class Mesh;
}

namespace BXDJ
{
	class ConfigData;
	class Joint;

	/// Stores a collection of component occurrences that act as a single RigidBody in the Synthesis engine.
	class RigidNode : public XmlWritable
	{
	public:
		/// Copy constructor.
		RigidNode(const RigidNode &);
		
		///
		/// Creates a RigidNode that acts as the root of a node tree.
		/// \param rootComponent Component to base the tree off of. The bodies of the component itself will not be a part of the output, but all occurrences it contains will be.
		/// \param config Configuration data to use when creating joints, drivers, and components.
		///
		RigidNode(core::Ptr<fusion::Component>, ConfigData);

		///
		/// Creates a RigidNode as a child of another RigidNode via a Joint.
		/// This should only ever be called by the constructor for Joint.
		/// \param occ Primary Fusion occurrence of the new RigidNode.
		/// \param parent Joint to connect the RigidNode to.
		///
		RigidNode(core::Ptr<fusion::Occurrence>, Joint *);

		
		Guid getGUID() const; ///< \return The Guid of the RigidNode.
		std::string getModelId() const; ///< \return The fullPathName of the primary occurrence of the RigidNode.
		Joint * getParent() const; ///< \return The Joint connecting the RigidNode to its parent.
		int getOccurrenceCount() const; ///< \return The number of Fusion occurrences contained in this RigidNode.
		
		
		///
		/// Creates a vector containing the children of this RigidNode.
		/// \param[out] children Vector to place all child RigidNodes into.
		/// \param recursive Whether to also collect the children of this node's children.
		///
		void getChildren(std::vector<std::shared_ptr<RigidNode>> &, bool = false) const;

		///
		/// Generates a Mesh for this RigidNode.
		/// \param[out] mesh Mesh generated from this RigidNode's Fusion occurrences.
		/// \param ignorePhysics True to skip physics calculations. Doing this will save processing time.
		/// \param progressCallback Function that will be passed the progress of the mesh generation.
		/// \param cancel If this value is set to true, generation will be stopped as soon as possible.
		/// 
		void getMesh(BXDA::Mesh &, bool = false, std::function<void(double)> = nullptr, const bool * = nullptr) const;

#if _DEBUG
		std::string getLog() const; ///< \return The log generated while building the tree.
#endif

	private:
		///
		/// Used for storing information about which occurrences are parents or children in joints.
		/// 
		struct JointSummary
		{
			std::map<core::Ptr<fusion::Occurrence>, core::Ptr<fusion::Occurrence>> children; ///< Map of all Fusion occurrences that are children of joints/rigidgroups to their parents.
			std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Joint>>> parents; ///< Map of all Fusion occurrences that are parents of joints to their children.
			std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::AsBuiltJoint>>> asBuiltParents; ///< Map of all Fusion occurrences that are parents of as-built joints to their children.
			std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Occurrence>>> rigidgroups; ///< Map of all Fusion occurrences that are parents of rigidgroups to their children.

			std::string toString() const; ///< \return A stringified version of the JointSummary.
		};

		Guid guid; ///< Globally Unique IDentifier: Used to identify this node in the BXDJ file.
		std::shared_ptr<ConfigData> configData; ///< Provides the drivers and components that will be associated with joints after the tree is built.
		std::shared_ptr<JointSummary> jointSummary; ///< Summarizes the joints and rigidgroups of a Fusion document by listing child and parent occurrences.
		std::vector<std::shared_ptr<Joint>> childrenJoints; ///< The joints connected to this node in which this node is recognized as the parent.
		Joint * parent; ///< The joint that attaches this node to its parent node.
		std::vector<core::Ptr<fusion::Occurrence>> fusionOccurrences; ///< Stores all Fusion occurrences that are grouped into this node.

#if _DEBUG
		static std::string log; ///< Contains a log of the tree building process. (Debug builds only)
		static int depth; ///< The current depth within the tree that is being logged. (Debug builds only)
#endif

		///
		/// Creates a JointSummary for building the RigidNode tree.
		/// \param rootComponent Root component in the Fusion document hierarchy. Used to retrieve all joints and rigidgroups.
		/// \return The generated JointSummary.
		///
		JointSummary getJointSummary(core::Ptr<fusion::Component>);

		///
		/// Adds a Fusion occurrence to the RigidNode. Traverses down the occurrence's suboccurrences.
		/// Any occurrences that are children of joints or rigidgroups will be ignored (uses jointSummary).
		/// Creates a Joint for every child connected to the occurrence given.
		/// \param rootOccurrence Occurrence to build tree from.
		///
		void buildTree(core::Ptr<fusion::Occurrence>);

		///
		/// Creates a Joint based on a Fusion joint and attaches it to this RigidNode.
		/// Unsupported joint types will be treated the same as rigid joints, that is,
		/// they will be merged into this RigidNode.
		/// \param joint Fusion joint to base the Joint off of.
		/// \param parent The occurrence that should be recognized as the parent of the Joint.
		///
		void addJoint(core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		///
		/// Creates a Joint based on a Fusion as-built joint and attaches it to this RigidNode.
		/// Unsupported joint types will be treated the same as rigid joints, that is,
		/// they will be merged into this RigidNode.
		/// \param joint Fusion as-built joint to base the Joint off of.
		/// \param parent The occurrence that should be recognized as the parent of the Joint.
		///
		void addJoint(core::Ptr<fusion::AsBuiltJoint>, core::Ptr<fusion::Occurrence>);
		
		void write(XmlWriter &) const;

	};
};
