#include "CustomHandlers.h"

using namespace Synthesis;

/// Button Events
// Create Palette Button Event
void ShowPaletteCommandCreatedHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	Ptr<Command> command = eventArgs->command();
	if (!command)
		return;

	Ptr<CommandEvent> exec = command->execute();
	if (!exec)
		return;

	// Add click command to button
	ShowPaletteCommandExecuteHandler * onShowPaletteCommandExecuted = new ShowPaletteCommandExecuteHandler;
	onShowPaletteCommandExecuted->app = app;
	onShowPaletteCommandExecuted->palette = palette;
	exec->add(onShowPaletteCommandExecuted);
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	Exporter exporter(app);
	palette->sendInfoToHTML("send", exporter.collectJoints());
	palette->isVisible(true);
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	Exporter exporter(app);
	exporter.exportMeshes();
	palette->isVisible(false);
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{}
