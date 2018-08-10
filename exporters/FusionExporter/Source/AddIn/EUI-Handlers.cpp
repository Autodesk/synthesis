#include "EUI.h"

using namespace Synthesis;

// Workspace Activated Handler
template<>
bool EUI::addHandler<WorkspaceActivatedHandler>(Ptr<UserInterface> UI)
{
	if (workspaceActivatedHandler == nullptr)
		workspaceActivatedHandler = new WorkspaceActivatedHandler(this);

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceActivated();
	if (workspaceEvent)
		workspaceEvent->add(workspaceActivatedHandler);

	return true;
}

template<>
bool EUI::clearHandler<WorkspaceActivatedHandler>(Ptr<UserInterface> UI)
{
	if (workspaceActivatedHandler == nullptr)
		return false;

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceActivated();
	if (workspaceEvent)
		workspaceEvent->remove(workspaceActivatedHandler);

	return true;
}

// Workspace Deactivated Handler
template<>
bool EUI::addHandler<WorkspaceDeactivatedHandler>(Ptr<UserInterface> UI)
{
	if (workspaceDeactivatedHandler == nullptr)
		workspaceDeactivatedHandler = new WorkspaceDeactivatedHandler(this);

	Ptr<WorkspaceEvent> workspaceEvent = UI->workspaceDeactivated();
	if (workspaceEvent)
		return workspaceEvent->add(workspaceDeactivatedHandler);
}

template<>
bool EUI::clearHandler<WorkspaceDeactivatedHandler>(Ptr<UserInterface> UI)
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
bool EUI::addHandler<ShowPaletteCommandCreatedHandler>(Ptr<CommandDefinition> commandDef)
{
	if (showPaletteCommandCreatedHandler == nullptr)
		showPaletteCommandCreatedHandler = new ShowPaletteCommandCreatedHandler(this);

	Ptr<CommandCreatedEvent> commandEvent = commandDef->commandCreated();
	if (!commandEvent)
		return false;

	return commandEvent->add(showPaletteCommandCreatedHandler);
}

template<>
bool EUI::clearHandler<ShowPaletteCommandCreatedHandler>(Ptr<CommandDefinition> commandDef)
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
bool EUI::addHandler<ReceiveFormDataHandler>(Ptr<Palette> palette)
{
	if (receiveFormDataHandler == nullptr)
		receiveFormDataHandler = new ReceiveFormDataHandler(this);

	Ptr<HTMLEvent> htmlEvent = palette->incomingFromHTML();
	if (!htmlEvent)
		return false;
	
	return htmlEvent->add(receiveFormDataHandler);
}

template<>
bool EUI::clearHandler<ReceiveFormDataHandler>(Ptr<Palette> palette)
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
bool EUI::addHandler<CloseExporterFormEventHandler>(Ptr<Palette> palette)
{
	if (closeExporterFormEventHandler == nullptr)
		closeExporterFormEventHandler = new CloseExporterFormEventHandler(this);

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (!closeEvent)
		return false;
	
	return closeEvent->add(closeExporterFormEventHandler);
}

template<>
bool EUI::clearHandler<CloseExporterFormEventHandler>(Ptr<Palette> palette)
{
	if (closeExporterFormEventHandler == nullptr)
		return false;

	Ptr<UserInterfaceGeneralEvent> closeEvent = palette->closed();
	if (!closeEvent)
		return false;
	
	return closeEvent->remove(closeExporterFormEventHandler);
}
