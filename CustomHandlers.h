#pragma once

#include "Exporter.h"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

namespace Synthesis {
	class ExportCommandCreatedEventHandler : public adsk::core::CommandCreatedEventHandler {
	public:
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	private:

	};

	class ExportWheelCommandCreatedEventHandler : public adsk::core::CommandCreatedEventHandler {
	public:
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	private:

	};
}