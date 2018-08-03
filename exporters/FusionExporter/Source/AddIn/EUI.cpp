//
//  EUI.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#define ALLOW_MULTITHREADING

#include "EUI.h"
#include "../Exporter.h"

using namespace Synthesis;

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> app)
{
	this->UI = UI;
	this->app = app;
	exportThread = nullptr;
	createWorkspace();
};

EUI::~EUI()
{
	cancelExportThread();
	deleteWorkspace();
}

bool EUI::createWorkspace()
{
	try
	{
		// Create workspace
		workSpace = UI->workspaces()->itemById(K_WORKSPACE);
		if (!workSpace)
		{
			workSpace = UI->workspaces()->add("DesignProductType", K_WORKSPACE, "Synthesis", "Resources/Sample");
			workSpace->tooltip("Export robot models to the Synthesis simulator");
		}
		
		// Create workspace events
		Ptr<WorkspaceEvent> workspaceActivatedEvent = UI->workspaceActivated();
		if (!workspaceActivatedEvent)
			throw "Failed to create workspace events.";

		workspaceActivatedEvent->add(new WorkspaceActivatedHandler(this));

		Ptr<WorkspaceEvent> workspaceDeactivatedEvent = UI->workspaceDeactivated();
		if (!workspaceDeactivatedEvent)
			throw "Failed to create workspace events.";

		workspaceDeactivatedEvent->add(new WorkspaceDeactivatedHandler(this));

		// Create panel
		Ptr<ToolbarPanels> toolbarPanels = workSpace->toolbarPanels();
		panel = workSpace->toolbarPanels()->itemById(K_PANEL);
		if (!panel)
			panel = workSpace->toolbarPanels()->add(K_PANEL, "Export");

		panelControls = panel->controls();

		// Create buttons
		if (!createExportButton())
			throw "Failed to create toolbar buttons.";

		// Add buttons to panel
		if (!panelControls->itemById(K_EXPORT_BUTTON))
			panelControls->addCommand(exportButtonCommand)->isPromoted(true);

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
	// Delete palettes
	deleteExportPalette();
	deleteProgressPalette();

	// Delete buttons
	deleteExportButton();
}

bool EUI::createExportPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	exportPalette = palettes->itemById(K_EXPORT_PALETTE);
	if (!exportPalette)
	{
		// Create palette
		exportPalette = palettes->add(K_EXPORT_PALETTE, "Robot Exporter Form", "Palette/export.html", false, true, true, 300, 200);
		if (!exportPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		exportPalette->dockingState(PaletteDockStateRight);

		// Add handler to HTMLEvent of the palette
		Ptr<HTMLEvent> htmlEvent = exportPalette->incomingFromHTML();
		if (!htmlEvent)
			return false;

		htmlEvent->add(new ReceiveFormDataHandler(app, this));

		// Add handler to CloseEvent of the palette
		Ptr<UserInterfaceGeneralEvent> closeEvent = exportPalette->closed();
		if (!closeEvent)
			return false;

		closeEvent->add(new CloseFormEventHandler(app));
	}

	return true;
}

void EUI::deleteExportPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Check if palette already exists
	exportPalette = palettes->itemById(K_EXPORT_PALETTE);
	
	if (exportPalette)
		exportPalette->deleteMe();

	exportPalette = nullptr;
}

void EUI::closeExportPalette()
{
	exportPalette->isVisible(false);
}

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

	if (progressPalette)
		progressPalette->deleteMe();

	progressPalette = nullptr;
}

void EUI::closeProgressPalette()
{
	progressPalette->isVisible(false);
}

bool EUI::createExportButton()
{
	// Create button command definition
	exportButtonCommand = UI->commandDefinitions()->itemById(K_EXPORT_BUTTON);
	if (!exportButtonCommand)
	{
		exportButtonCommand = UI->commandDefinitions()->addButtonDefinition(K_EXPORT_BUTTON, "Export", "Setup your robot for exporting to Synthesis.", "Resources/Sample");

		// Add create and click events to button
		Ptr<CommandCreatedEvent> commandCreatedEvent = exportButtonCommand->commandCreated();
		if (!commandCreatedEvent)
			return false;

		return commandCreatedEvent->add(new ShowPaletteCommandCreatedHandler(app));
	}
	
	return true;
}

void EUI::deleteExportButton()
{
	// Delete button
	Ptr<ToolbarPanelList> panels = UI->allToolbarPanels();
	if (!panels)
		return;

	Ptr<ToolbarPanel> panel = panels->itemById(Synthesis::K_PANEL);
	if (!panel)
		return;

	Ptr<ToolbarControls> controls = panel->controls();
	if (!controls)
		return;

	Ptr<ToolbarControl> ctrl = controls->itemById(K_EXPORT_BUTTON);
	if (ctrl)
		ctrl->deleteMe();

	// Delete command
	Ptr<CommandDefinitions> commandDefinitions = UI->commandDefinitions();
	if (!commandDefinitions)
		return;

	exportButtonCommand = commandDefinitions->itemById(K_EXPORT_BUTTON);
	if (exportButtonCommand)
		exportButtonCommand->deleteMe();

	exportButtonCommand = nullptr;
}

void EUI::startExportThread(BXDJ::ConfigData & config)
{
#ifdef ALLOW_MULTITHREADING
	// Wait for all threads to finish
	if (exportThread != nullptr)
	{
		progressPalette->isVisible(true);
		exportThread->join();
		delete exportThread;
	}

	killExportThread = false;
	exportThread = new std::thread(&EUI::exportRobot, this, config);
#else
	Exporter::exportMeshes(config, app->activeDocument());
#endif
}

void EUI::cancelExportThread()
{
	if (exportThread != nullptr)
	{
		killExportThread = true;
		exportThread->join();
		delete exportThread;
		exportThread = nullptr;
		closeProgressPalette();
	}
}

void EUI::updateProgress(double percent)
{
	if (percent < 0)
		percent = 0;
	
	if (percent > 1)
		percent = 1;

	progressPalette->sendInfoToHTML("progress", std::to_string(percent));
	progressPalette->isVisible(true);
}

void EUI::exportRobot(BXDJ::ConfigData config)
{
	try
	{
		updateProgress(0);

		// Add delay so that loading bar has time to animate
		std::this_thread::sleep_for(std::chrono::milliseconds(250));

		Exporter::exportMeshes(config, app->activeDocument(), [this](double percent)
		{
			updateProgress(percent);
		}, &killExportThread);

		// Add delay so that loading bar has time to animate
		std::this_thread::sleep_for(std::chrono::milliseconds(250));
	}
	catch (const std::exception& e)
	{
		UI->messageBox("An error occurred while exporting \"" + config.robotName + "\"\n" + std::string(e.what()));
	}
	
	closeProgressPalette();
}
