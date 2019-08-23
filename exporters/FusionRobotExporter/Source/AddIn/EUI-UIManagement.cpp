#include "EUI.h"
#include "../Exporter.h"
#include "../Data/BXDJ/Utility.h"
#include "Analytics.h"

using namespace SynthesisAddIn;

// Developer Note: When sending data to HTML pages, data is sent twice. Sometimes pages do not receive/process the
//                 first set of data in time, so sending it a second time ensures the data is received. Additionally,
//                 sending data before displaying the page improves the chances that the data will already be rendering
//                 by the time the window is made visible.

// WORKSPACE

bool EUI::createWorkspace()
{
	try
	{
		// Create workspace
		// workSpace = UI->workspaces()->itemById(WORKSPACE_SYNTHESIS);
		// if (!workSpace)
		// {
			// workSpace = UI->workspaces()->add("DesignProductType", WORKSPACE_SYNTHESIS, "Synthesis", "Resources/FinishIcons");
			// workSpace->tooltip("Export robot models to the Synthesis simulator");

			addHandler<DocumentOpenedHandler>(UI, documentOpenedHandler);

			addHandler<WorkspaceActivatedHandler>(UI, workspaceActivatedHandler);
			addHandler<WorkspaceDeactivatedHandler>(UI, workspaceDeactivatedHandler);

			workSpace = UI->allToolbarTabs()->itemById("ToolsTab");

			createPanels();
			createButtons();
		// }

		return true;
	}
	catch (std::exception e)
	{
		UI->messageBox("Failed to load Synthesis Exporter add-in.");
		return false;
	}
}

void EUI::deleteWorkspace()
{
	clearHandler<WorkspaceActivatedHandler>(UI, workspaceActivatedHandler);
	clearHandler<WorkspaceDeactivatedHandler>(UI, workspaceDeactivatedHandler);

	// Delete palettes
	deleteDriveWeightPalette();
	deleteDriveTypePalette();
	deleteJointEditorPalette();
	deleteSensorsPalette();
	// deleteKeyPalette();
	deleteGuidePalette();
	deleteSettingsPalette();
	deleteFinishPalette();
	deleteProgressPalette();

	// Delete buttons
	deleteButtons();

	// Delete event handlers
	delete workspaceActivatedHandler;
	delete workspaceDeactivatedHandler;
	delete showPaletteCommandCreatedHandler;
	delete receiveFormDataHandler;
	delete closeExporterFormEventHandler;
}

// PALETTES

void EUI::prepareAllPalettes()
{
	createDriveTypePalette();
	createDriveWeightPalette();
	createJointEditorPalette();
	createSensorsPalette();
	// createKeyPalette();
	createSettingsPalette();
	createFinishPalette();
	createProgressPalette();
	createGuidePalette();
}

void EUI::closeAllPalettes()
{
	closeDriveTypePalette();
	closeDriveWeightPalette();
	closeJointEditorPalette();
	closeSensorsPalette();
	closeGuidePalette(false);
	closeSettingsPalette("");
	cancelExportRobot();
}

void EUI::closeEditorPalettes()
{
	closeDriveTypePalette();
	closeDriveWeightPalette();
	closeJointEditorPalette();
	closeSensorsPalette();
	// closeGuidePalette();
	//closeKeyPalette();
	closeSettingsPalette("");
	closeFinishPalette();
}

void EUI::disableEditorButtons()
{
	driveTrainTypeButton->controlDefinition()->isEnabled(false); ///< Export robot button.
	driveTrainWeightButton->controlDefinition()->isEnabled(false);
	editJointsButton->controlDefinition()->isEnabled(false); ///< Export robot button.
	// editDOFButton->controlDefinition()->isEnabled(false); ///< Export robot button.
	// keyDOFButton->controlDefinition()->isEnabled(false); ///< Export robot button.
	// robotExportGuideButton->controlDefinition()->isEnabled(false); ///< Export robot button.
	settingsButton->controlDefinition()->isEnabled(false);
	finishButton->controlDefinition()->isEnabled(false); ///< Export robot button.
}

