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
		workSpace = UI->workspaces()->add(app->activeProduct()->productType(), "SynthesisExporterAddIn", "Synthesis", "Resources/Sample");
		workSpace->tooltip("Export robot models to the Synthesis simulator");
		
		// Create toolbar
		Ptr<ToolbarPanels> toolbarPanels = workSpace->toolbarPanels();
		toolbar = workSpace->toolbarPanels()->add("SynthesisExport", "Export");
		toolbarControls = toolbar->controls();

		// Create palettes
		if (!defineExportPalette())
			throw "Failed to create toolbar buttons.";

		// Create buttons
		if (!defineExportButton())
			throw "Failed to create toolbar buttons.";

		// Add buttons to toolbar
		toolbarControls->addCommand(exportButtonCommand)->isPromoted(true);
		
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
	exportPalette = palettes->itemById("exporterForm");

	if (!exportPalette)
	{
		// Create palette
		exportPalette = palettes->add("exporterForm", "Robot Exporter Form", "Palette/palette.html", false, true, true, 300, 200);
		if (!exportPalette)
			return false;

		// Dock the palette to the right side of Fusion window.
		exportPalette->dockingState(PaletteDockStateRight);

		// Add handler to HTMLEvent of the palette
		Ptr<HTMLEvent> htmlEvent = exportPalette->incomingFromHTML();
		if (!htmlEvent)
			return false;

		ReceiveFormDataHandler * onHTMLEvent = new ReceiveFormDataHandler;
		onHTMLEvent->app = app;
		onHTMLEvent->palette = exportPalette;

		htmlEvent->add(onHTMLEvent);

		// Add handler to CloseEvent of the palette
		Ptr<UserInterfaceGeneralEvent> closeEvent = exportPalette->closed();
		if (!closeEvent)
			return false;

		CloseFormEventHandler * onClose = new CloseFormEventHandler;
		onClose->app = app;
		onClose->palette = exportPalette;

		closeEvent->add(onClose);
	}

	return true;
}

bool EUI::defineExportButton()
{
	// Create button command definition
	exportButtonCommand = UI->commandDefinitions()->addButtonDefinition("ExportRobotButton", "Export", "Setup your robot for exporting to Synthesis.", "Resources/Sample");

	// Add create and click events to button
	Ptr<CommandCreatedEvent> commandCreatedEvent = exportButtonCommand->commandCreated();
	if (!commandCreatedEvent)
		return false;
	
	ShowPaletteCommandCreatedHandler* commandCreatedEventHandler = new ShowPaletteCommandCreatedHandler;
	commandCreatedEventHandler->palette = exportPalette;
	
	return commandCreatedEvent->add(commandCreatedEventHandler);
}
