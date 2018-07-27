//
//  EUI.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#include "EUI.h"

using namespace Synthesis;

EUI::EUI()
{

}

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> app)
{
	this->UI = UI;
	this->app = app;
	createWorkspace();
};

EUI::~EUI()
{

}

bool EUI::createWorkspace()
{
	try
	{
		// Create workspace
		workSpace = UI->workspaces()->add(app->activeProduct()->productType(), K_WORKSPACE, "Synthesis", "Resources/Sample");
		workSpace->tooltip("Export robot models to the Synthesis simulator");
		
		// Create panel
		Ptr<ToolbarPanels> toolbarPanels = workSpace->toolbarPanels();
		panel = workSpace->toolbarPanels()->add(K_PANEL, "Export");
		panelControls = panel->controls();

		// Create palettes
		if (!defineExportPalette())
			throw "Failed to create toolbar buttons.";

		// Create buttons
		if (!defineExportButton())
			throw "Failed to create toolbar buttons.";

		// Add buttons to panel
		panelControls->addCommand(exportButtonCommand)->isPromoted(true);

		// Activate workspace
		workSpace->activate();

		return true;
	}
	catch (std::exception e)
	{
		UI->messageBox("Failed to load Synthesis Exporter add-in.");
		return false;
	}
}

bool EUI::defineExportPalette()
{
	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return false;

	// Check if palette already exists
	exportPalette = palettes->itemById(K_EXPORT_PALETTE);

	if (!exportPalette)
	{
		// Create palette
		exportPalette = palettes->add(K_EXPORT_PALETTE, "Robot Exporter Form", "Palette/debug.html", false, true, true, 300, 200);
		if (!exportPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		exportPalette->dockingState(PaletteDockStateRight);

		// Add handler to HTMLEvent of the palette
		Ptr<HTMLEvent> htmlEvent = exportPalette->incomingFromHTML();
		if (!htmlEvent)
			return false;

		ReceiveFormDataHandler * onHTMLEvent = new ReceiveFormDataHandler;
		onHTMLEvent->joints = &joints;
		onHTMLEvent->app = app;

		htmlEvent->add(onHTMLEvent);

		// Add handler to CloseEvent of the palette
		Ptr<UserInterfaceGeneralEvent> closeEvent = exportPalette->closed();
		if (!closeEvent)
			return false;

		CloseFormEventHandler * onClose = new CloseFormEventHandler;
		onClose->app = app;

		closeEvent->add(onClose);
	}

	return true;
}

bool EUI::defineExportButton()
{
	// Create button command definition
	exportButtonCommand = UI->commandDefinitions()->addButtonDefinition(K_EXPORT_BUTTON, "Export", "Setup your robot for exporting to Synthesis.", "Resources/Sample");

	// Add create and click events to button
	Ptr<CommandCreatedEvent> commandCreatedEvent = exportButtonCommand->commandCreated();
	if (!commandCreatedEvent)
		return false;
	
	ShowPaletteCommandCreatedHandler* commandCreatedEventHandler = new ShowPaletteCommandCreatedHandler;
	commandCreatedEventHandler->joints = &joints;
	commandCreatedEventHandler->app = app;
	
	return commandCreatedEvent->add(commandCreatedEventHandler);
}
