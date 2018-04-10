#pragma once

#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>
#include <CAM/CAMAll.h>
#include <string>
#include <Core/UserInterface/ToolbarControls.h>

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;


class EUI {
public:
	EUI();
	~EUI();

	void buttonListener();

private:

};