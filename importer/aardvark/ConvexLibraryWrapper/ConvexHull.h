#pragma once

namespace ConvexLibraryWrapper
{
	public ref class ConvexHull
	{
	public:
		ConvexHull() {}
		ConvexHull(VHACD::IVHACD::ConvexHull * toCopy) { CopyFromUnmanaged(toCopy); }

		array<double> ^ m_points;
		array<int> ^ m_triangles;
		unsigned int m_nPoints;
		unsigned int m_nTriangles;

	internal:
		inline void CopyFromUnmanaged(VHACD::IVHACD::ConvexHull * toCopy)
		{
			m_points = gcnew array<double>(toCopy->m_nPoints * 3);
			m_triangles = gcnew array<int>(toCopy->m_nTriangles * 3);

			System::Runtime::InteropServices::Marshal::Copy(System::IntPtr((void *) toCopy->m_points), m_points, 0, toCopy->m_nPoints * 3);
			System::Runtime::InteropServices::Marshal::Copy(System::IntPtr((void *) toCopy->m_triangles), m_triangles, 0, toCopy->m_nTriangles * 3);
			m_nPoints = toCopy->m_nPoints;
			m_nTriangles = toCopy->m_nTriangles;
		}
	};
}
