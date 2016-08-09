#pragma once

#include <CLEW.h>

#include <vector>
#include <iostream>

class CLHelper
{
public:
	static int GetDevice(std::vector<cl_device_id> & devices);
private:
	static inline void checkErr(cl_int err, const char * name)
	{
		if (err != CL_SUCCESS) {
			std::cerr << "ERROR: " << name
				<< " (" << err << ")" << std::endl;
			exit(EXIT_FAILURE);
		}
	}
};

