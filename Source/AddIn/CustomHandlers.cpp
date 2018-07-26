#include "CustomHandlers.h"

using namespace Synthesis;

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
	ShowPaletteCommandExecuteHandler * onShowPaletteCommandExecuted = new ShowPaletteCommandExecuteHandler;
	onShowPaletteCommandExecuted->palette = palette;
	exec->add(onShowPaletteCommandExecuted);
}

// Show Palette Button Event
void ShowPaletteCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
{
	palette->isVisible(true);
}

/// Palette Events
// Send info to palette HTML
/*void SendInfoCommandExecuteHandler::notify(const Ptr<CommandEventArgs>& eventArgs)
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
}*/

// Close Exporter Form Event
void CloseFormEventHandler::notify(const Ptr<UserInterfaceGeneralEventArgs>& eventArgs)
{

}

// Submit Exporter Form Event
void ReceiveFormDataHandler::notify(const Ptr<HTMLEventArgs>& eventArgs)
{

}
