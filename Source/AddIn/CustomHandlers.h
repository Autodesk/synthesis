#pragma once

#include <Core/CoreAll.h>
#include "Identifiers.h"
#include "../Exporter.h"

using namespace adsk::core;
using namespace adsk::fusion;

namespace Synthesis
{
	// Button Events
	class ShowPaletteCommandCreatedHandler : public adsk::core::CommandCreatedEventHandler
	{
	public:
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		std::vector<Ptr<Joint>> * joints;
		Ptr<Application> app;
	};

	class ShowPaletteCommandExecuteHandler : public adsk::core::CommandEventHandler
	{
	public:
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
		std::vector<Ptr<Joint>> * joints;
		Ptr<Application> app;
	};

	// Palette Events
	class ReceiveFormDataHandler : public adsk::core::HTMLEventHandler
	{
	public:
		void notify(const Ptr<HTMLEventArgs>& eventArgs) override;
		std::vector<Ptr<Joint>> * joints;
		Ptr<Application> app;
	};

	class CloseFormEventHandler : public adsk::core::UserInterfaceGeneralEventHandler
	{
	public:
		void notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) override;
		Ptr<Application> app;
	};
}
