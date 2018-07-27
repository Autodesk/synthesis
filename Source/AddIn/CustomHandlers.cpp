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
	exec->add(onShowPaletteCommandExecuted);
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	Ptr<UserInterface> UI = app->userInterface();
	if (!UI)
		return;

	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById(K_EXPORT_PALETTE);
	if (!palette)
		return;

	Exporter exporter(app);
	palette->sendInfoToHTML("joints", exporter.stringifyJoints(exporter.collectJoints()));
	palette->isVisible(true);
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	Ptr<UserInterface> UI = app->userInterface();
	if (!UI)
		return;

	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById(K_EXPORT_PALETTE);
	if (!palette)
		return;

	if (eventArgs->action() == "export")
	{
		Exporter exporter(app);

		std::vector<Ptr<Joint>> joints = exporter.collectJoints();

		BXDJ::ConfigData config;

		// Create config
		std::string dataReceived = eventArgs->data();
		for (int i = 0; i < joints.size() && i < dataReceived.length(); i++)
		{
			BXDJ::Driver driver(BXDJ::Driver::MOTOR);

			driver.portA = (dataReceived[i] == 'L') ? 0 : 1;

			driver.setComponent(BXDJ::Wheel());

			config.setDriver(joints[i], driver);
		}

		palette->isVisible(false);
		exporter.exportMeshes(config);
	}
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{}
