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
#include <windows.h>
#include <KnownFolders.h>
#include <ShlObj.h>
#include "CustomHandlers.h"
#include "Identifiers.h"
#include "../Data/BXDJ/ConfigData.h"
#include "../Data/Filesystem.h"
#include <Fusion/Components/JointGeometry.h>

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
		void focusCameraOnGeometry(bool transition, double zoom, Ptr<JointGeometry> geo);
		bool getJointGeometryAndOccurances(std::string jointID, Ptr<JointGeometry>& geo, Ptr<Occurrence>& entity);

		// UI Management
		void prepareAllPalettes(); ///< Creates all palettes
		void hideAllPalettes();

		void openDriveTypePalette();
		void closeDriveTypePalette(std::string data);

		void openJointEditorPalette();///< Loads and opens the robot exporter configuration palette. Disables the export button.
		void closeJointEditorPalette(); ///< Closes the robot exporter configuration palette. Enables the export button.

		/// Loads and opens the sensors configuration palette.
		/// \param sensors Existing sensor configuration to load.
		void openSensorsPalette(std::string sensors);
		/// Closes the sensors configuration palette.
		/// \param sensorsToSave Sensor configuration to send to the robot exporter for saving.
		void closeSensorsPalette(std::string sensorsToSave = "");

		void openGuidePalette(); ///< Loads and opens the robot exporter guide palette.
		void closeGuidePalette(); ///< Loads and opens the robot export guide palette.

		void toggleKeyPalette();

		void openFinishPalette();
		void closeFinishPalette();

		void openProgressPalette(); ///< Opens the progress bar palette and sets the progress to 0.
		void closeProgressPalette(); ///< Closes the progress bar palette.

		// UI Controls
		///
		/// Highlights a joint in Fusion.
		/// \param jointID ID generated from BXDJ::Utility::getUniqueJointID.
		///
		void highlightAndFocusSingleJoint(std::string jointID, bool transition, double zoom);
		void highlightJoint(std::string jointID);
		void resetHighlightAndFocusWholeModel(bool transition, double zoom, Ptr<Camera> ogCam);

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
		void toggleDOF();
		void focusWholeModel(bool transition, double zoom, Ptr<Camera> ogCam);

		bool dofViewEnabled;

	private:
		Ptr<Application> app; ///< Active Fusion application.
		Ptr<UserInterface> UI; ///< Active Fusion user interface.

		Ptr<Workspace> workSpace; ///< Synthesis workspace.
		Ptr<ToolbarPanel> finishPanel; ///< Synthesis control finishPanel.
		Ptr<ToolbarPanel> driveTrainPanel; ///< Synthesis control finishPanel.
		Ptr<ToolbarPanel> jointSetupPanel; ///< Synthesis control finishPanel.
		Ptr<ToolbarPanel> precheckPanel; ///< Synthesis control finishPanel.

		Ptr<Palette> driveTypePalette; ///< Drive train type configuration palette.
		Ptr<Palette> jointEditorPalette; ///< Robot export configuration palette.
		Ptr<Palette> sensorsPalette; ///< Sensor configuration palette.
		Ptr<Palette> guidePalette; ///< Robot export guide palette.
		Ptr<Palette> keyPalette; ///< Degree of Freedom color key dialog
		Ptr<Palette> finishPalette; ///< Robot export configuration palette.
		Ptr<Palette> progressPalette; ///< Progress bar palette.

		Ptr<CommandDefinition> driveTrainTypeButton; ///< Export robot button.
		Ptr<CommandDefinition> driveTrainWeightButton; ///< Export robot button.
		Ptr<CommandDefinition> editJointsButton; ///< Export robot button.
		Ptr<CommandDefinition> editDOFButton; ///< Export robot button.
		Ptr<CommandDefinition> keyDOFButton; ///< Export robot button.
		Ptr<CommandDefinition> robotExportGuideButton; ///< Export robot button.
		Ptr<CommandDefinition> finishButton; ///< Export robot button.

		// Event Handlers
		// These handlers are managed in EUI-Handers.cpp.
		// Pointers to each handlers are kept for removal and deletion
		// when the exporter add-in is deactivated.

		WorkspaceActivatedHandler * workspaceActivatedHandler = nullptr;
		WorkspaceDeactivatedHandler * workspaceDeactivatedHandler = nullptr;

		ShowPaletteCommandCreatedHandler* showPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* driveTrainTypeShowPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* driveTrainWeightShowPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* editJointsShowPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* editDOFShowPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* keyShowPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* robotExportGuideShowPaletteCommandCreatedHandler = nullptr;
		ShowPaletteCommandCreatedHandler* finishShowPaletteCommandCreatedHandler = nullptr;

		ClosePaletteEventHandler* driveTypeClosePaletteHandler = nullptr;
		ClosePaletteEventHandler* jointEditorPaletteHandler = nullptr;
		ClosePaletteEventHandler* jointEditorClosePaletteEventHandler = nullptr;
		ClosePaletteEventHandler* keyClosePaletteEventHandler = nullptr;
		ClosePaletteEventHandler* guideCloseGuideFormEventHandler = nullptr;
		ClosePaletteEventHandler* closeExporterFormEventHandler = nullptr;
		ClosePaletteEventHandler* finishPaletteCloseEventHandler = nullptr;

		ReceiveFormDataHandler* receiveFormDataHandler = nullptr;
		ReceiveFormDataHandler* driveTypeReceiveFormDataHandler = nullptr;
		ReceiveFormDataHandler* jointEditorReceiveFormDataHandler = nullptr;
		ReceiveFormDataHandler* sensorsReceiveFormDataHandler = nullptr;
		ReceiveFormDataHandler* guideReceiveFormDataHandler = nullptr;
		ReceiveFormDataHandler* keyCloseFormDataEventHandler = nullptr;
		ReceiveFormDataHandler* finishPaletteReceiveFormDataHandler = nullptr;

		template<typename E, typename T>
		bool addHandler(Ptr<T> el, E* a);
		
		template<typename E, typename T>
		bool clearHandler(Ptr<T> el, E*);

		// UI Creation/Deletion
		bool createWorkspace(); ///< Creates the Synthesis workspace, finishPanel, and controls.
		void deleteWorkspace(); ///< Deletes the finishPanel and controls.

		bool createDriveTypePalette(); ///< Creates the Drivetrain type configuration palette.
		void deleteDriveTypePalette(); ///< Deletes the Drivetrain type configuration palette.

		bool createJointEditorPalette(); ///< Creates the robot export configuration palette.
		void deleteJointEditorPalette(); ///< Deletes the robot export configuration palette.

		bool createFinishPalette();
		void deleteFinishPalette();

		bool createSensorsPalette(); ///< Creates the sensor configuration palette.
		void deleteSensorsPalette(); ///< Deletes the sensor configuration palette.

		bool createGuidePalette(); ///< Creates the robot export guide configuration palette.
		void deleteGuidePalette(); ///< Deletes the robot export guide configuration palette.

		bool createKeyPalette(); ///< Creates the DOF color key palette
		void deleteKeyPalette(); ///< Deletes the DOF color key palette

		bool createProgressPalette(); ///< Creates the progress bar palette.
		void deleteProgressPalette(); ///< Deletes the progress bar palette.

		void createPanels(); ///< Creates the export robot button.
		void createButtons(); ///< Creates the export robot button.
		void deleteButtonCommand(Ptr<CommandDefinition>& buttonCommand,
		                  ShowPaletteCommandCreatedHandler* buttonHandler);
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
