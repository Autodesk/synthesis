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
	onShowPaletteCommandExecuted->joints = joints;
	onShowPaletteCommandExecuted->app = app;
	onShowPaletteCommandExecuted->palette = palette;
	exec->add(onShowPaletteCommandExecuted);
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	Exporter exporter(app);
	palette->sendInfoToHTML("send", exporter.collectJoints(*joints));
	palette->isVisible(true);
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	Exporter exporter(app);

	// Create config
	BXDJ::ConfigData config;

	for (int i = 0; i < joints->size() && i < eventArgs->data().length(); i++)
	{
		BXDJ::Driver driver(BXDJ::Driver::MOTOR);

		driver.portA = (eventArgs->data()[i] == 'L') ? 0 : 1;

		config.setDriver((*joints)[i], driver);
	}

	exporter.exportMeshes(config);
	palette->isVisible(false);
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{}