void EUI::enableEditorButtons()
{
	driveTrainTypeButton->controlDefinition()->isEnabled(true); ///< Export robot button.
	driveTrainWeightButton->controlDefinition()->isEnabled(true);
	editJointsButton->controlDefinition()->isEnabled(true); ///< Export robot button.
	// editDOFButton->controlDefinition()->isEnabled(true); ///< Export robot button.
	// keyDOFButton->controlDefinition()->isEnabled(true); ///< Export robot button.
	// robotExportGuideButton->controlDefinition()->isEnabled(true); ///< Export robot button.
	settingsButton->controlDefinition()->isEnabled(true);
	finishButton->controlDefinition()->isEnabled(true); ///< Export robot button.
}

// First Launch Notification

void EUI::showFirstLaunchNotification()
{
	DialogResults res = UI->messageBox("The Synthesis robot exporter add-in has been installed. To access the exporter, select the \"Tools\" tab under the \"Design\" workspace.", "Synthesis Add-In", MessageBoxButtonTypes::OKButtonType, InformationIconType);
	if (res == DialogOK)
	{
		Analytics::firstLaunchNotification = false;
		Analytics::SaveSettings();
	}
}

// Drivetrain Weight Palette

bool EUI::createDriveWeightPalette() {
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	driveWeightPalette = palettes->itemById(PALETTE_DT_WEIGHT);
	if (!driveWeightPalette)
	{
		driveWeightPalette = palettes->add(PALETTE_DT_WEIGHT, "Synthesis Drivetrain Weight", "palette/drivetrain_weight.html", false, false, true, 300, 140+HEADER_HEIGHT);
		if (!driveWeightPalette)
			return false;

		driveWeightPalette->setMaximumSize(300, 178+HEADER_HEIGHT);

		driveWeightPalette->dockingState(PaletteDockingStates::PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(driveWeightPalette, driveWeightReceiveFormDataHandler);
		addHandler<ClosePaletteEventHandler>(driveWeightPalette, driveWeightClosePaletteHandler);
	}

	return true;
}

void EUI::deleteDriveWeightPalette() {
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette exists
	driveWeightPalette = palettes->itemById(PALETTE_DT_WEIGHT);

	if (!driveWeightPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(driveWeightPalette, driveWeightReceiveFormDataHandler);
	clearHandler<ClosePaletteEventHandler>(driveWeightPalette, driveWeightClosePaletteHandler);

	driveWeightPalette->deleteMe();
	driveWeightPalette = nullptr;
}

void EUI::openDriveWeightPalette() {
	closeEditorPalettes();
	disableEditorButtons();
	driveTrainWeightButton->controlDefinition()->isEnabled(false);

	// In some cases, sending info to the HTML of a palette on the same thread causes issues
	static std::thread* uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this](std::string configJSON)
	{
		driveWeightPalette->sendInfoToHTML("state", configJSON);
		driveWeightPalette->isVisible(true);
		driveWeightPalette->sendInfoToHTML("state", configJSON);
	}, Exporter::loadConfiguration(app->activeDocument()).toJSONString());
	Analytics::LogPage(U("Drivetrain Weight Editor"));
}

void EUI::closeDriveWeightPalette() {
	driveWeightPalette->isVisible(false);
	enableEditorButtons();
}

// Export Palette

bool EUI::createJointEditorPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	jointEditorPalette = palettes->itemById(PALETTE_JOINT_EDITOR);
	if (!jointEditorPalette)
	{
		jointEditorPalette = palettes->add(PALETTE_JOINT_EDITOR, "Synthesis Joint Editor", "palette/jointEditor.html", false, false, true, 450, 200);
		if (!jointEditorPalette)
			return false;

		jointEditorPalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(jointEditorPalette, jointEditorReceiveFormDataHandler);
		addHandler<ClosePaletteEventHandler>(jointEditorPalette, jointEditorPaletteHandler);
	}

	return true;
}

void EUI::deleteJointEditorPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette exists
	jointEditorPalette = palettes->itemById(PALETTE_JOINT_EDITOR);

	if (!jointEditorPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(jointEditorPalette, jointEditorReceiveFormDataHandler);
	clearHandler<ClosePaletteEventHandler>(jointEditorPalette, jointEditorClosePaletteEventHandler);

	jointEditorPalette->deleteMe();
	jointEditorPalette = nullptr;
}

