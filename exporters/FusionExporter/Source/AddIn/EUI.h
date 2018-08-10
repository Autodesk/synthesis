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

namespace SynthesisAddIn
{
	class EUI
	{
	public:
		EUI(Ptr<UserInterface>, Ptr<Application>);
		~EUI();

		// UI Management
		void preparePalettes();

		void openExportPalette();
		void closeExportPalette();

		void openSensorsPalette(std::string sensors);
		void closeSensorsPalette(std::string sensorsToSave = "");

		void openProgressPalette();
		void closeProgressPalette();

		void enableExportButton();
		void disableExportButton();

		// UI Controls
		void highlightJoint(std::string jointID);

		// Robot Exporting
		void saveConfiguration(std::string jsonConfig);

		void startExportRobot();
		void cancelExportRobot();
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
		WorkspaceActivatedHandler * workspaceActivatedHandler = nullptr;
		WorkspaceDeactivatedHandler * workspaceDeactivatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler * showPaletteCommandCreatedHandler = nullptr;
		ReceiveFormDataHandler * receiveFormDataHandler = nullptr;
		CloseExporterFormEventHandler * closeExporterFormEventHandler = nullptr;

		template<typename E, typename T>
		bool addHandler(Ptr<T>);
		template<typename E, typename T>
		bool clearHandler(Ptr<T>);

		// UI Creation/Deletion
		bool createWorkspace();
		void deleteWorkspace();

		bool createExportPalette();
		void deleteExportPalette();

		bool createSensorsPalette();
		void deleteSensorsPalette();

		bool createProgressPalette();
		void deleteProgressPalette();

		bool createExportButton();
		void deleteExportButton();

		// Thread Information
		std::thread * exportRobotThread;
		bool killExportThread;
		void exportRobot(BXDJ::ConfigData);
	};
}
