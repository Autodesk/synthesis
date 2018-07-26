#include "CustomHandlers.h"

using namespace Synthesis;

/// Button Events
// Create Palette Button Event
void ShowPaletteCommandCreatedHandler::notify(const Ptr<CommandCreatedEventArgs>& eventArgs)
{
	Ptr<UserInterface> UI = app->userInterface();

	Ptr<Command> command = eventArgs->command();
	if (!command)
		return;

	Ptr<CommandEvent> exec = command->execute();
	if (!exec)
		return;

	// Add click command to button
	ShowPaletteCommandExecuteHandler * onShowPaletteCommandExecuted = new ShowPaletteCommandExecuteHandler;
	onShowPaletteCommandExecuted->app = app;
	exec->add(onShowPaletteCommandExecuted);
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	Ptr<UserInterface> UI = app->userInterface();

	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	// Get palette if it already exists
	Ptr<Palette> palette = palettes->itemById("myPalette");

	if (!palette)
	{
		palette = palettes->add("exporterForm", "Robot Exporter Form", "Palette/palette.html", false, true, true, 300, 200);
		if (!palette)
			return;

		// Dock the palette to the right side of Fusion window.
		palette->dockingState(PaletteDockStateRight);

		// Add handler to HTMLEvent of the palette
		Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
		if (!htmlEvent)
			return;

		ReceiveFormDataHandler * onHTMLEvent = new ReceiveFormDataHandler;
		onHTMLEvent->app = app;

		htmlEvent->add(onHTMLEvent);

		// Add handler to CloseEvent of the palette
		Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
		if (!closeEvent)
			return;

		CloseFormEventHandler * onClose = new CloseFormEventHandler;
		onClose->app = app;

		closeEvent->add(onClose);
	}

	// Show palette
	palette->isVisible(true);
}

/// Palette Events
// Send info to palette HTML
void SendInfoCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	// Get the palette
	Ptr<UserInterface> UI = app->userInterface();

	Ptr<Palettes> palettes = UI->palettes();
	if (!palettes)
		return;

	Ptr<Palette> palette = palettes->itemById("myPalette");
	if (!palette)
		return;

	// Send info to the palette
	palette->sendInfoToHTML("send", "This is a message sent to the palette from Fusion. It has been sent times.");
}

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{

}

// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{

}
