//
//  EUI.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#include "EUI.h"

using namespace Synthesis;

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> app)
{
	this->UI = UI;
	this->app = app;
	createWorkspace();
};

EUI::~EUI()
{
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

		workspaceDeactivatedEvent->add(new WorkspaceDeactivatedHandler(UI));

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

void Synthesis::EUI::deleteWorkspace()
{
	// Delete palettes
	deleteExportPalette();

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
		exportPalette = palettes->add(K_EXPORT_PALETTE, "Robot Exporter Form", "Palette/palette.html", false, true, true, 300, 200);
		if (!exportPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		exportPalette->dockingState(PaletteDockStateRight);

		// Add handler to HTMLEvent of the palette
		Ptr<HTMLEvent> htmlEvent = exportPalette->incomingFromHTML();
		if (!htmlEvent)
			return false;

		htmlEvent->add(new ReceiveFormDataHandler(app));

		// Add handler to CloseEvent of the palette
		Ptr<UserInterfaceGeneralEvent> closeEvent = exportPalette->closed();
		if (!closeEvent)
			return false;

		closeEvent->add(new CloseFormEventHandler(app));
	}

	return true;
}

void Synthesis::EUI::deleteExportPalette()
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

void Synthesis::EUI::deleteExportButton()
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
