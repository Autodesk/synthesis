//
//  EUI.h
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved. // How did they even generate this wtf
//

#pragma once

#include "../Exporter.h"
#include "CustomHandlers.h"
#include "Identifiers.h"

using namespace adsk::core;
using namespace adsk::fusion;

namespace Synthesis
{
	class EUI
	{
	public:
		EUI(Ptr<UserInterface>, Ptr<Application>);
		~EUI();

	private:
		Ptr<Application> app;
		Ptr<UserInterface> UI;

		Ptr<Workspace> workSpace;
		Ptr<ToolbarPanel> panel;
		Ptr<ToolbarControls> panelControls;

		Ptr<CommandDefinition> exportButtonCommand;
		Ptr<Palette> exportPalette;

		bool createWorkspace();
		void deleteWorkspace();

		bool createExportPalette();
		void deleteExportPalette();

		bool createExportButton();
		void deleteExportButton();

	};
}
