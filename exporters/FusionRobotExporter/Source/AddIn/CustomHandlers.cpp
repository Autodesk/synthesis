 #include "CustomHandlers.h"
#include "Identifiers.h"
#include "EUI.h"
#include "../Data/BXDJ/Utility.h"
#include "../Exporter.h"
#include "../Data/Filesystem.h"
#include "../Data/BXDJ/Driver.h"
#include "../Data/BXDJ/Components.h"
#include "Analytics.h"

 using namespace SynthesisAddIn;

/// General Events
// Once Fusion 360 is done loading
void DocumentOpenedHandler::notify(const Ptr<DocumentEventArgs>& eventArgs)
{
	if (Analytics::firstLaunchNotification)
		eui->showFirstLaunchNotification();
}

/// Workspace Events
// Activate Workspace Event
void WorkspaceActivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == "FusionSolidEnvironment")
	{
		Analytics::StartSession(eui->getApp());
		eui->prepareAllPalettes();
		if (Analytics::guideEnabled)
			eui->openGuidePalette();
		else
			eui->closeGuidePalette();
	}
}

// Deactivate Workspace Event
void WorkspaceDeactivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == "FusionSolidEnvironment")
	{
		eui->closeAllPalettes();
		Analytics::EndSession();
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
	if (id == SynthesisAddIn::BTN_DT_TYPE)
		eui->openDriveTypePalette();
	else if (id == SynthesisAddIn::BTN_DT_WEIGHT)
		eui->openDriveWeightPalette();
	else if (id == SynthesisAddIn::BTN_EDIT_JOINTS)
		eui->openJointEditorPalette();
	//else if (id == SynthesisAddIn::BTN_DOF) {
	//	eui->toggleDOF();
	//	eui->toggleKeyPalette();
	/*} */ else if (id == SynthesisAddIn::BTN_SETTINGS)
	{
		eui->openSettingsPalette();
	}
	else if (id == SynthesisAddIn::BTN_EXPORT)
		eui->openFinishPalette();
}
// Conversion helper
std::wstring s2ws(const std::string& s)
{
	int len;
	int slength = (int)s.length() + 1;
	len = MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, 0, 0);
	wchar_t* buf = new wchar_t[len];
	MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, buf, len);
	std::wstring r(buf);
	delete[] buf;
	return r;
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	if (eventArgs->action() == "highlight")
		eui->highlightAndFocusSingleJoint(eventArgs->data(), false, 1);
	else if (eventArgs->action() == "edit_sensors")
		eui->openSensorsPalette(eventArgs->data());
	else if (eventArgs->action() == "save_sensors")
		eui->closeSensorsPalette(eventArgs->data());
	else if (eventArgs->action() == "settings_guide")
		eui->closeSettingsPalette(eventArgs->data());
	else if (eventArgs->action() == "open_link")
	{
		std::wstring stemp = s2ws(eventArgs->data());
		LPCWSTR result = stemp.c_str();
		ShellExecute(0, 0, result, 0, 0, SW_SHOWNORMAL);
		//system("open http://google.com"); opens link on Linux/macOS/Unix

	}
	else if (eventArgs->action() == "settings_analytics")
	{
		Analytics::SetEnabled(eventArgs->data() == "true");
	}
	else if (eventArgs->action() == "close")
	{
		if (eventArgs->data() == "drivetrain_type")
		{
			eui->closeDriveTypePalette();
		} else if (eventArgs->data() == "drivetrain_weight")
		{
			eui->closeDriveWeightPalette();
		}
		else if (eventArgs->data() == "joint_editor")
		{
			eui->closeSensorsPalette();
			eui->closeJointEditorPalette();
		} else if (eventArgs->data() == "settings") {
			eui->closeSettingsPalette("");
		} else if (eventArgs->data() == "export")
		{
			eui->closeFinishPalette();
		}
	}
	else if (eventArgs->action() == "save" || eventArgs->action() == "dt_weight_save" || eventArgs->action() == "drivetrain_type" || eventArgs->action() == "export" || eventArgs->action() == "export-and-open") {
		eui->saveConfiguration(eventArgs->data());

		if (eventArgs->action() == "save") {
			eui->closeJointEditorPalette();
		}
		else if (eventArgs->action() == "drivetrain_type") {
			eui->closeDriveTypePalette();
		}
		else if (eventArgs->action() == "dt_weight_save") {
			eui->closeDriveWeightPalette();
		}
		else {// export
			eui->startExportRobot(eventArgs->action() == "export-and-open"); // TODO: export-and-open is lazy, include this in the JSON
		}
	}
}

// Close Exporter Form Event
void ClosePaletteEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{
	if (id == SynthesisAddIn::PALETTE_DT_TYPE)
		eui->closeDriveTypePalette();
	else if (id == SynthesisAddIn::PALETTE_DT_WEIGHT)
		eui->closeDriveWeightPalette();
	else if (id == SynthesisAddIn::PALETTE_JOINT_EDITOR)
		eui->closeJointEditorPalette();
	else if (id == SynthesisAddIn::PALETTE_GUIDE) {
		eui->closeGuidePalette();
		Analytics::SaveSettings();
	}
	else if (id == SynthesisAddIn::PALETTE_SETTINGS)
		eui->closeSettingsPalette("");
	else if (id == SynthesisAddIn::PALETTE_FINISH)
		eui->closeFinishPalette();
}
