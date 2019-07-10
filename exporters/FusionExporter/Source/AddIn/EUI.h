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
#include <experimental/filesystem>
#include <Core/Application/Viewport.h>
#include "CustomHandlers.h"
#include "Identifiers.h"
#include "../Data/BXDJ/ConfigData.h"
#include "../Data/Filesystem.h"

using namespace adsk::core;
using namespace adsk::fusion;

namespace SynthesisAddIn
{
	/// Exporter User Interface: Manages the creation and deletion of user interface elements, such as palettes and buttons.
	class EUI
	{
	public:
		///
		/// Creates the Robot Exporter workspace.
		/// \param UI Fusion User Interface object.
		/// \param app Fusion Application object.
		///
		EUI(Ptr<UserInterface>, Ptr<Application>);
		~EUI(); ///< Cancels any in-progress export and deletes the workspace.

		// UI Management
		void preparePalettes(); ///< Creates all palettes

		void openExportPalette();///< Loads and opens the robot exporter configuration palette. Disables the export button.
		void closeExportPalette(); ///< Closes the robot exporter configuration palette. Enables the export button.

		/// Loads and opens the sensors configuration palette.
		/// \param sensors Existing sensor configuration to load.
		void openSensorsPalette(std::string sensors);
		/// Closes the sensors configuration palette.
		/// \param sensorsToSave Sensor configuration to send to the robot exporter for saving.
		void closeSensorsPalette(std::string sensorsToSave = "");

		void openProgressPalette(); ///< Opens the progress bar palette and sets the progress to 0.
		void closeProgressPalette(); ///< Closes the progress bar palette.

		void enableExportButton(); ///< Enables the export robot button.
		void disableExportButton(); ///< Disables the export robot button.

		// UI Controls
		///
		/// Highlights a joint in Fusion.
		/// \param jointID ID generated from BXDJ::Utility::getUniqueJointID.
		///
		void highlightJoint(std::string jointID, bool transition, double zoom);
		void resetHighlight(bool transition, double zoom, Ptr<Camera> ogCam);

		// Robot Exporting
		///
		/// Saves a configuration string to the open document's property set.
		/// \param jsonConfig Configuration to save.
		///
		void saveConfiguration(std::string jsonConfig);

		void startExportRobot(); ///< Starts a thread for exporting the robot (EUI::exportRobot).
		void cancelExportRobot(); ///< Cancels any export thread.

		///
		/// Updates the progress bar value.
		/// \param percent Current progress (0 to 1).
		///
		void updateProgress(double percent);

	private:
		Ptr<Application> app; ///< Active Fusion application.
		Ptr<UserInterface> UI; ///< Active Fusion user interface.

		Ptr<Workspace> workSpace; ///< Synthesis workspace.
		Ptr<ToolbarPanel> finishPanel; ///< Synthesis control finishPanel.
		Ptr<ToolbarPanel> driveTrainPanel; ///< Synthesis control finishPanel.
		Ptr<ToolbarPanel> jointSetupPanel; ///< Synthesis control finishPanel.
		Ptr<ToolbarPanel> precheckPanel; ///< Synthesis control finishPanel.

		Ptr<Palette> exportPalette; ///< Robot export configuration palette.
		Ptr<Palette> sensorsPalette; ///< Sensor configuration palette.
		Ptr<Palette> progressPalette; ///< Progress bar palette.

		Ptr<CommandDefinition> driveTrainType; ///< Export robot button.
		Ptr<CommandDefinition> driveTrainWeight; ///< Export robot button.
		Ptr<CommandDefinition> editJointsButton; ///< Export robot button.
		Ptr<CommandDefinition> editDOFButton; ///< Export robot button.
		Ptr<CommandDefinition> robotExportGuide; ///< Export robot button.
		Ptr<CommandDefinition> exportButtonCommand; ///< Export robot button.

		// Event Handlers
		// These handlers are managed in EUI-Handers.cpp.
		// Pointers to each handlers are kept for removal and deletion
		// when the exporter add-in is deactivated.

		WorkspaceActivatedHandler * workspaceActivatedHandler = nullptr;
		WorkspaceDeactivatedHandler * workspaceDeactivatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler * showPaletteCommandCreatedHandler = nullptr;
		ReceiveFormDataHandler * receiveFormDataHandler = nullptr;
		CloseExporterFormEventHandler * closeExporterFormEventHandler = nullptr;

		///
		/// Add a handler to a UI element.
		/// \param E Handler to add.
		/// \param el UI element to add handler to.
		///
		template<typename E, typename T>
		bool addHandler(Ptr<T> el);

		///
		/// Removes a handler from a UI element.
		/// \param E Handler to remove.
		/// \param el UI element to remove handler from.
		///
		template<typename E, typename T>
		bool clearHandler(Ptr<T> el);

		// UI Creation/Deletion
		bool createWorkspace(); ///< Creates the Synthesis workspace, finishPanel, and controls.
		void deleteWorkspace(); ///< Deletes the finishPanel and controls.

		bool createExportPalette(); ///< Creates the robot export configuration palette.
		void deleteExportPalette(); ///< Deletes the robot export configuration palette.

		bool createSensorsPalette(); ///< Creates the sensor configuration palette.
		void deleteSensorsPalette(); ///< Deletes the sensor configuration palette.

		bool createProgressPalette(); ///< Creates the progress bar palette.
		void deleteProgressPalette(); ///< Deletes the progress bar palette.

		void createPanels(); ///< Creates the export robot button.
		void createButtons(); ///< Creates the export robot button.
		void deleteButtons(); ///< Deletes the export robot button.

		// Thread Information
		std::thread * exportRobotThread; ///< Pointer to any active robot export thread.
		bool killExportThread; ///< If set to true, the exportRobotThread will stop early.
		
		///
		/// Used with threading to export the robot.
		/// \param config Configuration to export the robot with.
		///
		void exportRobot(BXDJ::ConfigData);
	};
}
