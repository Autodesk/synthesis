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
		void openExportPalette();
		void deleteExportPalette();
		void closeExportPalette();

		bool createSensorsPalette();
		void openSensorsPalette(std::string sensors);
		void deleteSensorsPalette();
		void closeSensorsPalette();

		bool createProgressPalette();
		void openProgressPalette();
		void deleteProgressPalette();
		void closeProgressPalette();

		bool createExportButton();
		void deleteExportButton();

		void startExportThread(BXDJ::ConfigData &);
		void cancelExportThread();
		void updateProgress(double percent);

	private:
		Ptr<Application> app;
		Ptr<UserInterface> UI;

		Ptr<Workspace> workSpace;
		Ptr<ToolbarPanel> panel;
		Ptr<ToolbarControls> panelControls;

		Ptr<Palette> exportPalette;
		Ptr<Palette> sensorsPalette;
		Ptr<Palette> progressPalette;

		Ptr<CommandDefinition> exportButtonCommand;

		// Event Handlers
		ReceiveFormDataHandler * formDataHandler = nullptr;
		CloseExporterFormEventHandler * closeExporterHandler = nullptr;

		template<typename T>
		bool addEventToPalette(Ptr<Palette>);
		template<typename T>
		bool clearEventFromPalette(Ptr<Palette>);

		// Thread Information
		std::thread * exportThread;
		bool killExportThread;
		void exportRobot(BXDJ::ConfigData);
	};
}
