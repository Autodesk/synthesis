#include "EUI.h"
#include "../Exporter.h"
#include "../Data/BXDJ/Utility.h"

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
		workSpace = UI->workspaces()->itemById(WORKSPACE_SYNTHESIS);
		if (!workSpace)
		{
			workSpace = UI->workspaces()->add("DesignProductType", WORKSPACE_SYNTHESIS, "Synthesis", "Resources/FinishIcons");
			workSpace->tooltip("Export robot models to the Synthesis simulator");

			addHandler<WorkspaceActivatedHandler>(UI, workspaceActivatedHandler);
			addHandler<WorkspaceDeactivatedHandler>(UI, workspaceDeactivatedHandler);

			createPanels();
			createButtons();
		}

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
	createGuidePalette();
	createSettingsPalette();
	createFinishPalette();
	createProgressPalette();
}

void EUI::closeAllPalettes()
{
	closeDriveTypePalette("");
	closeDriveWeightPalette("");
	closeJointEditorPalette();
	closeSensorsPalette();
	closeGuidePalette();
	closeSettingsPalette("");
	cancelExportRobot();
}

void EUI::hideAllPalettes()
{
	driveTypePalette->isVisible(false);
	driveWeightPalette->isVisible(false);
	jointEditorPalette->isVisible(false);
	sensorsPalette->isVisible(false);
	guidePalette->isVisible(false);
	// keyPalette->isVisible(false);
	settingsPalette->isVisible(false);
	finishPalette->isVisible(false);
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
		driveWeightPalette = palettes->add(PALETTE_DT_WEIGHT, "Drivetrain Weight", "palette/dt_weight.html", false, true, true, 300, 150);
		if (!driveWeightPalette)
			return false;

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
}

void EUI::closeDriveWeightPalette(std::string weightData) {
	driveWeightPalette->isVisible(false);
	driveTrainWeightButton->controlDefinition()->isEnabled(true);

	if (weightData.length() > 0)
	{
		static std::thread* uiThread = nullptr;
		if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

		//Pass the weight value to the export palette as it store all the export data.
		uiThread = new std::thread([this](std::string weightData) { driveWeightPalette->sendInfoToHTML("dt_weight", weightData); }, weightData);
	}
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
		jointEditorPalette = palettes->add(PALETTE_JOINT_EDITOR, "Joint Editor", "palette/jointEditor.html", false, true, true, 450, 200);
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
	hideAllPalettes();
	editJointsButton->controlDefinition()->isEnabled(false);
	driveTrainTypeButton->controlDefinition()->isEnabled(true);
	robotExportGuideButton->controlDefinition()->isEnabled(true);
	

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

	EUI::resetHighlightAndFocusWholeModel(true, 1.5, ogCam); // clear highlight and move camera to look at whole robot

	uiThread = new std::thread([this](std::string configJSON) // Actually open the palette and send the joint data
	{
		jointEditorPalette->sendInfoToHTML("joints", configJSON); // TODO: Why is this duplicated
		jointEditorPalette->isVisible(true);
		jointEditorPalette->sendInfoToHTML("joints", configJSON);
	}, config.toJSONString());
}


void EUI::closeJointEditorPalette()
{
	jointEditorPalette->isVisible(false);
	sensorsPalette->isVisible(false);
	editJointsButton->controlDefinition()->isEnabled(true);
	robotExportGuideButton->controlDefinition()->isEnabled(true);
	finishButton->controlDefinition()->isEnabled(true);
	
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
		guidePalette = palettes->add(PALETTE_GUIDE, "Robot Export Guide", "palette/guide.html", false, true, true, 400, 200);
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
	if (!guideEnabled) {
		robotExportGuideButton->controlDefinition()->isEnabled(false);
		guidePalette->isVisible(true);
		guideEnabled = true;
	}
}

