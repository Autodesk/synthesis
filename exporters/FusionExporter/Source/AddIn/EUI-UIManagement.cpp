#include "EUI.h"
#include "../Exporter.h"

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
		workSpace = UI->workspaces()->itemById(K_WORKSPACE);
		if (!workSpace)
		{
			workSpace = UI->workspaces()->add("DesignProductType", K_WORKSPACE, "Synthesis", "Resources/SynthesisIcons");
			workSpace->tooltip("Export robot models to the Synthesis simulator");

			addHandler<WorkspaceActivatedHandler>(UI);
			addHandler<WorkspaceDeactivatedHandler>(UI);

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
	clearHandler<WorkspaceActivatedHandler>(UI);
	clearHandler<WorkspaceDeactivatedHandler>(UI);

	// Delete palettes
	deleteExportPalette();
	deleteSensorsPalette();
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

void EUI::preparePalettes()
{
	createExportPalette();
	createSensorsPalette();
	createProgressPalette();
}

// Export Palette

bool EUI::createExportPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	exportPalette = palettes->itemById(K_EXPORT_PALETTE);
	if (!exportPalette)
	{
		exportPalette = palettes->add(K_EXPORT_PALETTE, "Robot Exporter Form", "Palette/export.html", false, true, true, 300, 200);
		if (!exportPalette)
			return false;

		exportPalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(exportPalette);
		addHandler<CloseExporterFormEventHandler>(exportPalette);
	}

	return true;
}

void EUI::deleteExportPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette exists
	exportPalette = palettes->itemById(K_EXPORT_PALETTE);

	if (!exportPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(exportPalette);
	clearHandler<CloseExporterFormEventHandler>(exportPalette);

	exportPalette->deleteMe();
	exportPalette = nullptr;
}

void EUI::openExportPalette()
{
	exportButtonCommand->controlDefinition()->isEnabled(false);

	// In some cases, sending info to the HTML of a palette on the same thread causes issues
	static std::thread * uiThread = nullptr;
	if (uiThread != nullptr) { uiThread->join(); delete uiThread; }

	uiThread = new std::thread([this](std::string configJSON)
	{
		exportPalette->sendInfoToHTML("joints", configJSON);
		exportPalette->isVisible(true);
		exportPalette->sendInfoToHTML("joints", configJSON);
	}, Exporter::loadConfiguration(app->activeDocument()).toJSONString());
}

void EUI::closeExportPalette()
{
	exportPalette->isVisible(false);
	sensorsPalette->isVisible(false);
	enableExportButton();
}

// Sensors Palette

bool EUI::createSensorsPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	sensorsPalette = palettes->itemById(K_SENSORS_PALETTE);
	if (!sensorsPalette)
	{
		// Create palette
		sensorsPalette = palettes->add(K_SENSORS_PALETTE, "Sensors", "Palette/sensors.html", false, true, true, 300, 200);
		if (!sensorsPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		sensorsPalette->dockingState(PaletteDockStateRight);

		addHandler<ReceiveFormDataHandler>(sensorsPalette);
	}

	return true;
}

void EUI::deleteSensorsPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette already exists
	sensorsPalette = palettes->itemById(K_SENSORS_PALETTE);

	if (!sensorsPalette)
		return;

	clearHandler<ReceiveFormDataHandler>(sensorsPalette);

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

		uiThread = new std::thread([this](std::string sensors) { exportPalette->sendInfoToHTML("sensors", sensors); }, sensorsToSave);
	}
}

// Progress Palette

bool EUI::createProgressPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	progressPalette = palettes->itemById(K_PROGRESS_PALETTE);
	if (!progressPalette)
	{
		// Create palette
		progressPalette = palettes->add(K_PROGRESS_PALETTE, "Loading", "Palette/progress.html", false, false, false, 150, 150);
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
	progressPalette = palettes->itemById(K_PROGRESS_PALETTE);
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
	enableExportButton();
}

