
#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>
#include <CAM/CAMAll.h>
#include <string>

using namespace adsk::core;
using namespace adsk::fusion;
using namespace adsk::cam;
using namespace std;

Ptr<Application> app;
Ptr<UserInterface> ui;
Ptr<Components> comps;

extern "C" XI_EXPORT bool run(const char* context)
{
	app = Application::get();
	if (!app)
		return false;

	ui = app->userInterface();
	if (!ui)
		return false;

	ui->messageBox("Started Exporting");
    
    Ptr<FusionDocument> doc = app->activeDocument();
    
    string a = "";
    
    for (Ptr<Joint> j : doc->design()->rootComponent()->allJoints()){
        a += j->name() + " ";
        doc->design()->activeComponent() = j->parentComponent();
    }
    
    ui->messageBox(a);
    
	return true;
}

extern "C" XI_EXPORT bool stop(const char* context)
{
	if (ui)
	{
		ui->messageBox("Stop addin");
		ui = nullptr;
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
