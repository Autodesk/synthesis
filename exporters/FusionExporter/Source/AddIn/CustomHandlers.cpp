#include "CustomHandlers.h"
#include "Identifiers.h"
#include "EUI.h"
#include "../Exporter.h"
#include "../Data/Filesystem.h"
#include "../Data/BXDJ/Driver.h"
#include "../Data/BXDJ/Components.h"

using namespace SynthesisAddIn;

/// Workspace Events
// Activate Workspace Event
void WorkspaceActivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == K_WORKSPACE)
	{
		eui->preparePalettes();
	}
}

// Deactivate Workspace Event
void WorkspaceDeactivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == K_WORKSPACE)
	{
		eui->closeExportPalette();
		eui->cancelExportRobot();
	}
}

/// Button Events
// Create Palette Button Event
void ShowPaletteCommandCreatedHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	command = eventArgs->command();
	if (!command)
		return;

	// Create handler
	if (showPaletteCommandExecuteHandler == nullptr)
		showPaletteCommandExecuteHandler = new ShowPaletteCommandExecuteHandler(eui);

	Ptr<CommandEvent> exec = command->execute();
	if (exec)
		exec->add(showPaletteCommandExecuteHandler);
}

ShowPaletteCommandCreatedHandler::~ShowPaletteCommandCreatedHandler()
{
	if (showPaletteCommandExecuteHandler == nullptr)
		return;

	if (!command)
		return;

	Ptr<CommandEvent> exec = command->execute();
	if (exec)
		exec->remove(showPaletteCommandExecuteHandler);

	delete showPaletteCommandExecuteHandler;
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	eui->openExportPalette();
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	if (eventArgs->action() == "highlight")
		eui->highlightJoint(eventArgs->data());

	else if (eventArgs->action() == "edit_sensors")
		eui->openSensorsPalette(eventArgs->data());

	else if (eventArgs->action() == "save_sensors")
		eui->closeSensorsPalette(eventArgs->data());

	else if (eventArgs->action() == "dt_weight_save") {
		
	}
		

	else if (eventArgs->action() == "save" || eventArgs->action() == "export")
	{
		eui->saveConfiguration(eventArgs->data());

		if (eventArgs->action() == "export")
			eui->startExportRobot();
	}
}

// Close Exporter Form Event
void CloseExporterFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{
	eui->closeExportPalette();
}
