#ifndef __WAIT_SEMAPHORE_H
#define __WAIT_SEMAPHORE_H

#include <OSAL/OSAL.h>

#if USE_WINAPI
#include <Windows.h>
#elif
asdf	// Just die.  Windows only so far
#endif

class WaitSemaphore
{
private:
#if USE_WINAPI
	HANDLE _handle;
#endif
public:
	WaitSemaphore(void)
	{
#if USE_WINAPI
		_handle = CreateEvent(0, FALSE, FALSE, 0);
#endif
	}

	~WaitSemaphore(void)
	{
#if USE_WINAPI
		CloseHandle(_handle);
#endif
	}

	/**
	* time in millis
	*/
	bool wait(unsigned long time = INFINITE)
	{
#if USE_WINAPI
		return WaitForSingleObject(_handle, time) == 0;
#endif
	}

	void notify()
	{
#if USE_WINAPI
		SetEvent(_handle);
#endif
	}
};

#endif