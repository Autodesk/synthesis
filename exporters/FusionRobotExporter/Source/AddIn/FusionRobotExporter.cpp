#include <Core/CoreAll.h>
#include "EUI.h"
#include "Identifiers.h"

using namespace adsk::core;

Ptr<Application> app;
Ptr<UserInterface> UI;

SynthesisAddIn::EUI * EUI = nullptr;

extern "C" XI_EXPORT bool run(const char* context)
{
	app = Application::get();
	if (!app)
		return false;

	UI = app->userInterface();
	if (!UI)
		return false;

	EUI = new SynthesisAddIn::EUI(UI, app);

	return true;
}

extern "C" XI_EXPORT bool stop(const char* context)
{
	if (UI)
	{
		delete EUI;
		EUI = nullptr;

		// Delete reference to UI
		app = nullptr;
		UI = nullptr;
	}

	return true;
}

#ifdef XI_WIN


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
