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

void MyCloseEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs) {
	//close button is clicked
}

void MyHTMLEventHandler::notify(const Ptr<HTMLEventArgs>& eventArgs) {

}

void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs) {
	// Create a palette
	Ptr<UserInterface> _ui = _APP->userInterface();

	Ptr<Palettes> palettes = _ui->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById("myPalette");
	if (!palette) {
		palette = palettes->add("myPalette", "My Palette", "palette.html", true, true, true, 300, 200);
		if (!palette)
			return;

		// Dock the palette to the right side of Fusion window.
		palette->dockingState(PaletteDockStateRight);

		// Add handler to HTMLEvent of the palette
		Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
		if (!htmlEvent)
			return;

		MyHTMLEventHandler * onHTMLEvent_ = new MyHTMLEventHandler;
		onHTMLEvent_->_APP = _APP;

		htmlEvent->add(onHTMLEvent_);

		// Add handler to CloseEvent of the palette
		Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
		if (!closeEvent)
			return;

		MyCloseEventHandler * onClose_ = new MyCloseEventHandler;
		onClose_->_APP = _APP;

		closeEvent->add(onClose_);
	}
	else {
		palette->isVisible(true);
	}
}

void ShowPaletteCommandCreatedHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs) {

}

void SendInfoCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs) {

}
