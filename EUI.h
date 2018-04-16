//
//  EUI.h
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved. // How did they even generate this wtf
//

#include "Exporter.h"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

#ifndef EUI_h
#define EUI_h

class EUI{
public:
    EUI();
    EUI(Ptr<UserInterface>);
    ~EUI();
    
    
    
private:
    Ptr<UserInterface> _UI;
    Ptr<Workspace> _WorkSpace;
    Ptr<Workspaces> _WorkSpaces;
    bool CreateWorkspace();
    
};


#endif /* EUI_h */
