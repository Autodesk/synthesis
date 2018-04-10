#include "Exporter.h"

Ptr<Application> _app;
Exporter * e;

extern "C" XI_EXPORT bool run(const char* context)
{
	_app = Application::get();
	if (!_app)
		return false;

	e = new Exporter(_app);

	e->test();

	return true;
}

extern "C" XI_EXPORT bool stop(const char* context)
{
	delete(e);

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
