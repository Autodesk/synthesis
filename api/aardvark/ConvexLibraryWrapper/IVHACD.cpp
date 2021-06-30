#include "Stdafx.h"
#include "IVHACD.h"
#include "CLHelper.h"

#include <cstdlib>
#include <iostream>
#include <vector>

namespace ConvexLibraryWrapper
{
	IVHACD::IVHACD(void)
	{
		instance = VHACD::CreateVHACD();
	}

	IVHACD::~IVHACD(void)
	{
		instance->Release();
	}

	void IVHACD::Cancel(void)
	{
		instance->Cancel();
	}

	bool IVHACD::Compute(array<float> ^ points,
		const unsigned int stridePoints,
		const unsigned int countPoints,
		array<int> ^ triangles,
		const unsigned int strideTriangles,
		const unsigned int countTriangles,
		Parameters ^ params)
	{
		VHACD::IVHACD::Parameters * newParams = new VHACD::IVHACD::Parameters();
		params->CopyToUnmanaged(newParams);

		float * meshPoints = (float *) malloc(points->Length * sizeof(float));
		System::Runtime::InteropServices::Marshal::Copy(points, 0, System::IntPtr((void *)meshPoints), points->Length);

		int * meshTriangles = (int *) malloc(triangles->Length * sizeof(unsigned int));
		System::Runtime::InteropServices::Marshal::Copy(triangles, 0, System::IntPtr((void *)meshTriangles), triangles->Length);

		bool result;
		try
		{
			result = instance->Compute((const float * const) meshPoints, stridePoints, countPoints, 
				(const int * const) meshTriangles, strideTriangles, countTriangles, 
				(const VHACD::IVHACD::Parameters &) *newParams);
		}
		catch (System::Runtime::InteropServices::SEHException ^ e)
		{
			return false;
		}

		return result;
	}

	unsigned int IVHACD::GetNConvexHulls(void)
	{
		return instance->GetNConvexHulls();
	}

	ConvexHull ^ IVHACD::GetConvexHull(const unsigned int index)
	{
		VHACD::IVHACD::ConvexHull * ch = new VHACD::IVHACD::ConvexHull();
		instance->GetConvexHull(index, (VHACD::IVHACD::ConvexHull &) *ch);
		ConvexHull ^ hull = gcnew ConvexHull(ch);
		return hull;
	}

	void IVHACD::Clean(void)
	{
		instance->Clean();
	}

	void IVHACD::Release(void)
	{
		instance->Release();
	}

	bool IVHACD::OCLInit(Parameters ^ params)
	{
		if (!clLoaded)
		{
			int err = clewInit();
			if (err != CL_SUCCESS)
			{
				throw gcnew System::DllNotFoundException("OpenCL library not found");
			}

			clLoaded = true;
		}

		std::vector<cl_device_id> devices;
		int err = CLHelper::GetDevice(devices);
		if (err == -1) return false;

		return instance->OCLInit(&devices[0], params->m_logger);
	}

	bool IVHACD::OCLRelease(Parameters ^ params)
	{
		if (clLoaded)
		{
			return instance->OCLRelease(params->m_logger);
		}
		else return false;
	}

}
