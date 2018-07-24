#include "CustomHandlers.h"

using namespace Synthesis;

void ExportCommandCreatedEventHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	// Code to react to the event.
	//_app->userInterface.messageBox("test");
	//_UI->messageBox("Test Export");
	Exporter * e = new Exporter(_APP);
	e->exportExampleXml();
	delete e;
	//e->~Exporter();
}

void ExportWheelCommandCreatedEventHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	// Code to react to the event.
	//_app->userInterface.messageBox("test");
	//_UI->messageBox("Test Export");
	Exporter * e = new Exporter(_APP);
	e->exportExampleXml();
	delete e;
	//e->exportWheel();
}

void MyCloseEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{
	//close button is clicked
}

void MyHTMLEventHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{

}

void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	// Create a palette
	Ptr<UserInterface> _ui = _APP->userInterface();

	Ptr<Palettes> palettes = _ui->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById("myPalette");
	if (!palette)
	{
		palette = palettes->add("myPalette", "My Palette", "palette/palette.html", true, true, true, 300, 200);
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
	else
	{
		palette->isVisible(true);
	}
}

void ShowPaletteCommandCreatedHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	Ptr<UserInterface> _ui = _APP->userInterface();
	Ptr<Command> command = eventArgs->command();
	if (!command)
		return;
	Ptr<CommandEvent> exec = command->execute();
	if (!exec)
		return;

	ShowPaletteCommandExecuteHandler * onShowPaletteCommandExecuted_ = new ShowPaletteCommandExecuteHandler;
	onShowPaletteCommandExecuted_->_APP = _APP;
	exec->add(onShowPaletteCommandExecuted_);
}

void SendInfoCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	// Send information to the palette.
	Ptr<UserInterface> _ui = _APP->userInterface();
	Ptr<Palettes> palettes = _ui->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById("myPalette");
	if (!palette)
		return;

	palette->sendInfoToHTML("send", "This is a message sent to the palette from Fusion. It has been sent times.");
}
