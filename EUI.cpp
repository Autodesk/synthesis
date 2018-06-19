//
//  EUI.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#include "EUI.h"


using namespace Synthesis;


EUI::EUI(){
    
}

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> APP){
	_UI = UI;
	_APP = APP;
    CreateWorkspace();
};

EUI::~EUI(){
    
}

bool EUI::CreateWorkspace(){
    try {
        /// Creates a new workspace for a specific product.
        /// productType : The name of the product this workspace will be displayed with. You can obtain a list of the available products by using the supportedProductTypes property of the Application object.
        /// id : The unique ID for this workspace. It must be unique with respect to all other workspaces.
        /// name : The displayed name of this workspace.
        /// resourceFolder : The resource folder should contain two files; 49X31.png and 98x62.png. The larger is used for the Apple Retina display.
        /// Returns the created workspace or null if the creation failed.
        //inline Ptr<Workspace> Workspaces::add(const std::string& productType, const std::string& id, const std::string& name, const std::string& resourceFolder)
        
        //Need to add check if workspace already exists and get reference to it
        
        //(_UI->workspaces()->itemById("BXD:Synthesis").get() != NULL) ? (_WorkSpace = _UI->workspaces()->add(_APP->supportedProductTypes()[0], "BXD:Synthesis", "Synthesis", "Resources")) : (_WorkSpace = _UI->workspaces()->itemById("BXD:Synthesis"));
        
        
        //GUID = sha256(Synthesis:workspace) = 58D6985C9E7C0A2D731CC2141775F86F163FBA96CA871E8EC2840DF1FBEA2C0D
        _WorkSpace = _UI->workspaces()->add(_APP->activeProduct()->productType(), "10001", "Synthesis", "./Resources");
        
        _WorkSpace->tooltip("Workspace for exporting fusion robot files");
        Ptr<ToolbarPanels> toolbarPanels = _WorkSpace->toolbarPanels();
        _ToolbarPanel = _WorkSpace->toolbarPanels()->add("10002" , "ExportingToolbar");
        _ToolbarControls = _ToolbarPanel->controls();
        configButtonWheel();
        configButtonExporter();
        _ToolbarControls->addCommand(_AddWheelCommandDef)->isPromoted(true);
        _ToolbarControls->addSeparator();
        _ToolbarControls->addCommand(_ExportCommandDef)->isPromoted(true);
        //_ToolbarControls->addSeparator();
        _WorkSpace->activate();

		//Write messagebox stuff here


        return true;
    } catch (exception e) {
		//string * lastError;
        //_UI->messageBox(*lastError);
        return false;
    }
}

void EUI::configButtonWheel(){
    _AddWheelCommandDef = _UI->commandDefinitions()->addButtonDefinition("AddWheelButtonDefinition", "WheelExport", "Wheel Config");
    _AddWheelCommandDef->resourceFolder("./Resources");

	Ptr<CommandCreatedEvent> commandCreatedEvent = _AddWheelCommandDef->commandCreated();
	if (!commandCreatedEvent)
		return;	//fix this
	ExportCommandCreatedEventHandler * _commandCreatedEvent = new ExportCommandCreatedEventHandler;
	_commandCreatedEvent->_APP = _APP;	//copy references
	bool isOk = commandCreatedEvent->add(_commandCreatedEvent);
	if (!isOk)
		return;
}

void EUI::configButtonExporter(){
    _ExportCommandDef = _UI->commandDefinitions()->addButtonDefinition("AddExportButtonDefinition", "Export", "This will start the exporting process which will take approximately 5-10 minutes");
    _ExportCommandDef->resourceFolder("Resources");

	Ptr<CommandCreatedEvent> commandCreatedEvent = _ExportCommandDef->commandCreated();
	if (!commandCreatedEvent)
		return;	//fix this
	ExportWheelCommandCreatedEventHandler * _commandCreatedEvent = new ExportWheelCommandCreatedEventHandler;
	_commandCreatedEvent->_APP = _APP;	//copy references
	bool isOk = commandCreatedEvent->add(_commandCreatedEvent);
	if (!isOk)
		return;
}


