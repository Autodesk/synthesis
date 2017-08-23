#pragma once

#include <iostream>

#include <public\VHACD.h>

namespace ConvexLibraryWrapper
{
	public class ProgressCallback : public VHACD::IVHACD::IUserCallback
	{
	public:
		ProgressCallback(void) {}
		~ProgressCallback(void) {}

		void Update(const double overallProgress,
			        const double stageProgress,
					const double operationProgress,
					const char * const stage,
					const char * const operation)
		{
			std::cout << overallProgress << "%" << std::endl;
		}
	};
}
