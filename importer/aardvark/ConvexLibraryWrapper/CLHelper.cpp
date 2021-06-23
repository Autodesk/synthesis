#include "stdafx.h"
#include "CLHelper.h"

#include <vector>

int CLHelper::GetDevice(std::vector<cl_device_id> & devices)
{
	cl_int err;
	cl_uint numPlatforms;
	cl_uint numDevices;

	err = clGetPlatformIDs(0, NULL, &numPlatforms);
	if (err != CL_SUCCESS || numPlatforms == 0)
	{
		std::cout << "Couldn't get platform info" << std::endl;
		return -1;
	}

	cl_platform_id * platforms = new cl_platform_id[numPlatforms];
	err = clGetPlatformIDs(numPlatforms, platforms, NULL);
	if (err != CL_SUCCESS)
	{
		std::cout << "Couldn't get platform info" << std::endl;
		return -1;
	}

	std::cout << std::string(20, '-').c_str() << std::endl;

	char nameBuf[80];
	for (int i = 0; i < numPlatforms; i++)
	{
		err = clGetPlatformInfo(platforms[i], CL_PLATFORM_NAME, 80, nameBuf, NULL);
		if (err == CL_SUCCESS)
		{
			char nameString[100];
			sprintf_s(nameString, 100, "[%d] Platform: %s", i, nameBuf);
			std::cout << nameString << std::endl;
		}

		err = clGetDeviceIDs(platforms[i], CL_DEVICE_TYPE_ALL, 0, NULL, &numDevices);
		if (err != CL_SUCCESS || numDevices == 0)
		{
			std::cout << "Couldn't get devices for " << nameBuf << std::endl;
			return -1;
		}

		cl_device_id * platformDevices = new cl_device_id[numDevices];

		err = clGetDeviceIDs(platforms[i], CL_DEVICE_TYPE_ALL, numDevices, platformDevices, NULL);
		if (err != CL_SUCCESS)
		{
			std::cout << "Couldn't get devices for " << nameBuf << std::endl;
			return -1;
		}

		for (int j = 0; j < numDevices; j++)
		{
			err = clGetDeviceInfo(platformDevices[j], CL_DEVICE_NAME, 80, nameBuf, NULL);
			if (err == CL_SUCCESS)
			{
				char nameString[98];
				sprintf_s(nameString, 98, "- [%d] Device: %s", j, nameBuf);
				std::cout << nameString << std::endl;
			}

			devices.push_back(platformDevices[j]);
		}

		std::cout << std::string(20, '-').c_str() << std::endl;

		delete platformDevices;
	}

	delete platforms;

	return 0;
}
