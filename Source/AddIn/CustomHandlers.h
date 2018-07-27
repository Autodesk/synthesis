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
		ShowPaletteCommandCreatedHandler(Ptr<Application> app) : app(app) {}
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
	private:
		Ptr<Application> app;
	};

	class ShowPaletteCommandExecuteHandler : public adsk::core::CommandEventHandler
	{
	public:
		ShowPaletteCommandExecuteHandler(Ptr<Application> app) : app(app) {}
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
	private:
		Ptr<Application> app;
	};

	// Palette Events
	class ReceiveFormDataHandler : public adsk::core::HTMLEventHandler
	{
	public:
		ReceiveFormDataHandler(Ptr<Application> app) : app(app) {}
		void notify(const Ptr<HTMLEventArgs>& eventArgs) override;
	private:
		const char ASCII_OFFSET = -32;
		Ptr<Application> app;
	};

	class CloseFormEventHandler : public adsk::core::UserInterfaceGeneralEventHandler
	{
	public:
		CloseFormEventHandler(Ptr<Application> app) : app(app) {}
		void notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) override;
	private:
		Ptr<Application> app;
	};
}
