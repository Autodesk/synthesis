#include "CustomHandlers.h"

EUI_Handlers::EUI_Handlers(Ptr<Application> _app) {
	//_commandCreated = new MyCommandCreatedEventHandler();

}

EUI_Handlers::~EUI_Handlers() {

}

void MyCommandCreatedEventHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs) {
	// Code to react to the event.
	_app->userInterface.messageBox("In MyCommandCreatedEventHandler event handler.");
}

