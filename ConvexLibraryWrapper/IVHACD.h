#pragma once
#pragma comment(lib, "VHACD_LIB.lib")

#include <include\VHACD.h>

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

	protected:
		~IVHACD(void);
		
		//The instance of the unmanaged class
		VHACD::IVHACD *instance;
	};
}
