/********************************************************************************
*  Project   		: FIRST Motor Controller
*  File Name  		: TrackAPI.cpp        
*  Contributors   	: ELF, DWD
*  Creation Date 	: August 10, 2008
*  Revision History	: Source code & revision history maintained at sourceforge.WPI.edu   
*  File Description	: Tracking Routines for FIRST Vision API
*/ 
/*----------------------------------------------------------------------------*/
/*        Copyright (c) FIRST 2008.  All Rights Reserved.                     */
/*  Open Source Software - may be modified and shared by FRC teams. The code  */
/*  must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib. */
/*----------------------------------------------------------------------------*/

#include "string.h"
#include "vxWorks.h" 

#include "AxisCamera.h" 
#include "FrcError.h"
#include "TrackAPI.h" 
#include "VisionAPI.h" 

int TrackAPI_debugFlag = 0;
#define DPRINTF if(TrackAPI_debugFlag)dprintf

/**
* @brief Find the largest particle that meets a criteria 
* @param binaryImage Image to inspect
* @param rect area to search
* @return 0 = error
*/
bool InArea(Image* binaryImage, int particleIndex, Rect rect)
{ 
	double position;
	
	imaqMeasureParticle(binaryImage, particleIndex, 0, 
			IMAQ_MT_BOUNDING_RECT_LEFT, &position);
	if ( position < (rect.left             ) ) return false; // outside left of rectangle?
	
	imaqMeasureParticle(binaryImage, particleIndex, 0, 
			IMAQ_MT_BOUNDING_RECT_TOP, &position);
	if ( position < (rect.top              ) ) return false; // outside top of rectangle ?

	imaqMeasureParticle(binaryImage, particleIndex, 0, 
			IMAQ_MT_BOUNDING_RECT_RIGHT, &position);
	if (position > (rect.left + rect.width) ) return false;	// outside right of rectangle ?
	
	imaqMeasureParticle(binaryImage, particleIndex, 0, 
			IMAQ_MT_BOUNDING_RECT_BOTTOM, &position);
	if (position > (rect.top + rect.height) ) return false; // outside bottom of rectangle ?	

	DPRINTF(LOG_INFO, "particle %i is in (%i %i) height %i width %i\n", 
			particleIndex, rect.left, rect.top, rect.height, rect.width);
	return true; 
}

/**
* @brief Find the largest particle that meets a criteria 
* @param binaryImage Image to inspect
* @param largestParticleIndex Index of the largest particle 
* @param rect area to search
* @return 0 = error
*/
int GetLargestParticle(Image* binaryImage, int* largestParticleIndex)
{ return GetLargestParticle(binaryImage, largestParticleIndex, IMAQ_NO_RECT); }

int GetLargestParticle(Image* binaryImage, int* largestParticleIndex, Rect rect)
{
	*largestParticleIndex = 0; // points to caller-provided variable
	
	/* determine number of particles in thresholded image */	
	int numParticles;
	int success = frcCountParticles(binaryImage, &numParticles);
	if ( !success )	{  return success; 	}			
	
	/* if no particles found we can quit here */
	if (numParticles == 0)  {  return 0; 	}  // unsuccessful if zero particles found
	
	// find the largest particle
	double largestParticleArea = 0;
	double particleArea;
	for (int i = 0; i < numParticles; ++i) {		
		success = imaqMeasureParticle(binaryImage, i, 0, IMAQ_MT_AREA, &particleArea);
		if ( !success )	{ return success; }		
		if (particleArea > largestParticleArea) 	{
			// see if is in the right area
			if ( InArea(binaryImage, i, rect) ) {
				largestParticleArea = particleArea;
				*largestParticleIndex = i;  // return index to caller
			}
		}
	}
	
	return success;
}

/**
* @brief Search for a color. Supports IMAQ_IMAGE_HSL. 
* @param color Definition for the hue range 
* @param trackReport Values for tracking: center of particle, particle size, color
* @return 0 = error
*/
int FindColor(FrcHue color, ParticleAnalysisReport* trackReport)
{
	int success = 0;		// return: 0 = error
	
	/* track color */
	// use ACTIVE_LIGHT or WHITE_LIGHT for brightly lit objects
	TrackingThreshold td = GetTrackingData(color, PASSIVE_LIGHT);

	success = FindColor(IMAQ_HSL, &td.hue, &td.saturation, &td.luminance, trackReport); 
	if ( !success )	{ 
		DPRINTF (LOG_INFO, "did not find color - errorCode= %i",GetLastVisionError());	
		return success;
	}

	//PrintReport(par);
	
	/* set an image quality restriction */
	if (trackReport->particleToImagePercent < PARTICLE_TO_IMAGE_PERCENT) {
		imaqSetError(ERR_PARTICLE_TOO_SMALL, __FUNCTION__);
		success = 0;
	}	
	return success;
}

