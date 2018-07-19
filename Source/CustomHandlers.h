#pragma once

#include "Exporter.h"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

namespace Synthesis
{
	//____________________Button Events_________________________

	class ExportCommandCreatedEventHandler : public adsk::core::CommandCreatedEventHandler
	{
	public:
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	private:

	};

	class ExportWheelCommandCreatedEventHandler : public adsk::core::CommandCreatedEventHandler
	{
	public:
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	private:

	};

	//________________Palette Events__________________________

	class MyCloseEventHandler : public adsk::core::UserInterfaceGeneralEventHandler
	{
	public:
		void notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	};

	class MyHTMLEventHandler : public adsk::core::HTMLEventHandler
	{
	public:
		void notify(const Ptr<HTMLEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	};

	class ShowPaletteCommandExecuteHandler : public adsk::core::CommandEventHandler
	{
	public:
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	};

	class ShowPaletteCommandCreatedHandler : public adsk::core::CommandCreatedEventHandler
	{
	public:
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	};

	// Event handler for the commandExecuted event to send info to the palette.
	class SendInfoCommandExecuteHandler : public adsk::core::CommandEventHandler
	{
	public:
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
		Ptr<Application> _APP;
	};
}