// BUTTONS AND PANELS

void EUI::createPanels()
{
	driveTrainPanel = workSpace->toolbarPanels()->add(K_DRIVE_PANEL, "Drive Train Setup");
	jointSetupPanel = workSpace->toolbarPanels()->add(K_JOINT_PANEL, "Joint Setup");
	precheckPanel = workSpace->toolbarPanels()->add(K_PRECHECK_PANEL, "Robot Setup Checklist");
	finishPanel = workSpace->toolbarPanels()->add(K_FINISH_PANEL, "Finish");
}

void EUI::createButtons()
{
	driveTrainType = UI->commandDefinitions()->addButtonDefinition(K_DT_TYPE, "Drive Train Type", "Setup your robot for exporting to Synthesis.", "Resources/DriveIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(driveTrainType);

	driveTrainWeight = UI->commandDefinitions()->addButtonDefinition(K_DT_WEIGHT, "Drive Train Weight", "Setup your robot for exporting to Synthesis.", "Resources/WeightIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(driveTrainWeight);

	editJointsButton = UI->commandDefinitions()->addButtonDefinition(K_EDIT_JOINTS, "Edit Joints", "Setup your robot for exporting to Synthesis.", "Resources/JointIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(editJointsButton);

	editDOFButton = UI->commandDefinitions()->addButtonDefinition(K_DOF, "Edit Degrees of Freedom", "Setup your robot for exporting to Synthesis.", "Resources/DOFIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(editDOFButton);

	robotExportGuide = UI->commandDefinitions()->addButtonDefinition(K_GUIDE, "Robot Export Guide", "Setup your robot for exporting to Synthesis.", "Resources/PrecheckIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(robotExportGuide);

	exportButtonCommand = UI->commandDefinitions()->addButtonDefinition(K_EXPORT_BUTTON, "Finish Robot Export", "Setup your robot for exporting to Synthesis.", "Resources/FinishIcons");
	addHandler<ShowPaletteCommandCreatedHandler>(exportButtonCommand);


	// Add buttons to finishPanel
	driveTrainPanel->controls()->addCommand(driveTrainType)->isPromoted(true);
	driveTrainPanel->controls()->addCommand(driveTrainWeight)->isPromoted(true);
	jointSetupPanel->controls()->addCommand(editJointsButton)->isPromoted(true);
	precheckPanel->controls()->addCommand(robotExportGuide)->isPromoted(true);
	precheckPanel->controls()->addCommand(editDOFButton)->isPromoted(true);
	finishPanel->controls()->addCommand(exportButtonCommand)->isPromoted(true);
}


void EUI::deleteButtons()
{
	// Delete button
	Ptr<ToolbarPanelList> panels = UI->allToolbarPanels();
	if (!panels)
		return;

	Ptr<ToolbarPanel> panel = panels->itemById(SynthesisAddIn::K_FINISH_PANEL);
	if (!panel)
		return;

	Ptr<ToolbarControls> controls = panel->controls();
	if (!controls)
		return;

	Ptr<ToolbarControl> ctrl = controls->itemById(K_EXPORT_BUTTON);
	if (ctrl)
		ctrl->deleteMe();

	Ptr<CommandDefinitions> commandDefinitions = UI->commandDefinitions();
	if (!commandDefinitions)
		return;

	// Delete command
	exportButtonCommand = commandDefinitions->itemById(K_EXPORT_BUTTON);
	
	if (!exportButtonCommand)
		return;

	clearHandler<ShowPaletteCommandCreatedHandler>(exportButtonCommand);

	exportButtonCommand->deleteMe();
	exportButtonCommand = nullptr;
}

void EUI::enableExportButton()
{
	exportButtonCommand->controlDefinition()->isEnabled(true);
}

void EUI::disableExportButton()
{
	exportButtonCommand->controlDefinition()->isEnabled(false);
}