/**
* @brief Search for a color. Supports IMAQ_IMAGE_HSL. 
* @param hueRange The range for the first plane
* @param trackReport Values for tracking: center of particle, particle size, color
* @return 0 = error
*/
int FindColor(const Range* hueRange, ParticleAnalysisReport *trackReport)
{ return FindColor(hueRange, DEFAULT_SATURATION_THRESHOLD, trackReport); }

/**
* @brief Search for a color. Supports IMAQ_IMAGE_HSL. 
* @param hueRange The range for the first plane
* @param minSaturation The lower range saturation
* @param trackReport Values for tracking: center of particle, particle size, color
* @return 0 = error
*/
int FindColor(const Range* hueRange, int minSaturation, ParticleAnalysisReport *trackReport)
{
	Range satRange;
	satRange.minValue = minSaturation;
	satRange.maxValue = 255;
	Range lumRange;
	lumRange.minValue = 0;
	lumRange.maxValue = 255;
	ColorMode cmode = IMAQ_HSL;
	return FindColor(cmode, hueRange, &satRange, &lumRange, trackReport);
}

/**
* @brief Search for a color. Supports IMAQ_IMAGE_HSL and IMAQ_IMAGE_RGB. 
* @param mode Color mode, either IMAQ_HSL or IMAQ_RGB
* @param plane1Range The range for the first plane (hue or red)
* @param plane2Range The range for the second plane (saturation or green)
* @param plane3Range The range for the third plane (luminance or blue)
* @param trackReport Values for tracking: center of particle, particle size, etc
* @return 0 = error
*/
int FindColor(ColorMode mode, const Range* plane1Range, const Range* plane2Range, 
		const Range* plane3Range, ParticleAnalysisReport *trackReport)
{
	return FindColor(mode, plane1Range, plane2Range, plane3Range, trackReport, NULL);
}

/**
* @brief Search for a color. Supports IMAQ_IMAGE_HSL and IMAQ_IMAGE_RGB. 
* @param mode Color mode, either IMAQ_HSL or IMAQ_RGB
* @param plane1Range The range for the first plane (hue or red)
* @param plane2Range The range for the second plane (saturation or green)
* @param plane3Range The range for the third plane (luminance or blue)
* @param trackReport Values for tracking: center of particle, particle size, etc
* @param colorReport Color charactaristics of the particle
* @return 0 = error
*/
int FindColor(ColorMode mode, const Range* plane1Range, const Range* plane2Range, 
		const Range* plane3Range, ParticleAnalysisReport *trackReport, 
		ColorReport *colorReport)
{
	return FindColor(mode, plane1Range, plane2Range, plane3Range, trackReport, 
			NULL, IMAQ_NO_RECT);
}

