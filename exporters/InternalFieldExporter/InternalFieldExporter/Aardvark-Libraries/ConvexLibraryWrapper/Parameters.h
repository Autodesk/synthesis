#pragma once

#include <public\VHACD.h>
#include "ProgressCallback.h"
#include "ProgressLogger.h"

using namespace VHACD;

namespace ConvexLibraryWrapper
{
	public ref class Parameters
	{
	public:
		Parameters(void)
		{
			m_concavity = 0.0005;
			m_alpha = 0.05;
			m_beta = 0.05;
			m_gamma = 0.0005;
			m_delta = 0.05;
			m_minVolumePerCH = 0.0001;
			m_resolution = 110000;
			m_maxNumVerticesPerCH = 64;
			m_depth = 30;
			m_planeDownsampling = 4;
			m_convexhullDownsampling = 4;
			m_pca = 0;
			m_mode = 0;
			m_convexhullApproximation = true;
			m_oclAcceleration = false;

			m_callback = new ProgressCallback();
			m_logger = new ProgressLogger();
		}

		//http://kmamou.blogspot.com/2014/12/v-hacd-20-parameters-description.html
		double m_concavity;
		double m_alpha;
		double m_beta;
		double m_gamma;
		double m_delta;
		double m_minVolumePerCH;
		unsigned int m_resolution;
		unsigned int m_maxNumVerticesPerCH;
		int m_depth;
		int m_planeDownsampling;
		int m_convexhullDownsampling;
		int m_pca;
		int m_mode;
		int m_convexhullApproximation;
		int m_oclAcceleration;

		VHACD::IVHACD::IUserCallback * m_callback;
		VHACD::IVHACD::IUserLogger * m_logger;

	internal:
		inline void CopyToUnmanaged(VHACD::IVHACD::Parameters * toCopy)
		{
			toCopy->m_concavity = m_concavity;
			toCopy->m_alpha = m_alpha;
			toCopy->m_beta = m_beta;
			toCopy->m_gamma = m_gamma;
			toCopy->m_delta = m_delta;
			toCopy->m_minVolumePerCH = m_minVolumePerCH;
			toCopy->m_resolution = m_resolution;
			toCopy->m_maxNumVerticesPerCH = m_maxNumVerticesPerCH;
			toCopy->m_depth = m_depth;
			toCopy->m_planeDownsampling = m_planeDownsampling;
			toCopy->m_convexhullDownsampling = m_convexhullDownsampling;
			toCopy->m_pca = m_pca;
			toCopy->m_mode = m_mode;
			toCopy->m_convexhullApproximation = m_convexhullApproximation;
			toCopy->m_oclAcceleration = m_oclAcceleration;

			toCopy->m_callback = m_callback;
			toCopy->m_logger = m_logger;
		}
	};
}