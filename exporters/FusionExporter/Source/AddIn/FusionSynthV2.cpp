#include <Core/CoreAll.h>
#include "EUI.h"
#include "Identifiers.h"

using namespace adsk::core;

Ptr<Application> app;
Ptr<UserInterface> UI;

Synthesis::EUI * EUI = nullptr;
Synthesis::WorkspaceActivatedHandler * activatedHandler = nullptr;
Synthesis::WorkspaceDeactivatedHandler * deactivatedHandler = nullptr;

extern "C" XI_EXPORT bool run(const char* context)
{
	app = Application::get();
	if (!app)
		return false;

	UI = app->userInterface();
	if (!UI)
		return false;

	EUI = new Synthesis::EUI(UI, app);

	// Add workspace events
	Ptr<WorkspaceEvent> workspaceActivatedEvent = UI->workspaceActivated();
	Ptr<WorkspaceEvent> workspaceDeactivatedEvent = UI->workspaceDeactivated();

	if (!workspaceActivatedEvent || !workspaceDeactivatedEvent)
		return false;

	activatedHandler = new Synthesis::WorkspaceActivatedHandler(EUI);
	workspaceActivatedEvent->add(activatedHandler);

	deactivatedHandler = new Synthesis::WorkspaceDeactivatedHandler(EUI);
	workspaceDeactivatedEvent->add(deactivatedHandler);

	return true;
}

extern "C" XI_EXPORT bool stop(const char* context)
{
	if (UI)
	{
		delete EUI;
		EUI = nullptr;

		// Delete old handlers
		Ptr<WorkspaceEvent> workspaceActivatedEvent = UI->workspaceActivated();
		Ptr<WorkspaceEvent> workspaceDeactivatedEvent = UI->workspaceDeactivated();

		if (!workspaceActivatedEvent || !workspaceDeactivatedEvent)
			return false;

		workspaceActivatedEvent->remove(activatedHandler);
		delete activatedHandler;
		activatedHandler = nullptr;

		workspaceDeactivatedEvent->remove(deactivatedHandler);
		delete deactivatedHandler;
		deactivatedHandler = nullptr;

		// Delete reference to UI
		app = nullptr;
		UI = nullptr;
	}

	return true;
}

#ifdef XI_WIN

#include <windows.h>

BOOL APIENTRY DllMain(HMODULE hmodule, DWORD reason, LPVOID reserved)
{
	switch (reason)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

#endif // XI_WIN
