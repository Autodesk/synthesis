#pragma once

#if _DEBUG
#pragma comment(lib, "..\\VHACD\\Debug\\VHACD.lib")
#pragma comment(lib, "..\\CLEW\\Debug\\CLEW.lib")
#else
#pragma comment(lib, "..\\VHACD\\Release\\VHACD.lib")
#pragma comment(lib, "..\\CLEW\\Release\\CLEW.lib")
#endif

#include <public\VHACD.h>

#include "ConvexHull.h"
#include "Parameters.h"

namespace ConvexLibraryWrapper
{
	public ref class IVHACD
	{
	public:
		IVHACD(void);

		void Cancel(void);
		bool Compute(array<float> ^ points,
					 const unsigned int stridePoints,
					 const unsigned int countPoints,
					 array<int> ^ triangles,
					 const unsigned int strideTriangles,
					 const unsigned int countTriangles,
					 Parameters ^ params);
		//bool Compute(double points[],
		//			 const unsigned int stridePoints,
		//			 const unsigned int countPoints,
		//			 int triangles[],
		//			 const unsigned int strideTriangles,
		//			 const unsigned int countTriangles,
		//			 Parameters ^ params);
		unsigned int GetNConvexHulls(void);
		ConvexHull ^ GetConvexHull(const unsigned int index);
		void Clean(void);
		void Release(void);
		bool OCLInit(Parameters ^ params);
		bool OCLRelease(Parameters ^ params);

	protected:
		~IVHACD(void);
		
		//The instance of the unmanaged class
		VHACD::IVHACD * instance;

	private:
		static bool clLoaded = false;
	};
}
