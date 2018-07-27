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
	ShowPaletteCommandExecuteHandler * onShowPaletteCommandExecuted = new ShowPaletteCommandExecuteHandler(app);
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
		for (int j = 0, i = 0; j < joints.size() && i < dataReceived.length(); j++)
		{
			if ((dataReceived[i++] + ASCII_OFFSET) == 1)
			{
				BXDJ::Driver driver((BXDJ::Driver::Type)(dataReceived[i++] + ASCII_OFFSET));

				driver.portA = (int)(dataReceived[i++] - 1 + ASCII_OFFSET);
				driver.portB = (int)(dataReceived[i++] - 1 + ASCII_OFFSET);

				if ((dataReceived[i++] + ASCII_OFFSET) == 1)
				{
					BXDJ::Wheel wheel((BXDJ::Wheel::Type)(dataReceived[i++] + ASCII_OFFSET));
					wheel.frictionLevel = (BXDJ::Wheel::FrictionLevel)(dataReceived[i++] + ASCII_OFFSET);
					wheel.isDriveWheel = (dataReceived[i++] + ASCII_OFFSET) == 1;
					driver.setComponent(wheel);
				}

				config.setDriver(joints[j], driver);
			}
		}

		palette->isVisible(false);
		exporter.exportMeshes(config);
	}
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{}
