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

EUI::EUI(Ptr<UserInterface> UI) : _UI(UI){
    CreateWorkspace();
};

EUI::~EUI(){
    
}

bool EUI::CreateWorkspace(){
    try {
        _WorkSpace = _UI->workspaces()->add("Fusion Synthesis", "1001", "Synthesis", "Fision");
        _WorkSpace->activate(); // this actually crashes fusion pretty hard regardless of exception handling
        _UI->messageBox("Adding workspace");
        return true;
    } catch (exception e) {
        _UI->messageBox(e.what());
        return false;
    }
}