void EUI::closeGuidePalette()
{
	if (guideEnabled)
	{

		robotExportGuideButton->controlDefinition()->isEnabled(true);
		guidePalette->isVisible(false);
		guideEnabled = false;

		static std::thread* uiThread = nullptr;
		if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

		uiThread = new std::thread([this]()
		{
			settingsPalette->sendInfoToHTML("settings_guide", guideEnabled ? "true" : "false");
			settingsPalette->sendInfoToHTML("settings_guide", guideEnabled ? "true" : "false");
		});

		//settingsPalette->sendInfoToHTML("settings_guide", guideEnabled ? "true" : "false");
	}
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

		finishPalette = palettes->add(PALETTE_FINISH, "Robot Exporter Form", "palette/export.html", false, true, true, 300, 200);
		if (!finishPalette)
			return false;

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
}

void EUI::closeFinishPalette()
{
	finishButton->controlDefinition()->isEnabled(true);
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
		sensorsPalette = palettes->add(PALETTE_SENSORS, "Advanced", "palette/sensors.html", false, true, true, 300, 200);
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
	static std::thread * uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this](std::string sensors)
	{
		sensorsPalette->sendInfoToHTML("sensors", sensors);
		sensorsPalette->isVisible(true);
		sensorsPalette->sendInfoToHTML("sensors", sensors);
	}, sensors);
}

void EUI::closeSensorsPalette(std::string sensorsToSave)
{
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
		driveTypePalette = palettes->add(PALETTE_DT_TYPE, "Drivetrain Type", "palette/drivetrain.html", false, true, false, 350, 200);
		if (!driveTypePalette)
			return false;

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
	
	driveTrainTypeButton->controlDefinition()->isEnabled(false);

	// In some cases, sending info to the HTML of a palette on the same thread causes issues
	static std::thread* uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this](std::string configJSON)
		{
			driveTypePalette->sendInfoToHTML("state", configJSON);
			driveTypePalette->isVisible(true);
			driveTypePalette->sendInfoToHTML("state", configJSON);
		}, Exporter::loadConfiguration(app->activeDocument()).toJSONString());
}

void EUI::closeDriveTypePalette(std::string driveTypeData) {

	driveTrainTypeButton->controlDefinition()->isEnabled(true);
	driveTypePalette->isVisible(false);

	BXDJ::ConfigData config = Exporter::loadConfiguration(app->activeDocument());
	config.setDriveType(driveTypeData);
	Exporter::saveConfiguration(config, app->activeDocument());

	if (driveTypeData.length() > 0)
	{
		static std::thread* uiThread = nullptr;
		if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

		// Pass the weight value to the export palette as it store all the export data.
		uiThread = new std::thread([this](std::string driveTypeData) { driveTypePalette->sendInfoToHTML("drivetrain_type", driveTypeData); }, driveTypeData);
	}
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
		progressPalette = palettes->add(PALETTE_PROGRESS, "Loading", "palette/progress.html", false, false, false, 150, 150);
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
		settingsPalette = palettes->add(PALETTE_SETTINGS, "Add-In Settings", "palette/settings.html", false, true, true, 350, 200);
		if (!settingsPalette)
			return false;

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

void EUI::openSettingsPalette(bool nan)
{
	static std::thread* uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this]()
		{
			settingsPalette->sendInfoToHTML("settings_guide", guideEnabled ? "true" : "false");
			settingsPalette->isVisible(true);
			settingsPalette->sendInfoToHTML("settings_guide", guideEnabled ? "true" : "false");
		});

	settingsButton->controlDefinition()->isEnabled(false);
}

void EUI::closeSettingsPalette(std::string guideEnabled) {

	settingsButton->controlDefinition()->isEnabled(true);
	settingsPalette->isVisible(false);

	(guideEnabled == "true") ? openGuidePalette() : closeGuidePalette();

	if (guideEnabled.length() > 0)
	{
		static std::thread* uiThread = nullptr;
		if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

		// Pass the weight value to the export palette as it store all the export data.
		uiThread = new std::thread([this](std::string isGuideEnabled) { settingsPalette->sendInfoToHTML("settings_guide", isGuideEnabled); }, guideEnabled);
	}
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
	driveTrainTypeButton = UI->commandDefinitions()->addButtonDefinition(BTN_DT_TYPE, "Drive Train Type", "Select your drivetrain type (tank, H-drive, or other).", "Resources/DriveIcons");
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
