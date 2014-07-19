/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __BINARY_IMAGE_H__
#define __BINARY_IMAGE_H__

#include "MonoImage.h"

#include <vector>
#include <algorithm>
using namespace std;

#if ENABLE_NIVISION
typedef struct ParticleAnalysisReport_struct {
	int 	imageHeight;
	int 	imageWidth;
	double 	imageTimestamp;				
	int		particleIndex;				// the particle index analyzed
	/* X-coordinate of the point representing the average position of the 
	 * total particle mass, assuming every point in the particle has a constant density */
	int 	center_mass_x;  			// MeasurementType: IMAQ_MT_CENTER_OF_MASS_X 
	/* Y-coordinate of the point representing the average position of the 
	 * total particle mass, assuming every point in the particle has a constant density */
	int 	center_mass_y;  			// MeasurementType: IMAQ_MT_CENTER_OF_MASS_Y 
	double 	center_mass_x_normalized;  	//Center of mass x value normalized to -1.0 to +1.0 range
	double 	center_mass_y_normalized;  	//Center of mass y value normalized to -1.0 to +1.0 range
	/* Area of the particle */
	double 	particleArea;				// MeasurementType: IMAQ_MT_AREA
	/* Bounding Rectangle */
	Rect 	boundingRect;				// left/top/width/height
	/* Percentage of the particle Area covering the Image Area. */
	double 	particleToImagePercent;		// MeasurementType: IMAQ_MT_AREA_BY_IMAGE_AREA
	/* Percentage of the particle Area in relation to its Particle and Holes Area */
	double 	particleQuality;			// MeasurementType: IMAQ_MT_AREA_BY_PARTICLE_AND_HOLES_AREA
} ParticleAnalysisReport;

class BinaryImage : public MonoImage
{
public:
	BinaryImage();
	virtual ~BinaryImage();
	int GetNumberParticles();
	ParticleAnalysisReport GetParticleAnalysisReport(int particleNumber);
	void GetParticleAnalysisReport(int particleNumber, ParticleAnalysisReport *par);
	vector<ParticleAnalysisReport>* GetOrderedParticleAnalysisReports();
	BinaryImage *RemoveSmallObjects(bool connectivity8, int erosions);
	BinaryImage *RemoveLargeObjects(bool connectivity8, int erosions);
	BinaryImage *ConvexHull(bool connectivity8);
	BinaryImage *ParticleFilter(ParticleFilterCriteria2 *criteria, int criteriaCount);
	virtual void Write(const char *fileName);
private:
	bool ParticleMeasurement(int particleNumber, MeasurementType whatToMeasure, int *result);
	bool ParticleMeasurement(int particleNumber, MeasurementType whatToMeasure, double *result);
	static double NormalizeFromRange(double position, int range);
	static bool CompareParticleSizes(ParticleAnalysisReport particle1, ParticleAnalysisReport particle2);
};

#endif
#endif