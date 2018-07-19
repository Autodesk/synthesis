//
//  EUI.h
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved. // How did they even generate this wtf
//

#pragma once

#include "Exporter.h"
#include "CustomHandlers.h"

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

namespace Synthesis
{
	class EUI
	{
	public:
		EUI();
		EUI(Ptr<UserInterface>, Ptr<Application>);
		~EUI();

	private:
		Ptr<Application> _APP;
		Ptr<UserInterface> _UI;
		Ptr<Workspace> _WorkSpace;
		Ptr<Workspaces> _WorkSpaces;
		Ptr<ToolbarPanel> _ToolbarPanelExport;
		Ptr<ToolbarPanel> _ToolbarPanelLoad;
		Ptr<ToolbarPanel> _ToolbarPanelHelp;

		Ptr<ToolbarControls> _ToolbarControlsExport;
		Ptr<ToolbarControls> _ToolbarControlsLoad;
		Ptr<ToolbarControls> _ToolbarControlsHelp;

		Ptr<CommandDefinition> _ExportCommandDefQuick;
		Ptr<CommandDefinition> _ExportCommandDef;
		Ptr<CommandDefinition> _ExportCommandLoad;
		Ptr<CommandDefinition> _ExportCommandHelp;

		void configButtonQuickExport();
		void configButtonExport();
		void configButtonLoad();
		void configButtonHelp();
		bool CreateWorkspace();
		void configPalette();
	};
}
