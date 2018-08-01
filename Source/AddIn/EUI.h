//
//  EUI.h
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved. // How did they even generate this wtf
//

#pragma once

#include <thread>
#include <list>
#include "CustomHandlers.h"
#include "Identifiers.h"
#include "../Data/BXDJ/ConfigData.h"

using namespace adsk::core;
using namespace adsk::fusion;

namespace Synthesis
{
	class EUI
	{
	public:
		EUI(Ptr<UserInterface>, Ptr<Application>);
		~EUI();

		bool createWorkspace();
		void deleteWorkspace();

		bool createExportPalette();
		void deleteExportPalette();

		bool createExportButton();
		void deleteExportButton();
		
		void startExportThread(BXDJ::ConfigData &);

	private:
		Ptr<Application> app;
		Ptr<UserInterface> UI;

		Ptr<Workspace> workSpace;
		Ptr<ToolbarPanel> panel;
		Ptr<ToolbarControls> panelControls;

		Ptr<CommandDefinition> exportButtonCommand;
		Ptr<Palette> exportPalette;

		std::list<std::thread *> exportThreads;
		void exportRobot(BXDJ::ConfigData);
	};
}
