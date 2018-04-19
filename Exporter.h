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

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;


class Exporter {
public:
	Exporter(Ptr<Application>);
	Exporter();
	~Exporter();

	void ButtonListener();
	void Test();

	Ptr<CommandDefinition> expCommand();

private:
	Ptr<Application> _app;
	Ptr<UserInterface> _ui;
    
    bool StartUI();
};
