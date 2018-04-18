//
//  CustomTypes.h
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//
#include "Exporter.h"

#using "\DebugSimulaterFileIO\SimulaterAPI.dll"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

#ifndef CustomTypes_h
#define CustomTypes_h

namespace Synthesis{
    class CommandCreatedEventHandler : public adsk::core::CommandCreatedEventHandler{
        public:
            void notify(const Ptr<CommandCreatedEventArgs>& eventArgs) override;
        private:
    };
    
    class CommandEventHandler : public adsk::core::CommandEventHandler{
    public:
        void notify(const Ptr<CommandEventArgs>& eventArgs) override;
    };
    
    class Button{
    public:
        Button();
        ~Button();
        
    private:
        
    };
}


#endif /* CustomTypes_h */
