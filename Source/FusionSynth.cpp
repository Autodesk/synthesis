#include "Exporter.h"

Ptr<Application> app;
Synthesis::Exporter * e;

extern "C" XI_EXPORT bool run(const char* context)
{
	app = Application::get();
	if (!app)
		return false;
    
    if (!e){
        e = new Synthesis::Exporter(app);
    }

	return true;
}

extern "C" XI_EXPORT bool stop(const char* context)
{
	delete(e);

	//DLL needs to be unloaded.
	//make sure the items that are createdin the UI get destroyed on call to stop the addin.

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
