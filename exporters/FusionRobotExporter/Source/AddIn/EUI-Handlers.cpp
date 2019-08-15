#include "EUI.h"

using namespace SynthesisAddIn;

// Workspace Activated Handler
template<>
bool EUI::addHandler<WorkspaceActivatedHandler>(Ptr<UserInterface> UI, WorkspaceActivatedHandler* workspaceActivatedHandler)
{
	if (workspaceActivatedHandler == nullptr)
		workspaceActivatedHandler = new WorkspaceActivatedHandler(this);

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceActivated();
	if (workspaceEvent)
		workspaceEvent->add(workspaceActivatedHandler);

	return true;
}

template<>
bool EUI::clearHandler<WorkspaceActivatedHandler>(Ptr<UserInterface> UI, WorkspaceActivatedHandler* workspaceActivatedHandler)
{
	if (workspaceActivatedHandler == nullptr)
		return false;

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceActivated();
	if (workspaceEvent)
		workspaceEvent->remove(workspaceActivatedHandler);

	return true;
}

// Fusion 360 startup finished
template<>
bool EUI::addHandler<StartupCompletedHandler>(Ptr<UserInterface> UI, StartupCompletedHandler * startupCompletedHandler)
{
	startupCompletedHandler = new StartupCompletedHandler(this);

	Ptr<ApplicationEvent> startupEvent = app->startupCompleted();
	if (!startupEvent)
		return false;

	return startupEvent->add(startupCompletedHandler);

	//if (startupCompletedHandler == nullptr)
	//	startupCompletedHandler = new StartupCompletedHandler(this);

	//Ptr<ApplicationEvent> startupEvent = app->startupCompleted();
	//if (startupEvent)
	//	return startupEvent->add(startupCompletedHandler);
}

// Workspace Deactivated Handler
template<>
bool EUI::addHandler<WorkspaceDeactivatedHandler>(Ptr<UserInterface> UI, WorkspaceDeactivatedHandler* workspaceDeactivatedHandler)
{
	if (workspaceDeactivatedHandler == nullptr)
		workspaceDeactivatedHandler = new WorkspaceDeactivatedHandler(this);

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceDeactivated();
	if (workspaceEvent)
		return workspaceEvent->add(workspaceDeactivatedHandler);
}

template<>
bool EUI::clearHandler<WorkspaceDeactivatedHandler>(Ptr<UserInterface> UI, WorkspaceDeactivatedHandler* workspaceDeactivatedHandler)
{
	if (workspaceDeactivatedHandler == nullptr)
		return false;

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceDeactivated();
	if (!workspaceEvent)
		return false;

	return workspaceEvent->remove(workspaceDeactivatedHandler);
}

// Show Palette Command Created Handler
template<>
bool EUI::addHandler<ShowPaletteCommandCreatedHandler>(Ptr<CommandDefinition> commandDef, ShowPaletteCommandCreatedHandler* showPaletteCommandCreatedHandler)
{
	showPaletteCommandCreatedHandler = new ShowPaletteCommandCreatedHandler(this, commandDef->id());

	Ptr<CommandCreatedEvent> commandEvent = commandDef->commandCreated();
	if (!commandEvent)
		return false;

	return commandEvent->add(showPaletteCommandCreatedHandler);
}

template<>
bool EUI::clearHandler<ShowPaletteCommandCreatedHandler>(Ptr<CommandDefinition> commandDef, ShowPaletteCommandCreatedHandler* showPaletteCommandCreatedHandler)
{
	if (showPaletteCommandCreatedHandler == nullptr)
		return false;

	Ptr<CommandCreatedEvent> commandEvent = commandDef->commandCreated();
	if (!commandEvent)
		return false;

	return commandEvent->remove(showPaletteCommandCreatedHandler);
}

// Receive Form Data Handler
template<>
bool EUI::addHandler<ReceiveFormDataHandler>(Ptr<Palette> palette, ReceiveFormDataHandler* receiveFormDataHandler)
{
	if (receiveFormDataHandler == nullptr)
		receiveFormDataHandler = new ReceiveFormDataHandler(this);

	Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
	if (!htmlEvent)
		return false;
	
	return htmlEvent->add(receiveFormDataHandler);
}

template<>
bool EUI::clearHandler<ReceiveFormDataHandler>(Ptr<Palette> palette, ReceiveFormDataHandler* receiveFormDataHandler)
{
	if (receiveFormDataHandler == nullptr)
		return false;

	Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
	if (!htmlEvent)
		return false;

	return htmlEvent->remove(receiveFormDataHandler);
}

// Close Exporter Form Handler
template<>
bool EUI::addHandler<ClosePaletteEventHandler>(Ptr<Palette> palette, ClosePaletteEventHandler* closePaletteEventHandler)
{
	closePaletteEventHandler = new ClosePaletteEventHandler(this, palette->id());

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (!closeEvent)
		return false;
	
	return closeEvent->add(closePaletteEventHandler);
}

template<>
bool EUI::clearHandler<ClosePaletteEventHandler>(Ptr<Palette> palette, ClosePaletteEventHandler* closePaletteEventHandler)
{
	if (closePaletteEventHandler == nullptr)
		return false;

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (!closeEvent)
		return false;
	
	return closeEvent->remove(closePaletteEventHandler);
}