void EUI::openJointEditorPalette()
{
	closeEditorPalettes();
	disableEditorButtons();
	// In some cases, sending info to the HTML of a palette on the same thread causes issues
	static std::thread * uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	BXDJ::ConfigData config = Exporter::loadConfiguration(app->activeDocument()); // Load joint config and update with current joints in document

	Ptr<Camera> ogCam = app->activeViewport()->camera(); // Save the original camera position

	config.tempIconDir = std::experimental::filesystem::temp_directory_path().string() + "Synthesis\\FusionIconCache\\"; // Get OS temp dir for icons and save to JSON object

	int index = 0;
	for (std::pair<const std::basic_string<char>, BXDJ::ConfigData::JointConfig> joint : config.getJoints()) // For each joint, focus on the joint, take a pic, save to temp dir
	{
		EUI::highlightAndFocusSingleJoint(joint.first, false, 0.6);
		app->activeViewport()->saveAsImageFile(config.tempIconDir+std::to_string(index)+".png", 90, 90); // TODO: Is this cross-platform?
		index++;
	};

	// EUI::resetHighlightAndFocusWholeModel(true, 1.5, ogCam); // clear highlight and move camera to look at whole robot

	uiThread = new std::thread([this](std::string configJSON) // Actually open the palette and send the joint data
	{
		jointEditorPalette->sendInfoToHTML("joints", configJSON); // TODO: Why is this duplicated
		jointEditorPalette->isVisible(true);
		jointEditorPalette->sendInfoToHTML("joints", configJSON);
	}, config.toJSONString());
	Analytics::LogPage(U("Joint Editor"));

	UI->activeSelections()->clear();
}


void EUI::closeJointEditorPalette()
{
	enableEditorButtons();
	jointEditorPalette->isVisible(false);
	sensorsPalette->isVisible(false);
}

// Guide Palette

bool EUI::createGuidePalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	guidePalette = palettes->itemById(PALETTE_GUIDE);
	if (!guidePalette)
	{
		// Create palette
		guidePalette = palettes->add(PALETTE_GUIDE, "Synthesis Robot Export Guide", "palette/guide.html", false, true, true, 470, 200);

		if (!guidePalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		guidePalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(guidePalette, guideReceiveFormDataHandler);
		addHandler<ClosePaletteEventHandler>(guidePalette, guideCloseGuideFormEventHandler);
	}

	return true;
}

void EUI::deleteGuidePalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette already exists
	guidePalette = palettes->itemById(PALETTE_GUIDE);
	if (!guidePalette)
		return;

	clearHandler<ReceiveFormDataHandler>(guidePalette, guideReceiveFormDataHandler);
	clearHandler<ClosePaletteEventHandler>(guidePalette, guideCloseGuideFormEventHandler);

	guidePalette->deleteMe();
	guidePalette = nullptr;
}

void EUI::openGuidePalette()
{
	robotExportGuideButton->controlDefinition()->isEnabled(false);
	guidePalette->isVisible(true);
	Analytics::guideEnabled = true;
}

void EUI::closeGuidePalette(bool manualClose)
{
	robotExportGuideButton->controlDefinition()->isEnabled(true);
	guidePalette->isVisible(false);

	if (manualClose) Analytics::guideEnabled = false;

	static std::thread* uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this]()
	{
		settingsPalette->sendInfoToHTML("settings_guide", Analytics::guideEnabled ? "true" : "false");
		settingsPalette->sendInfoToHTML("settings_guide", Analytics::guideEnabled ? "true" : "false");
	});
}

// Key Palette

// TO BE RE-IMPLEMENTED LATER

