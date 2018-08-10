#pragma once

#include <Core/CoreAll.h>
#include <thread>

using namespace adsk::core;
using namespace adsk::fusion;

namespace Synthesis
{
	class EUI;

	// Workspace Events
	class WorkspaceActivatedHandler : public WorkspaceEventHandler
	{
	public:
		WorkspaceActivatedHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<WorkspaceEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	class WorkspaceDeactivatedHandler : public WorkspaceEventHandler
	{
	public:
		WorkspaceDeactivatedHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<WorkspaceEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	// Button Events
	class ShowPaletteCommandExecuteHandler : public CommandEventHandler
	{
	public:
		ShowPaletteCommandExecuteHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	class ShowPaletteCommandCreatedHandler : public CommandCreatedEventHandler
	{
	public:
		ShowPaletteCommandCreatedHandler(EUI * eui) : eui(eui) {}
		~ShowPaletteCommandCreatedHandler();
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
	private:
		EUI * eui;

		ShowPaletteCommandExecuteHandler * showPaletteCommandExecuteHandler = nullptr;
		Ptr<Command> command;
	};

	// Palette Events
	class ReceiveFormDataHandler : public HTMLEventHandler
	{
	public:
		ReceiveFormDataHandler(EUI * eui) : eui(eui) { }
		void notify(const Ptr<HTMLEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	class CloseExporterFormEventHandler : public UserInterfaceGeneralEventHandler
	{
	public:
		CloseExporterFormEventHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};
}
