//
//  CustomTypes.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#include "CustomTypes.h"

using namespace Synthesis;

void Synthesis::CommandCreatedEventHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs){
    //code react
}

void Synthesis::CommandEventHandler::notify(const Ptr<CommandEventArgs> &eventArgs){
    //code react
}

Button::Button(){
	BXDAMesh bIO;
}