//bool EUI::createKeyPalette()
//{
//	Ptr<Palettes> palettes = UI->palettes();
//	if (!palettes)
//		return false;
//
//	// Check if palette already exists
//	keyPalette = palettes->itemById(PALETTE_KEY);
//	if (!keyPalette)
//	{
//		// Create palette
//		keyPalette = palettes->add(PALETTE_KEY, "Degrees of Freedom Key", "palette/dofkey.html", false, true, false, 220, 165);
//		if (!keyPalette)
//			return false;
//
//		// Dock the palette to float
//		keyPalette->dockingState(PaletteDockStateFloating);
//		keyPalette->setPosition(500, 500);
//
//		addHandler<ReceiveFormDataHandler>(keyPalette, keyCloseFormDataEventHandler);
//		addHandler<ClosePaletteEventHandler>(keyPalette, keyClosePaletteEventHandler);
//	}
//
//	return true;
//}
//
//void EUI::deleteKeyPalette()
//{
//	Ptr<Palettes> palettes = UI->palettes();
//	if (!palettes)
//		return;
//
//	// Check if palette already exists
//	keyPalette = palettes->itemById(PALETTE_KEY);
//	if (!keyPalette)
//		return;
//
//	clearHandler<ReceiveFormDataHandler>(keyPalette, keyCloseFormDataEventHandler);
//	clearHandler<ClosePaletteEventHandler>(keyPalette, keyClosePaletteEventHandler);
//
//	keyPalette->deleteMe();
//	keyPalette = nullptr;
//}
//
//void EUI::toggleKeyPalette()
//{
//	keyPalette->isVisible(dofViewEnabled);
//}

// Finish palette

bool EUI::createFinishPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	finishPalette = palettes->itemById(PALETTE_FINISH);
	if (!finishPalette)
	{

		finishPalette = palettes->add(PALETTE_FINISH, "Finish Synthesis Exporter", "palette/export.html", false, false, false, 340, 130+HEADER_HEIGHT);
		if (!finishPalette)
			return false;

		finishPalette->setMaximumSize(300, 192+HEADER_HEIGHT);

		finishPalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(finishPalette, finishPaletteReceiveFormDataHandler);
		addHandler<ClosePaletteEventHandler>(finishPalette, finishPaletteCloseEventHandler);
	}

	return true;
}

void EUI::deleteFinishPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette exists
	finishPalette = palettes->itemById(PALETTE_FINISH);

	if (!finishPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(finishPalette, finishPaletteReceiveFormDataHandler);
	clearHandler<ClosePaletteEventHandler>(finishPalette, finishPaletteCloseEventHandler);

	finishPalette->deleteMe();
	finishPalette = nullptr;
}

void EUI::openFinishPalette()
{
	closeEditorPalettes();
	disableEditorButtons();
	finishButton->controlDefinition()->isEnabled(false);

	// In some cases, sending info to the HTML of a palette on the same thread causes issues
	static std::thread * uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	BXDJ::ConfigData config = Exporter::loadConfiguration(app->activeDocument()); // Load joint config and update with current joints in document

	// Field Names
	// PWSTR path = NULL;
	//
	// std::string fieldNames = "[";
	//
	// HRESULT result = SHGetKnownFolderPath(FOLDERID_RoamingAppData, 0, NULL, &path);
	// if (SUCCEEDED(result)) {
	// 	std::wstring relativeSynthesis = L"\\Autodesk\\Synthesis\\Fields";
	// 	for (const auto& entry : std::experimental::filesystem::directory_iterator(path+relativeSynthesis))
	// 		fieldNames+="\""+entry.path().filename().string()+"\",";
	// }
	// fieldNames.pop_back(); // remove last comma
	// fieldNames += "]";
	//
	// CoTaskMemFree(path);
	uiThread = new std::thread([this](std::string configJSON) // Actually open the palette and send the joint data
	{
		finishPalette->sendInfoToHTML("joints", configJSON); // TODO: Why is this duplicated
		finishPalette->isVisible(true);
		finishPalette->sendInfoToHTML("joints", configJSON);
	}, config.toJSONString());
	Analytics::LogPage(U("Pre-Export Form"));
}

void EUI::closeFinishPalette()
{
	enableEditorButtons();
	finishPalette->isVisible(false);
}

// Sensors Palette

bool EUI::createSensorsPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	sensorsPalette = palettes->itemById(PALETTE_SENSORS);
	if (!sensorsPalette)
	{
		// Create palette
		sensorsPalette = palettes->add(PALETTE_SENSORS, "Synthesis Advanced Joint Settings", "palette/sensors.html", false, false, true, 300, 200);
		if (!sensorsPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		sensorsPalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(sensorsPalette, sensorsReceiveFormDataHandler);
	}

	return true;
}

