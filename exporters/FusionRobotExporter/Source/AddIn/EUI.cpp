//
//  EUI.cpp
//  FusionSynth
//
//  Created by Madvisitor on 4/16/18.
//  Copyright © 2018 Autodesk. All rights reserved.
//

#define ALLOW_MULTITHREADING

#include "EUI.h"
#include <Fusion/FusionAll.h>
#include "../Exporter.h"
#include "../Data/BXDJ/Utility.h"
#include "Analytics.h"
using namespace SynthesisAddIn;

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> app)
{
	this->UI = UI;
	//UI->messageBox(Filesystem::getCurrentRobotDirectory("test"));
	this->app = app;
	exportRobotThread = nullptr;
	createWorkspace();
};

EUI::~EUI()
{
	cancelExportRobot();
	deleteWorkspace();
}

// UI Controls

void EUI::focusCameraOnGeometry(bool transition, double zoom, Ptr<JointGeometry> geo)
{
	// Set camera view
	Ptr<Camera> ogCam = app->activeViewport()->camera();
	Ptr<Camera> cam = app->activeViewport()->camera();

	Ptr<Point3D> eyeLocation = Point3D::create(geo->origin()->x(), geo->origin()->y(), geo->origin()->z());
	eyeLocation->translateBy(Vector3D::create(0, 100, 0));

	cam->target(geo->origin());
	cam->upVector(Vector3D::create(1, 0, 0));
	cam->eye(eyeLocation);
	cam->isFitView(true);
	cam->isSmoothTransition(false);

	app->activeViewport()->camera(cam);

	// TODO: There must be a better way to find the extents of the robot without calculations
	auto fitCamera = app->activeViewport()->camera(); // This camera is only used to find the extents of the complete robot

	cam->isFitView(false);
	cam->viewExtents(fitCamera->viewExtents() * zoom);

	if (transition) { // If smooth transition, move the camera to the original starting place
		ogCam->isSmoothTransition(false);
		app->activeViewport()->camera(ogCam);
		cam->isSmoothTransition(true);
	}

	app->activeViewport()->camera(cam);
}

bool EUI::getJointGeometryAndOccurances(std::string jointID, Ptr<JointGeometry>& geo, Ptr<Occurrence>& entity)
{
	std::vector<Ptr<Joint>> joints = Exporter::collectJoints(app->activeDocument());

	// Find the joint that was selected
	Ptr<Joint> highlightedJoint = nullptr;

	for (Ptr<Joint> joint : joints)
	{
		if (BXDJ::Utility::getUniqueJointID(joint) == jointID)
		{
			highlightedJoint = joint;
			break;
		}
	}

	if (highlightedJoint != nullptr)
	{
		geo = highlightedJoint->geometryOrOriginOne();
		if (geo == nullptr || geo->origin() == nullptr)
			geo = highlightedJoint->geometryOrOriginTwo();

		if (geo == nullptr || geo->origin() == nullptr)
			return true;

		entity = highlightedJoint->occurrenceOne();
	}
	else
	{
		std::vector<Ptr<AsBuiltJoint>> asBuiltJoints = Exporter::collectAsBuiltJoints(app->activeDocument());

		// Find the as-built joint that was selected
		Ptr<AsBuiltJoint> highlightedAsBuiltJoint = nullptr;

		for (Ptr<AsBuiltJoint> joint : asBuiltJoints)
		{
			if (BXDJ::Utility::getUniqueJointID(joint) == jointID)
			{
				highlightedAsBuiltJoint = joint;
				break;
			}
		}

		// No joint was found, return early
		if (highlightedAsBuiltJoint == nullptr)
			return true;

		geo = highlightedAsBuiltJoint->geometry();

		entity = highlightedAsBuiltJoint->occurrenceOne();
	}
	return false;
}

void EUI::highlightAndFocusSingleJoint(std::string jointID, bool transition, double zoom)
{
	Ptr<JointGeometry> geo;
	Ptr<Occurrence> entity;

	if (getJointGeometryAndOccurances(jointID, geo, entity)) return;

	// dofViewEnabled = true;

	// Highlight the parts of the joint
	UI->activeSelections()->clear();
	UI->activeSelections()->add(entity);

	focusCameraOnGeometry(transition, zoom, geo);
}

void EUI::highlightJoint(std::string jointID)
{
	Ptr<JointGeometry> geo;
	Ptr<Occurrence> entity;

	if (getJointGeometryAndOccurances(jointID, geo, entity)) return;

	UI->activeSelections()->add(entity);
}


//void EUI::toggleDOF()
//{
//	dofViewEnabled = !dofViewEnabled;
//
//	if (dofViewEnabled)
//	{
//		focusWholeModel(true, 1.5, app->activeViewport()->camera());
//
//		auto config = Exporter::loadConfiguration(app->activeDocument());
//
//		for (const auto joint : config.getJoints())
//		{
//			EUI::highlightJoint(joint.first);
//		};
//	} else
//	{
//		UI->activeSelections()->clear();
//		return;
//	}
//}

