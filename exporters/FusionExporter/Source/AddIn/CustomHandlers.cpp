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
		eui->createProgressPalette();
	}
}

// Deactivate Workspace Event
void WorkspaceDeactivatedHandler::notify(const Ptr<WorkspaceEventArgs>& eventArgs)
{
	if (eventArgs->workspace()->id() == K_WORKSPACE)
	{
		eui->closeExportPalette();
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
	exec->add(new ShowPaletteCommandExecuteHandler(app));
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
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

	palette->sendInfoToHTML("joints", BXDJ::ConfigData(Exporter::collectJoints(app->activeDocument())).toString());

	palette->isVisible(true);
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
		palette->sendInfoToHTML("joints", BXDJ::ConfigData(Exporter::collectJoints(app->activeDocument())).toString());
	}
	else if (eventArgs->action() == "highlight")
	{
		std::vector<Ptr<Joint>> joints = Exporter::collectJoints(app->activeDocument());

		// Find the joint that was selected
		Ptr<Joint> highlightedJoint = nullptr;
		for (Ptr<Joint> joint : joints)
		{
			if (joint->name() == eventArgs->data())
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
	else if (eventArgs->action() == "export")
	{
		palette->isVisible(false);
		eui->startExportThread(BXDJ::ConfigData(eventArgs->data(), Exporter::collectJoints(app->activeDocument())));
	}
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{}
