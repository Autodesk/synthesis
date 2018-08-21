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

using namespace SynthesisAddIn;

EUI::EUI(Ptr<UserInterface> UI, Ptr<Application> app)
{
	this->UI = UI;
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

void EUI::highlightJoint(std::string jointID)
{
	std::vector<Ptr<Joint>> joints = Exporter::collectJoints(app->activeDocument());

	Ptr<JointGeometry> geo;

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
			return;

		// Highlight the parts of the joint
		UI->activeSelections()->clear();
		UI->activeSelections()->add(highlightedJoint->occurrenceOne());
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
			return;

		geo = highlightedAsBuiltJoint->geometry();

		// Highlight the parts of the joint
		UI->activeSelections()->clear();
		UI->activeSelections()->add(highlightedAsBuiltJoint->occurrenceOne());
	}

	// Set camera view
	Ptr<Camera> cam = app->activeViewport()->camera();

	Ptr<Point3D> eyeLocation = Point3D::create(geo->origin()->x(), geo->origin()->y(), geo->origin()->z());
	eyeLocation->translateBy(Vector3D::create(0, 100, 0));

	cam->target(geo->origin());
	cam->upVector(Vector3D::create(1, 0, 0));
	cam->eye(eyeLocation);

	cam->isSmoothTransition(true);
	app->activeViewport()->camera(cam);
}

// Robot Exporting

void EUI::saveConfiguration(std::string jsonConfig)
{
	BXDJ::ConfigData config;
	config.fromJSONString(jsonConfig);
	Exporter::saveConfiguration(config, app->activeDocument());
}

void EUI::startExportRobot()
{
	exportPalette->isVisible(false);
	sensorsPalette->isVisible(false);

#ifdef ALLOW_MULTITHREADING
	// Wait for all threads to finish
	if (exportRobotThread != nullptr)
	{
		progressPalette->isVisible(true);
		exportRobotThread->join();
		delete exportRobotThread;
	}

	killExportThread = false;
	exportRobotThread = new std::thread(&EUI::exportRobot, this, Exporter::loadConfiguration(app->activeDocument()));
#else
	Exporter::exportMeshes(config, app->activeDocument(), [this](double percent)
	{
		//updateProgress(percent);
	}, &killExportThread);
#endif
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
	
	if (percent > 1)
		percent = 1;

	progressPalette->sendInfoToHTML("progress", std::to_string(percent));
}

void EUI::exportRobot(BXDJ::ConfigData config)
{
	openProgressPalette();

	try
	{
		Exporter::exportMeshes(config, app->activeDocument(), [this](double percent)
		{
			updateProgress(percent);
		}, &killExportThread);

		// Add delay before closing so that loading bar has time to animate
		if (!killExportThread)
			std::this_thread::sleep_for(std::chrono::milliseconds(250));
	}
	catch (const std::exception& e)
	{
		progressPalette->sendInfoToHTML("error", "An error occurred while exporting \"" + config.robotName + "\":<br>" + std::string(e.what()));
		std::this_thread::sleep_for(std::chrono::milliseconds(5000));
	}
	
	closeProgressPalette();
}
