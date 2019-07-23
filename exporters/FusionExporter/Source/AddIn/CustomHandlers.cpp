#include "CustomHandlers.h"
#include "Identifiers.h"
#include "EUI.h"
#include "../Data/BXDJ/Utility.h"
#include "../Exporter.h"
#include "../Data/Filesystem.h"
#include "../Data/BXDJ/Driver.h"
#include "../Data/BXDJ/Components.h"

using namespace SynthesisAddIn;

/// Workspace Events
// Activate Workspace Event
void WorkspaceActivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == WORKSPACE_SYNTHESIS)
	{
		eui->prepareAllPalettes();
		eui->openGuidePalette();
	}
}

// Deactivate Workspace Event
void WorkspaceDeactivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == WORKSPACE_SYNTHESIS)
	{
		eui->closeDriveTypePalette("");
		eui->closeJointEditorPalette();
		eui->closeGuidePalette();
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
		showPaletteCommandExecuteHandler = new ShowPaletteCommandExecuteHandler(eui, id);

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
	if (id == SynthesisAddIn::BTN_WEIGHT)
	{
		eui->openDriveWeightPalette();
	}
    else if (id == SynthesisAddIn::BTN_DT_TYPE)
    {
		eui->openDriveTypePalette();
    }
	else if (id == SynthesisAddIn::BTN_GUIDE)
	{
		eui->openGuidePalette();
	}
	else if (id == SynthesisAddIn::BTN_EDIT_JOINTS)
	{
		eui->openJointEditorPalette();
	}
	else if (id == SynthesisAddIn::BTN_EXPORT)
	{
		eui->openFinishPalette();
  }
	else if (id == SynthesisAddIn::BTN_DOF) {
		eui->toggleDOF();
		eui->toggleKeyPalette();
	}
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	if (eventArgs->action() == "drivetrain_type") {
		eui->closeDriveTypePalette(eventArgs->data());

	} else if (eventArgs->action() == "highlight") {
		eui->highlightAndFocusSingleJoint(eventArgs->data(), false, 1);
	} else if (eventArgs->action() == "edit_sensors") {
		eui->openSensorsPalette(eventArgs->data());
	} else if (eventArgs->action() == "save_sensors") {
		eui->closeSensorsPalette(eventArgs->data());
	} else if (eventArgs->action() == "dt_weight_save") {
		eui->closeDriveWeightPalette("");
	} else if (eventArgs->action() == "save" || eventArgs->action() == "export") {
		eui->saveConfiguration(eventArgs->data());

		if (eventArgs->action() == "export")
			eui->startExportRobot();
	}
}

// Close Exporter Form Event
void ClosePaletteEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{
	if (id == SynthesisAddIn::PALETTE_DT_TYPE)
		eui->closeDriveTypePalette("");
	if (id == SynthesisAddIn::PALETTE_JOINT_EDITOR)
		eui->closeJointEditorPalette();
	else if (id == SynthesisAddIn::PALETTE_FINISH)
		eui->closeFinishPalette();
	else if (id == SynthesisAddIn::PALETTE_GUIDE)
		eui->closeGuidePalette();
}