void EUI::deleteSensorsPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette already exists
	sensorsPalette = palettes->itemById(PALETTE_SENSORS);

	if (!sensorsPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(sensorsPalette, sensorsReceiveFormDataHandler);

	sensorsPalette->deleteMe();
	sensorsPalette = nullptr;
}

void EUI::openSensorsPalette(std::string sensors)
{
	disableEditorButtons();
	static std::thread * uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this](std::string sensors)
	{
		sensorsPalette->sendInfoToHTML("sensors", sensors);
		sensorsPalette->isVisible(true);
		sensorsPalette->sendInfoToHTML("sensors", sensors);
	}, sensors);
	Analytics::LogPage(U("Joint Editor"), U("Optional Settings Editor"));
}

void EUI::closeSensorsPalette(std::string sensorsToSave)
{
	enableEditorButtons();
	sensorsPalette->isVisible(false);

	if (sensorsToSave.length() > 0)
	{
		static std::thread * uiThread = nullptr;
		if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

		uiThread = new std::thread([this](std::string sensors) { jointEditorPalette->sendInfoToHTML("sensors", sensors); }, sensorsToSave);
	}
}

// Drive Type Palette

bool EUI::createDriveTypePalette() {
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	driveTypePalette = palettes->itemById(PALETTE_DT_TYPE);
	if (!driveTypePalette)
	{
		// Create palette
		driveTypePalette = palettes->add(PALETTE_DT_TYPE, "Synthesis Drivetrain Layout", "palette/drivetrain_type.html", false, false, false, 350, 210 + HEADER_HEIGHT);
		if (!driveTypePalette)
			return false;

		driveTypePalette->setMaximumSize(350, 259+HEADER_HEIGHT);

		// Dock the palette to the right side of Fusion window.
		driveTypePalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(driveTypePalette, driveTypeReceiveFormDataHandler);
		addHandler<ClosePaletteEventHandler>(driveTypePalette, driveTypeClosePaletteHandler);
	}

	return true;
}

void EUI::deleteDriveTypePalette() {
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette exists
	driveTypePalette = palettes->itemById(PALETTE_DT_TYPE);

	if (!driveTypePalette)
		return;

	clearHandler<ReceiveFormDataHandler>(driveTypePalette, driveTypeReceiveFormDataHandler);
	clearHandler<ClosePaletteEventHandler>(driveTypePalette, driveTypeClosePaletteHandler);

	driveTypePalette->deleteMe();
	driveTypePalette = nullptr;
}

void EUI::openDriveTypePalette() {
	closeEditorPalettes();
	disableEditorButtons();
	driveTrainTypeButton->controlDefinition()->isEnabled(false);

	// In some cases, sending info to the HTML of a palette on the same thread causes issues
	static std::thread* uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	BXDJ::ConfigData config = Exporter::loadConfiguration(app->activeDocument()); // Load joint config and update with current joints in document
	uiThread = new std::thread([this](std::string configJSON) // Actually open the palette and send the joint data
	{
		driveTypePalette->sendInfoToHTML("joints", configJSON); // TODO: Why is this duplicated
		driveTypePalette->isVisible(true);
		driveTypePalette->sendInfoToHTML("joints", configJSON);
	}, config.toJSONString());
	Analytics::LogPage(U("Drivetrain Type Editor"));
}

void EUI::closeDriveTypePalette() {
	enableEditorButtons();
	driveTrainTypeButton->controlDefinition()->isEnabled(true);
	driveTypePalette->isVisible(false);
}
// Progress Palette

bool EUI::createProgressPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	progressPalette = palettes->itemById(PALETTE_PROGRESS);
	if (!progressPalette)
	{
		// Create palette
		progressPalette = palettes->add(PALETTE_PROGRESS, "Exporting Synthesis Robot", "palette/progress.html", false, false, false, 150, 150);
		if (!progressPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		progressPalette->dockingState(PaletteDockStateBottom);
		progressPalette->dockingOption(PaletteDockOptionsToVerticalOnly);
	}

	return true;
}

void EUI::deleteProgressPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette already exists
	progressPalette = palettes->itemById(PALETTE_PROGRESS);
	if (!progressPalette)
		return;

	progressPalette->deleteMe();
	progressPalette = nullptr;
}

