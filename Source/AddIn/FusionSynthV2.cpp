#include <Core/CoreAll.h>
#include "EUI.h"
#include "Identifiers.h"

using namespace adsk::core;

Ptr<Application> app;
Ptr<UserInterface> UI;

Synthesis::EUI * EUI;

extern "C" XI_EXPORT bool run(const char* context)
{
	app = Application::get();
	if (!app)
		return false;

	UI = app->userInterface();
	if (!UI)
		return false;

	EUI = new Synthesis::EUI(UI, app);

	return true;
}

extern "C" XI_EXPORT bool stop(const char* context)
{
	if (UI)
	{
		// Delete palette
		Ptr<Palettes> palettes = UI->palettes();
		if (!palettes)
			return false;

		Ptr<Palette> palette = palettes->itemById(Synthesis::K_EXPORT_PALETTE);
		if (palette)
			palette->deleteMe();

		// Delete controls and associated command definitions
		Ptr<ToolbarPanelList> panels = UI->allToolbarPanels();
		if (!panels)
			return false;

		Ptr<ToolbarPanel> panel = panels->itemById(Synthesis::K_PANEL);
		if (!panel)
			return false;

		Ptr<ToolbarControls> controls = panel->controls();
		if (!controls)
			return false;

		Ptr<ToolbarControl> ctrl = controls->itemById(Synthesis::K_EXPORT_BUTTON);
		if (ctrl)
			ctrl->deleteMe();

		Ptr<CommandDefinitions> commandDefinitions = UI->commandDefinitions();
		if (!commandDefinitions)
			return false;

		Ptr<CommandDefinition> cmdDef = commandDefinitions->itemById(Synthesis::K_EXPORT_BUTTON);
		if (cmdDef)
			cmdDef->deleteMe();

		// Delete reference to UI
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