void EUI::focusWholeModel(bool transition, double zoom, Ptr<Camera> ogCam)
{
	// Set camera view
	Ptr<Camera> cam = app->activeViewport()->camera();

	Ptr<Point3D> eyeLocation = Point3D::create(0, 100, 0); // TODO: Much of this is the same as highlightAndFocusSingleJoint
	Ptr<Point3D> targetLocation = Point3D::create(0, 0, 0);

	cam->target(targetLocation);
	cam->upVector(Vector3D::create(1, 0, 0));
	cam->eye(eyeLocation);

	cam->isFitView(true);
	cam->isSmoothTransition(false);

	app->activeViewport()->camera(cam);

	// TODO: There must be a better way to find the extents of the robot without calculations
	auto fitCamera = app->activeViewport()->camera(); // This camera is only used to find the extents of the complete robot

	auto newCamera = app->activeViewport()->camera();
	newCamera->viewExtents(fitCamera->viewExtents() * zoom);

	if (transition) { // If smooth transition, move the camera to the original starting place
		ogCam->isSmoothTransition(false);
		app->activeViewport()->camera(ogCam);
		newCamera->isSmoothTransition(true);
	}

	cam->isSmoothTransition(transition);
	app->activeViewport()->camera(newCamera);
}

void EUI::resetHighlightAndFocusWholeModel(bool transition, double zoom, Ptr<Camera> ogCam)
{
	UI->activeSelections()->clear();

	focusWholeModel(transition, zoom, ogCam);
}

// Robot Exporting

void EUI::saveConfiguration(std::string jsonConfig)
{
	//BXDJ::ConfigData config;		removed these lines as they were creating a new ConfigData instance
	//
	//Exporter::saveConfiguration(config, app->activeDocument());

	BXDJ::ConfigData config = Exporter::loadConfiguration(app->activeDocument()); // load the current config and apply changes, not generate new one
	config.fromJSONString(jsonConfig);
	Exporter::saveConfiguration(&config, app->activeDocument());

}

void EUI::startExportRobot(bool openSynthesis)
{
	jointEditorPalette->isVisible(false);
	sensorsPalette->isVisible(false);

	BXDJ::ConfigData config = Exporter::loadConfiguration(app->activeDocument());


	//Check if file already exists
	std::string filePath = Filesystem::getCurrentRobotDirectory(config.robotName);

	if (Filesystem::directoryExists(filePath)) {
		DialogResults res = UI->messageBox("Robot Export Already Exists! Overwrite?", "Robot Export", MessageBoxButtonTypes::YesNoButtonType, WarningIconType);
		if (res != DialogYes) {
			cancelExportRobot();
			return;
		}
	}

	
#ifdef ALLOW_MULTITHREADING
	// Wait for all threads to finish
	if (exportRobotThread != nullptr)
	{
		progressPalette->isVisible(true);
		exportRobotThread->join();
		delete exportRobotThread;
	}

	killExportThread = false;
	exportRobotThread = new std::thread(&EUI::exportRobot, this, config, openSynthesis);
#else
	Exporter::exportMeshes(config, app->activeDocument(), [this](double percent)
	{
	}, &killExportThread);
#endif

	// UI->messageBox("Your exported robot can be found at: " + Filesystem::getCurrentRobotDirectory(config.robotName));
}

void EUI::cancelExportRobot()
{
	if (exportRobotThread != nullptr)
	{
		killExportThread = true;
		exportRobotThread->join();
		delete exportRobotThread;
		exportRobotThread = nullptr;
		closeProgressPalette();
	}
}

void EUI::updateProgress(double percent)
{
	if (percent < 0)
		percent = 0;
	
	if (percent > 1) {
		percent = 1;
	}

	progressPalette->sendInfoToHTML("progress", std::to_string(percent));

}

void EUI::exportRobot(BXDJ::ConfigData config, bool openSynthesis)
{
	//UI->messageBox("Robot Exported successfully to: ");
	openProgressPalette();

	try
	{	
		Exporter::exportMeshes(config, app->activeDocument(), [this](double percent)
		{
			updateProgress(percent);
		}, &killExportThread);
		progressPalette->sendInfoToHTML("success", Filesystem::getCurrentRobotDirectory(config.robotName));
		// Add delay before closing so that loading bar has time to animate
		if (!killExportThread)
			std::this_thread::sleep_for(std::chrono::milliseconds(250));

		Analytics::LogEvent(U("Export"), U("Succeeded"));
		if (openSynthesis)
		{
			Analytics::LogEvent(U("System"), U("Launched Synthesis"));
			std::string cs = "start \"\" \"C:/Program Files/Autodesk/Synthesis/Synthesis/Synthesis.exe\" -robot \"" + Filesystem::getCurrentRobotDirectory(config.robotName) + "\"";
			try
			{
				system(cs.c_str());
			}
			catch (const std::exception& e) {}
		}

		std::this_thread::sleep_for(std::chrono::seconds(5));
		//UI->messageBox("Robot Exported successfully to: ");
	}
	catch (const std::exception& e)
	{
		progressPalette->sendInfoToHTML("error", "An error occurred while exporting \"" + config.robotName + "\":<br>" + std::string(e.what()));
		Analytics::LogEvent(U("Export"), U("Failed"));
		std::this_thread::sleep_for(std::chrono::milliseconds(5000));
	}

	//UI->messageBox("Robot Exported successfully to: " + Filesystem::getCurrentRobotDirectory("name"));

	progressPalette->sendInfoToHTML("progress", std::to_string(0));
	closeProgressPalette();
}