void EUI::openProgressPalette()
{
	static std::thread * uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	closeFinishPalette();

	uiThread = new std::thread([this]()
	{
		progressPalette->sendInfoToHTML("progress", "0");
		progressPalette->isVisible(true);
		progressPalette->sendInfoToHTML("progress", "0");
	});
}

void EUI::closeProgressPalette()
{
	progressPalette->isVisible(false);
	editJointsButton->controlDefinition()->isEnabled(true);
}

// SETTINGS PALETTE

bool EUI::createSettingsPalette() {
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	settingsPalette = palettes->itemById(PALETTE_SETTINGS);
	if (!settingsPalette)
	{
		// Create palette
		settingsPalette = palettes->add(PALETTE_SETTINGS, "Synthesis Add-In Settings", "palette/settings.html", false, false, false, 250, 102+HEADER_HEIGHT);
		if (!settingsPalette)
			return false;

		settingsPalette->setMaximumSize(250, 102+HEADER_HEIGHT);

		// Dock the palette to the right side of Fusion window.
		settingsPalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(settingsPalette, settingsReceiveFormDataHandler);
		addHandler<ClosePaletteEventHandler>(settingsPalette, settingsClosePaletteEventHandler);
	}

	return true;
}

void EUI::deleteSettingsPalette() {
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette exists
	settingsPalette = palettes->itemById(PALETTE_SETTINGS);

	if (!settingsPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(settingsPalette, settingsReceiveFormDataHandler);
	clearHandler<ClosePaletteEventHandler>(settingsPalette, settingsClosePaletteEventHandler);

	settingsPalette->deleteMe();
	settingsPalette = nullptr;
}

void EUI::openSettingsPalette()
{
	closeEditorPalettes();
	disableEditorButtons();
	static std::thread* uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this]()
		{
			settingsPalette->sendInfoToHTML("settings_guide", Analytics::guideEnabled ? "true" : "false");
			settingsPalette->sendInfoToHTML("settings_analytics", Analytics::IsEnabled() ? "true" : "false");
			settingsPalette->isVisible(true);
			settingsPalette->sendInfoToHTML("settings_guide", Analytics::guideEnabled ? "true" : "false");
			settingsPalette->sendInfoToHTML("settings_analytics", Analytics::IsEnabled() ? "true" : "false");
		});

	settingsButton->controlDefinition()->isEnabled(false);
	Analytics::LogPage(U("Exporter Settings"));
}

void EUI::closeSettingsPalette(std::string guideEnabled) {
	enableEditorButtons();
	settingsPalette->isVisible(false);

	if ((guideEnabled == "true" || guideEnabled == "false") && ((guideEnabled == "true") != Analytics::guideEnabled)) Analytics::LogEvent(U("Settings"), U("Guide Toggle"), guideEnabled == "true" ? U("Enabled") : U("Disabled"));
	if (guideEnabled == "true") // TODO: This is lazy, use JSON
	{
		openGuidePalette();
	} else if (guideEnabled == "false")
	{
		closeGuidePalette(true);
	}
	Analytics::SaveSettings();
}

// BUTTONS AND PANELS

void EUI::createPanels()
{
	driveTrainPanel = workSpace->toolbarPanels()->add(PANEL_DT, "Drive Train Setup");
	jointSetupPanel = workSpace->toolbarPanels()->add(PANEL_JOINT, "Joint Setup");
	// precheckPanel = workSpace->toolbarPanels()->add(PANEL_PRECHECK, "Export Precheck");
	settingsPanel = workSpace->toolbarPanels()->add(PANEL_SETTINGS, "Settings");
	finishPanel = workSpace->toolbarPanels()->add(PANEL_FINISH, "Finish");
}