/**
* @brief Search for a color. Supports IMAQ_IMAGE_HSL and IMAQ_IMAGE_RGB. 
* @param mode Color mode, either IMAQ_HSL or IMAQ_RGB
* @param plane1Range The range for the first plane (hue or red)
* @param plane2Range The range for the second plane (saturation or green)
* @param plane3Range The range for the third plane (luminance or blue)
* @param trackReport Values for tracking: center of particle, particle size, etc
* @param colorReport Color charactaristics of the particle
* @param rect Rectangle to confine search to
* @return 0 = error
*/
int FindColor(ColorMode mode, const Range* plane1Range, const Range* plane2Range, 
		const Range* plane3Range, ParticleAnalysisReport *trackReport, 
		ColorReport *colorReport, Rect rect)
{
	int errorCode = 0;
	int success = 0;
	
	/* create an image object */
	Image* cameraImage = frcCreateImage(IMAQ_IMAGE_HSL);
	if (!cameraImage)  { return success; }
	
	/* get image from camera - if the camera has not finished initializing,
	 * this will fail 
	 */
	double imageTime;
	success = GetImage(cameraImage, &imageTime);
	if (!success){
		DPRINTF(LOG_INFO, "No camera Image available Error = %i %s", 
				errorCode, GetVisionErrorText(errorCode));
		frcDispose(cameraImage); 
		imaqSetError(errorCode, __FUNCTION__);	//reset error code for the caller	
		return success;		
	}	
	
	/* save a copy of the image to another image for color thresholding later */
	Image* histImage = frcCreateImage(IMAQ_IMAGE_HSL);
	if (!histImage)  { frcDispose(cameraImage); return success; }
	success = frcCopyImage(histImage,cameraImage);
	if ( !success )	{ 
		errorCode = GetLastVisionError(); 
		frcDispose(__FUNCTION__,cameraImage,histImage,NULL); 
		return success; 
	}	
	
	/* Color threshold the image */
	success = frcColorThreshold(cameraImage, cameraImage, mode, plane1Range, plane2Range, plane3Range);
	if ( !success )	{ 
		errorCode = GetLastVisionError(); 
		DPRINTF (LOG_DEBUG, "Error = %i  %s ", errorCode, GetVisionErrorText(errorCode));
		frcDispose(__FUNCTION__,cameraImage,histImage,NULL); 
		return success; 
	}	

	int largestParticleIndex = 0;
	success = GetLargestParticle(cameraImage, &largestParticleIndex, rect );
	if ( !success )	{
		errorCode = GetLastVisionError(); 
		DPRINTF (LOG_DEBUG, "Error after GetLargestParticle = %i  %s ", errorCode, GetVisionErrorText(errorCode));
		frcDispose(__FUNCTION__,cameraImage,histImage,NULL); 
		imaqSetError(ERR_COLOR_NOT_FOUND, __FUNCTION__);
		return success; 
	}
	DPRINTF(LOG_INFO, "largestParticleIndex = %i\n", largestParticleIndex);

	/* Particles were found  */
		/* 
		 * Fill in report information for largest particle found
		 */
		success = frcParticleAnalysis(cameraImage, largestParticleIndex, trackReport);
		trackReport->imageTimestamp = imageTime;
		
		/* clean up */
		if (!success) {frcDispose(__FUNCTION__,cameraImage,histImage,NULL); return success;}
		
		/* particle color statistics */
		/* only if a color report requested */
		if (colorReport != NULL)
		{
			/* first filter out the other particles */
			ParticleFilterCriteria2 criteria;
			ParticleFilterOptions* options = NULL;
			Rect rect;
			int numParticles;
			success = frcParticleFilter(cameraImage, cameraImage, &criteria, 1, options, 
					rect, &numParticles);
			if ( !success )	{ 
				DPRINTF(LOG_INFO, "frcParticleFilter errorCode %i", GetLastVisionError()); 
			}	
					
			/* histogram the original image using the thresholded image as a mask */
			int numClasses = 10; //how many classes?
			ColorHistogramReport* chrep = imaqColorHistogram2(histImage, numClasses, IMAQ_HSL, 
						NULL, cameraImage);
			if (chrep == NULL) { 
				DPRINTF(LOG_INFO, "NULL Color Histogram");
				errorCode = GetLastVisionError(); 
			} else {
				colorReport->particleHueMax = chrep->plane1.max;
				colorReport->particleHueMin = chrep->plane1.min;
				colorReport->particleHueMean = chrep->plane1.mean;
				colorReport->particleSatMax = chrep->plane2.max;
				colorReport->particleSatMin  = chrep->plane2.min;
				colorReport->particleSatMean = chrep->plane2.mean;
				colorReport->particleLumMax = chrep->plane3.max;
				colorReport->particleLumMin = chrep->plane3.min;
				colorReport->particleLumMean = chrep->plane3.mean;
				colorReport->numberParticlesFound = numParticles;
				frcDispose(chrep); 
			}
		}

	/* clean up */
	frcDispose(__FUNCTION__,cameraImage,histImage,NULL); 
	
	return success;	
}


/**
 *   Data functions for tracking
 */


/**
 * @brief Get default HSL tracking parameters
 * Note these parameters are not fully characterized at this point
 * Get these default values and modify them as needed for your environment
 * @param hue tasked color
 * @param light saturation/luminance
 */
