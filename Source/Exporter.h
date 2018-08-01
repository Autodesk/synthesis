#pragma once

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>
#include "Data/BXDJ/ConfigData.h"

using namespace adsk::core;
using namespace adsk::fusion;

namespace Synthesis
{
	class Exporter
	{
	public:
		Exporter(Ptr<Application>);
		~Exporter();

		std::vector<Ptr<Joint>> collectJoints();
		std::string stringifyJoints(std::vector<Ptr<Joint>>);

		void exportExample();
		void exportExampleXml();
		void exportMeshes(BXDJ::ConfigData config);

	private:
		Ptr<Application> fusionApplication;

	};
}
