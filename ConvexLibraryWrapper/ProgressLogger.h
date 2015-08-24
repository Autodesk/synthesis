#pragma once

#include <iostream>

#include <public\VHACD.h>

namespace ConvexLibraryWrapper
{
	public class ProgressLogger : public VHACD::IVHACD::IUserLogger
	{
	public:
		ProgressLogger(void) {}
		~ProgressLogger(void) {}

		void Log(const char * const msg)
		{
			std::cout << msg;
		}
	};
}