void EUI::createButtons()
{
	driveTrainTypeButton = UI->commandDefinitions()->addButtonDefinition(BTN_DT_TYPE, "Drive Train Layout", "Select your drivetrain layout (basic, H-drive, or other).", "Resources/DriveIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(driveTrainTypeButton, driveTrainTypeShowPaletteCommandCreatedHandler);

	driveTrainWeightButton = UI->commandDefinitions()->addButtonDefinition(BTN_DT_WEIGHT, "Drive Train Weight", "Assign the weight of the drivetrain.", "Resources/WeightIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(driveTrainWeightButton, driveTrainWeightShowPaletteCommandCreatedHandler);

	editJointsButton = UI->commandDefinitions()->addButtonDefinition(BTN_EDIT_JOINTS, "Edit Joints", "Edit existing joints.", "Resources/JointIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(editJointsButton, editJointsShowPaletteCommandCreatedHandler);

	robotExportGuideButton = UI->commandDefinitions()->addButtonDefinition(BTN_GUIDE, "Robot Export Guide", "View a checklist of all necessary tasks prior to export.", "Resources/PrecheckIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(robotExportGuideButton, robotExportGuideShowPaletteCommandCreatedHandler);
	robotExportGuideButton->controlDefinition()->isVisible(false);

	//editDOFButton = UI->commandDefinitions()->addButtonDefinition(BTN_DOF, "Toggle Joint Viewer", "View degrees of freedom.", "Resources/DOFIcons");
	//addHandler<ShowPaletteCommandCreatedHandler>(editDOFButton, editDOFShowPaletteCommandCreatedHandler);

	settingsButton = UI->commandDefinitions()->addButtonDefinition(BTN_SETTINGS, "Add-In Settings", "Configure add-in settings.", "Resources/SettingsIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(settingsButton, settingsShowPaletteCommandCreatedHandler);

	finishButton = UI->commandDefinitions()->addButtonDefinition(BTN_EXPORT, "Finish Robot Export", "Setup your robot for exporting to Synthesis.", "Resources/FinishIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(finishButton, finishShowPaletteCommandCreatedHandler);


	// Add buttons to finishPanel
	driveTrainPanel->controls()->addCommand(driveTrainTypeButton)->isPromoted(true);
	driveTrainPanel->controls()->addCommand(driveTrainWeightButton)->isPromoted(true);
	jointSetupPanel->controls()->addCommand(editJointsButton)->isPromoted(true);
	// precheckPanel->controls()->addCommand(robotExportGuideButton)->isPromoted(true);
	//precheckPanel->controls()->addCommand(editDOFButton)->isPromoted(true);
	settingsPanel->controls()->addCommand(settingsButton)->isPromoted(true);
	finishPanel->controls()->addCommand(finishButton)->isPromoted(true);
}


void EUI::deleteButtonCommand(Ptr<CommandDefinition>& buttonCommand, ShowPaletteCommandCreatedHandler* buttonHandler)
{
	if (buttonCommand) {
		clearHandler<ShowPaletteCommandCreatedHandler>(buttonCommand, buttonHandler);
		buttonCommand->deleteMe();
		buttonCommand = nullptr;
	}
}

void EUI::deleteButtons()
{
	// Delete button
	Ptr<ToolbarPanelList> panels = UI->allToolbarPanels();
	if (!panels)
		return;

	Ptr<ToolbarPanel> panel = panels->itemById(SynthesisAddIn::PANEL_FINISH);
	if (!panel)
		return;

	Ptr<ToolbarControls> controls = panel->controls();
	if (!controls)
		return;

	Ptr<ToolbarControl> ctrl = controls->itemById(BTN_EXPORT);
	if (ctrl)
		ctrl->deleteMe();

	Ptr<CommandDefinitions> commandDefinitions = UI->commandDefinitions();
	if (!commandDefinitions)
		return;

	// Delete btn commands
	deleteButtonCommand(driveTrainTypeButton, driveTrainTypeShowPaletteCommandCreatedHandler);
	deleteButtonCommand(driveTrainWeightButton, driveTrainWeightShowPaletteCommandCreatedHandler);
	deleteButtonCommand(editJointsButton, editJointsShowPaletteCommandCreatedHandler);
	//deleteButtonCommand(editDOFButton, editDOFShowPaletteCommandCreatedHandler);
	deleteButtonCommand(robotExportGuideButton, robotExportGuideShowPaletteCommandCreatedHandler);
	deleteButtonCommand(settingsButton, settingsShowPaletteCommandCreatedHandler);
	deleteButtonCommand(finishButton, finishShowPaletteCommandCreatedHandler);
}
