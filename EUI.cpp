//
//  EUI.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#include "EUI.h"

EUI::EUI(){
    
}

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> APP) : _UI(UI) , _APP(APP){
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
        _WorkSpace = _UI->workspaces()->add(_APP->supportedProductTypes()[0], "1001", "Synthesis", "Resources");
        
        _WorkSpace->tooltip("Workspace for exporting fusion robot files");
        Ptr<ToolbarPanels> toolbarPanels = _WorkSpace->toolbarPanels();
        _ToolbarPanel = _WorkSpace->toolbarPanels()->add("1001" , "ExportingToolbar");
        _ToolbarControls = _ToolbarPanel->controls();
        //_ToolbarControls->addSeparator();
        _WorkSpace->activate();
        _UI->messageBox("Adding workspace");
        return true;
    } catch (exception e) {
        _UI->messageBox(e.what());
        return false;
    }
}