TrackingThreshold GetTrackingData(FrcHue hue, FrcLight light)
{
	TrackingThreshold trackingData;	
	
	//set saturation & luminance	
	switch (light) {
		default:
		case FLUORESCENT:
			trackingData.saturation.minValue = 100;
			trackingData.saturation.maxValue = 255;
			trackingData.luminance.minValue = 40;
			trackingData.luminance.maxValue = 255;
			if (hue == GREEN) trackingData.luminance.minValue = 100;
			if (hue == PINK) trackingData.saturation.minValue = 80;
			if (hue == PINK) trackingData.luminance.minValue = 60;
			if (hue == PINK) trackingData.luminance.maxValue = 155;
			break;
		case PASSIVE_LIGHT:
			trackingData.saturation.minValue = 50;
			trackingData.saturation.maxValue = 255;
			trackingData.luminance.minValue = 20;
			trackingData.luminance.maxValue = 255;
			break;
		case BRIGHT_LIGHT:
			trackingData.saturation.minValue = 0;
			trackingData.saturation.maxValue = 100;
			trackingData.luminance.minValue = 100;
			trackingData.luminance.maxValue = 255;
			break;
		case ACTIVE_LIGHT:
			trackingData.saturation.minValue = 0;
			trackingData.saturation.maxValue = 50;
			trackingData.luminance.minValue = 150;
			trackingData.luminance.maxValue = 255;
			break;
		case WHITE_LIGHT:
			trackingData.saturation.minValue = 0;
			trackingData.saturation.maxValue = 20;
			trackingData.luminance.minValue = 200;
			trackingData.luminance.maxValue = 255;
			break;
	}
	
	//set hue
	switch (hue){
	default:
	case WHITE:
		strcpy (trackingData.name, "WHITE");
		trackingData.hue.minValue = 0;
		trackingData.hue.maxValue = 255;
		break;
	case ORANGE:
		strcpy (trackingData.name, "ORANGE");
		trackingData.hue.minValue = 5;
		trackingData.hue.maxValue = 25;
		break;
	case YELLOW:
		strcpy (trackingData.name, "YELLOW");
		trackingData.hue.minValue = 30;
		trackingData.hue.maxValue = 50;
		break;
	case GREEN:
		strcpy (trackingData.name, "GREEN");
		if (light == FLUORESCENT) {
			trackingData.hue.minValue = 60;
			trackingData.hue.maxValue = 110;
		} else {
			trackingData.hue.minValue = 90;
			trackingData.hue.maxValue = 125;			
		}
		break;
	case BLUE:
		strcpy (trackingData.name, "BLUE");
		trackingData.hue.minValue = 140;
		trackingData.hue.maxValue = 170;
		break;
	case PURPLE:
		strcpy (trackingData.name, "PURPLE");
		trackingData.hue.minValue = 180;
		trackingData.hue.maxValue = 200;
		break;
	case PINK:
		strcpy (trackingData.name, "PINK");
		trackingData.hue.minValue = 210;
		trackingData.hue.maxValue = 250;
		break;
	case RED:
		strcpy (trackingData.name, "RED");
		trackingData.hue.minValue = 240;
		trackingData.hue.maxValue = 255;
		break;
	}
  return(trackingData);
}


/**
 * Print particle analysis report
 * @param myReport Report to print
 */
void PrintReport(ParticleAnalysisReport* myReport)
{
	dprintf(LOG_INFO, "particle analysis:\n    %s%i  %s%i\n    %s%lf\n    %s%i  %s%i\n    %s%g  %s%g\n    %s%g\n    %s%i  %s%i\n    %s%i  %s%i\n",
			"imageHeight = ", myReport->imageHeight, 
			"imageWidth = ", myReport->imageWidth, 
			"imageTimestamp = ", myReport->imageTimestamp, 
			"center_mass_x = ", myReport->center_mass_x, 
			"center_mass_y = ", myReport->center_mass_y, 
			"center_mass_x_normalized = ", myReport->center_mass_x_normalized, 
			"center_mass_y_normalized = ", myReport->center_mass_y_normalized, 
			"particleArea = ", myReport->particleArea, 
			"boundingRectangleTop = ", myReport->boundingRect.top, 
			"boundingRectangleLeft = ", myReport->boundingRect.left,
			"boundingRectangleHeight = ", myReport->boundingRect.height, 
			"boundingRectangleWidth = ", myReport->boundingRect.width);

	dprintf(LOG_INFO, "quality statistics:  \n    %s%g %s%g \n",
			"particleToImagePercent = ", myReport->particleToImagePercent, 
			"particleQuality = ", myReport->particleQuality);
}

/**
 * Print color report
 * @param myReport Report to print
 */
void PrintReport(ColorReport* myReport)
{
	dprintf(LOG_INFO, "particle ranges for %i particles:     ",
			"numberParticlesFound = ", myReport->numberParticlesFound);
	;
	dprintf(LOG_INFO, "\n    %s%f  %s%f  %s%f\n    %s%f %s%f  %s%f\n    %s%f  %s%f  %s%f\n -------",
			"particleHueMax = ", myReport->particleHueMax, 
			"particleHueMin = ", myReport->particleHueMin, 
			"particleHueMean = ", myReport->particleHueMean,
			"particleSatMax = ", myReport->particleSatMax, 
			"particleSatMin = ", myReport->particleSatMin, 
			"particleSatMean = ", myReport->particleSatMean,
			"particleLumMax = ", myReport->particleLumMax, 
			"particleLumMin = ", myReport->particleLumMin, 
			"particleLumMean = ", myReport->particleLumMean);

}

/**
 * Print color report
 * @param myReport Report to print
 */
void PrintReport(TrackingThreshold* myReport)
{
	dprintf(LOG_INFO, "name of color: %s", myReport->name);

	dprintf(LOG_INFO, "\n    %s%i  %s%i\n    %s%i %s%i\n    %s%i  %s%i\n -------",
			"hueMin = ", myReport->hue.minValue, 
			"hueMax = ", myReport->hue.maxValue, 
			"satMin = ", myReport->saturation.minValue, 
			"satMax = ", myReport->saturation.maxValue, 
			"lumMin = ", myReport->luminance.minValue, 
			"lumMax = ", myReport->luminance.maxValue );

}



