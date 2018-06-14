#include "CustomHandlers.h"

using namespace Synthesis;

void ExportCommandCreatedEventHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs) {
	// Code to react to the event.
	//_app->userInterface.messageBox("test");
	//_UI->messageBox("Test Export");
	Exporter * e = new Exporter(_APP);
	e->exportCommon();
	//e->~Exporter();
}

void ExportWheelCommandCreatedEventHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs) {
	// Code to react to the event.
	//_app->userInterface.messageBox("test");
	//_UI->messageBox("Test Export");
	Exporter * e = new Exporter(_APP);
	e->exportWheel();
}

