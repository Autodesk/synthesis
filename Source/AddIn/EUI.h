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

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;

namespace Synthesis
{
	class EUI
	{
	public:
		EUI();
		EUI(Ptr<UserInterface>, Ptr<Application>);
		~EUI();

	private:
		Ptr<Application> app;
		Ptr<UserInterface> UI;

		Ptr<Workspace> workSpace;
		Ptr<ToolbarPanel> toolbar;
		Ptr<ToolbarControls> toolbarControls;

		Ptr<CommandDefinition> exportButtonCommand;

		bool createWorkspace();
		bool defineExportButton();

	};
}
