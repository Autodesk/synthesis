#pragma once
#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>
#include <CAM/CAMAll.h>
#include <string>
#include <Core/UserInterface/ToolbarControls.h>
#include <Core/UserInterface/Command.h>
#include <Core/UserInterface/CommandEvent.h>
#include <Core/UserInterface/CommandEventHandler.h>
#include <Core/UserInterface/CommandEventArgs.h>
#include "EUI.h"
#include "BXDA.h"
#include "BinaryRWObject.h"

//#using "SimulatorAPI.dll"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;
using namespace BXDATA;



namespace Synthesis {

	enum logLevels { info, warn, critikal };

    class Exporter {
    public:
        Exporter(Ptr<Application>);
        ~Exporter();

		int exportCommon();
        int exportWheel();

		void writeToFile(string a, logLevels lvl); 

    private:
        Ptr<Application> _app;
        Ptr<UserInterface> _ui;

    };
}
