#pragma once

#include <Core/CoreAll.h>
#include <thread>

using namespace adsk::core;
using namespace adsk::fusion;

namespace SynthesisAddIn
{
	class EUI;

	// Workspace Events
	/// Notified when the Synthesis workspace is selected by the user.
	class WorkspaceActivatedHandler : public WorkspaceEventHandler
	{
	public:
		WorkspaceActivatedHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<WorkspaceEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	/// Notified when the user leaves the Synthesis workspace.
	class WorkspaceDeactivatedHandler : public WorkspaceEventHandler
	{
	public:
		WorkspaceDeactivatedHandler(EUI * eui) : eui(eui) {}
		void notify(const Ptr<WorkspaceEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	// Button Events
	/// Notified when the export robot button is created.
	class ShowPaletteCommandExecuteHandler : public CommandEventHandler
	{
	public:
		ShowPaletteCommandExecuteHandler(EUI * eui, std::string id_) : eui(eui), id(id_) {  }
		void notify(const Ptr<CommandEventArgs>& eventArgs) override;
	private:
		EUI * eui;
		std::string id;
	};

	/// Notified when the export robot button is clicked.
	class ShowPaletteCommandCreatedHandler : public CommandCreatedEventHandler
	{
	public:
		ShowPaletteCommandCreatedHandler(EUI* eui, std::string id_) : eui(eui), id(id_) {  }
		~ShowPaletteCommandCreatedHandler();
		void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
		void setId(std::string id_) { id = id_; }
	private:
		EUI * eui;
		std::string id;

		ShowPaletteCommandExecuteHandler * showPaletteCommandExecuteHandler = nullptr;
		Ptr<Command> command;
	};

	// Palette Events
	/// Notified when a palette sends data back to Fusion.
	class ReceiveFormDataHandler : public HTMLEventHandler
	{
	public:
		ReceiveFormDataHandler(EUI * eui) : eui(eui) { }
		void notify(const Ptr<HTMLEventArgs>& eventArgs) override;
	private:
		EUI * eui;
	};

	/// Notified when the export robot form is closed.
	class ClosePaletteEventHandler : public UserInterfaceGeneralEventHandler
	{
	public:
		ClosePaletteEventHandler(EUI * eui, std::string id_) : eui(eui), id(id_) {}
		void notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) override;
	private:
		EUI * eui;
		std::string id;
	};
}
