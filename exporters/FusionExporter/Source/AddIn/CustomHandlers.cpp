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
		Ptr<Palettes> palettes = UI->palettes();
		if (!palettes)
			return;

		Ptr<Palette> palette = palettes->itemById(K_EXPORT_PALETTE);
		if (!palette)
			return;

		palette->isVisible(false);

		palette = palettes->itemById(K_PROGRESS_PALETTE);
		if (!palette)
			return;

		palette->isVisible(false);
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

	palette->sendInfoToHTML("joints", Exporter::stringifyJoints(Exporter::collectJoints(app->activeDocument())));

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
		palette->sendInfoToHTML("joints", Exporter::stringifyJoints(Exporter::collectJoints(app->activeDocument())));
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
		std::vector<Ptr<Joint>> joints = Exporter::collectJoints(app->activeDocument());

		BXDJ::ConfigData config;

		// Create config
		std::string dataReceived = eventArgs->data();

		// Get robot name
		int i = 0;
		char nameLength = dataReceived[i++];
		std::string name = "";
		for (int j = 0; j < nameLength && i < dataReceived.length(); j++)
			name += dataReceived[i++];
		config.robotName = name;

		// Get joint data
		for (int j = 0; j < joints.size() && i < dataReceived.length(); j++)
		{
			if ((dataReceived[i++] + ASCII_OFFSET) == 1)
			{
				BXDJ::Driver driver((BXDJ::Driver::Type)(dataReceived[i++] + ASCII_OFFSET));

				driver.portSignal = (BXDJ::Driver::Signal)(dataReceived[i++] + ASCII_OFFSET);
				driver.portA = (int)(dataReceived[i++] - 1 + ASCII_OFFSET);
				driver.portB = (int)(dataReceived[i++] - 1 + ASCII_OFFSET);

				if ((dataReceived[i++] + ASCII_OFFSET) == 1)
				{
					BXDJ::Wheel wheel;
					wheel.type = (BXDJ::Wheel::Type)(dataReceived[i++] + ASCII_OFFSET);
					wheel.frictionLevel = (BXDJ::Wheel::FrictionLevel)(dataReceived[i++] + ASCII_OFFSET);
					wheel.isDriveWheel = (dataReceived[i++] + ASCII_OFFSET) == 1;
					driver.setComponent(wheel);
				}

				if ((dataReceived[i++] + ASCII_OFFSET) == 1)
				{
					BXDJ::Pneumatic pneumatic;
					pneumatic.widthMillimeter = BXDJ::Pneumatic::COMMON_WIDTHS[dataReceived[i++] + ASCII_OFFSET];
					pneumatic.pressurePSI = BXDJ::Pneumatic::COMMON_PRESSURES[dataReceived[i++] + ASCII_OFFSET];
					driver.setComponent(pneumatic);
				}

				config.setDriver(joints[j], driver);
			}
		}

		// Export robot
		palette->isVisible(false);
		eui->startExportThread(config);
	}
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{}
