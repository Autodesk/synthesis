#pragma once

#include <Core/Application/Application.h>
#include <Core/UserInterface/UserInterface.h>
#include "Data/BXDA/Mesh.h"
#include "Data/BXDJ/ConfigData.h"
#include "Data/BXDJ/RigidNode.h"
#include "Data/BXDA/BinaryWriter.h"
#include "Data/BXDJ/XmlWriter.h"

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
