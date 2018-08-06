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
	class ShowPaletteCommandCreatedHandler : public CommandCreatedEventHandler
	{
	public:
		ShowPaletteCommandCreatedHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	class ShowPaletteCommandExecuteHandler : public CommandEventHandler
	{
	public:
		ShowPaletteCommandExecuteHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	// Palette Events
	class ReceiveFormDataHandler : public HTMLEventHandler
	{
	public:
		ReceiveFormDataHandler(Ptr<Application> app, EUI * eui) : app(app), eui(eui) { }
		~ReceiveFormDataHandler() { if (thread != nullptr) { thread->join(); delete thread; } }
		void notify(const Ptr<HTMLEventArgs>& eventArgs) override;
	private:
		const char ASCII_OFFSET = -32;
		Ptr<Application> app;
		EUI * eui;
		// Some events don't like doing everything on a single thread
		std::thread * thread = nullptr;
	};

	class CloseExporterFormEventHandler : public UserInterfaceGeneralEventHandler
	{
	public:
		CloseExporterFormEventHandler(Ptr<Application> app) : app(app) {}
		void notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) override;
	private:
		Ptr<Application> app;
	};
}
