#pragma once

#include <Fusion/Fusion/FusionDocument.h>
#include <functional>
#include "Data/BXDJ/ConfigData.h"

using namespace adsk::core;
using namespace adsk::fusion;

namespace SynthesisAddIn
{
	/// Manages robot configurations and exporting robots.
	class Exporter
	{
	public:
		///
		/// Collects all configurable Fusion joints.
		/// \param document Fusion document to get joints from.
		///
		static std::vector<Ptr<Joint>> collectJoints(Ptr<FusionDocument>);
		///
		/// Collects all configurable Fusion as-built joints.
		/// \param document Fusion document to get joints from.
		///
		static std::vector<Ptr<AsBuiltJoint>> collectAsBuiltJoints(Ptr<FusionDocument>);

		///
		/// Loads ConfigData from a Fusion document's attributes.
		/// \param document Fusion document to load ConfigData from.
		///
		static BXDJ::ConfigData loadConfiguration(Ptr<FusionDocument>);

		///
		/// Saves ConfigData to a Fusion document's attributes.
		/// \param config ConfigData to save.
		/// \param document Fusion document to save ConfigData to.
		///
		static void saveConfiguration(BXDJ::ConfigData, Ptr<FusionDocument>);

		static void exportExample(); ///< Exports a basic BXDA file for binary analysis.
		static void exportExampleXml(); ///< Exports a basic BXDJ file for XML analysis.

		///
		/// Exports a robot.
		/// \param config ConfigData to use when building the robot's node tree.
		/// \param document Fusion document to export.
		/// \param progressCallback Function to call when updating the total export progress.
		/// \param cancel If this value ever becomes true, exporting will be stopped as soon as possible.
		///
		static void exportMeshes(BXDJ::ConfigData, Ptr<FusionDocument>, std::function<void(double)> = nullptr, const bool * = nullptr);

	};
}
