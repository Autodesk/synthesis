#include "CustomHandlers.h"
#include "Identifiers.h"
#include "EUI.h"
#include "../Exporter.h"
#include "../Data/Filesystem.h"
#include "../Data/BXDJ/Utility.h"
#include "../Data/BXDJ/Driver.h"
#include "../Data/BXDJ/Components.h"

using namespace Synthesis;

/// Workspace Events
// Activate Workspace Event
void WorkspaceActivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == K_WORKSPACE)
	{
		eui->createExportPalette();
		eui->createSensorsPalette();
		eui->createProgressPalette();
	}
}

// Deactivate Workspace Event
void WorkspaceDeactivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == K_WORKSPACE)
	{
		eui->closeExportPalette();
		eui->closeSensorsPalette();
		eui->cancelExportThread();
	}
}

/// Button Events
// Create Palette Button Event
void ShowPaletteCommandCreatedHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	Ptr<Command> command = eventArgs->command();
	if (!command)
		return;

	Ptr<CommandEvent> exec = command->execute();
	if (!exec)
		return;

	// Add click command to button
	exec->add(new ShowPaletteCommandExecuteHandler(eui));
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	eui->openExportPalette();
}

/// Palette Events
// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{
	Ptr<UserInterface> UI = app->userInterface();
	if (!UI)
		return;

	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById(K_EXPORT_PALETTE);
	if (!palette)
		return;

	if (eventArgs->action() == "send_joints")
	{
		palette->sendInfoToHTML("joints", Exporter::loadConfiguration(app->activeDocument()).toJSONString());
	}
	else if (eventArgs->action() == "highlight")
	{
		std::vector<Ptr<Joint>> joints = Exporter::collectJoints(app->activeDocument());

		// Find the joint that was selected
		Ptr<Joint> highlightedJoint = nullptr;
		for (Ptr<Joint> joint : joints)
		{
			if (BXDJ::Utility::getUniqueJointID(joint) == eventArgs->data())
			{
				highlightedJoint = joint;
				break;
			}
		}

		if (highlightedJoint == nullptr)
			return;

		// Highlight the parts of the joint
		UI->activeSelections()->clear();
		UI->activeSelections()->add(BXDJ::Utility::lowerOccurrence(highlightedJoint));

		// Set camera view
		Ptr<Camera> cam = app->activeViewport()->camera();

		Ptr<JointGeometry> geo = highlightedJoint->geometryOrOriginOne();
		Ptr<Point3D> eyeLocation = Point3D::create(geo->origin()->x(), geo->origin()->y(), geo->origin()->z());
		eyeLocation->translateBy(Vector3D::create(0, 100, 0));

		cam->target(geo->origin());
		cam->upVector(Vector3D::create(1, 0, 0));
		cam->eye(eyeLocation);

		cam->isSmoothTransition(true);
		app->activeViewport()->camera(cam);
	}
	else if (eventArgs->action() == "edit_sensors")
	{
		if (thread != nullptr)
		{
			thread->join();
			delete thread;
		}

		thread = new std::thread([](std::string data, EUI * eui)
		{
			eui->openSensorsPalette(data);
		}, eventArgs->data(), eui);
	}
	else if (eventArgs->action() == "save_sensors")
	{
		if (thread != nullptr)
		{
			thread->join();
			delete thread;
		}

		thread = new std::thread([](std::string data, Ptr<Palette> palette, EUI * eui)
		{
			eui->closeSensorsPalette();
			palette->sendInfoToHTML("sensors", data);
		}, eventArgs->data(), palette, eui);
	}
	else if (eventArgs->action() == "save")
	{
		BXDJ::ConfigData config;
		config.fromJSONString(eventArgs->data());
		Exporter::saveConfiguration(config, app->activeDocument());
	}
	else if (eventArgs->action() == "export")
	{
		palette->isVisible(false);
		BXDJ::ConfigData config;
		config.fromJSONString(eventArgs->data());
		Exporter::saveConfiguration(config, app->activeDocument());
		eui->startExportThread(config);
	}
}

// Close Exporter Form Event
void CloseExporterFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{
	eui->closeExportPalette();
}
