/********************************************************************************
*  Project   		: FIRST Motor Controller
*  File Name  		: AxisCamera.h          
*  Contributors   	: ELF
*  Creation Date 	: August 12, 2008
*  Revision History	: Source code & revision history maintained at sourceforge.WPI.edu    
*  File Description	: Globally defined values for the FRC Camera API
* 
*  API: Because nivision.h uses C++ style comments, any file including this
*  must be a .cpp instead of .c.
* 
*/
/*----------------------------------------------------------------------------*/
/*        Copyright (c) FIRST 2008.  All Rights Reserved.                     */
/*  Open Source Software - may be modified and shared by FRC teams. The code  */
/*  must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib. */
/*----------------------------------------------------------------------------*/

#ifndef __AXISCAMERA_H__
#define __AXISCAMERA_H__

#include "nivision.h"

/** port for communicating with camera */
#define CAMERA_PORT 80
/** how old an image is before it's discarded */
#define CAMERA_IMAGE_STALE_TIME_SEC 2.0  
/** time to wait for a new image in blocking call */
#define MAX_BLOCKING_TIME_SEC 0.5

/*  Enumerated Types */
/** @brief Counters for camera metrics */
enum FrcvCameraMetric {CAM_STARTS, CAM_STOPS, 
	CAM_NUM_IMAGE, CAM_BUFFERS_WRITTEN, CAM_BLOCKING_COUNT,
	
	CAM_SOCKET_OPEN, CAM_SOCKET_INIT_ATTEMPTS, CAM_BLOCKING_TIMEOUT,
	CAM_GETIMAGE_SUCCESS, CAM_GETIMAGE_FAILURE, 
	
	CAM_STALE_IMAGE, CAM_GETIMAGE_BEFORE_INIT, CAM_GETIMAGE_BEFORE_AVAILABLE,
	CAM_READ_JPEG_FAILURE, CAM_PID_SIGNAL_ERR, 
	
	CAM_BAD_IMAGE_SIZE, CAM_HEADER_ERROR};

#define CAM_NUM_METRICS 17

/** Private NI function needed to write to the VxWorks target */
IMAQ_FUNC int Priv_SetWriteFileAllowed(uint32_t enable); 

/**
@brief Possible image sizes that you can set on the camera.
*/
enum ImageResolution { k640x480, k320x240, k160x120 };

/**
@brief Possible rotation values that you can set on the camera.
*/
enum ImageRotation { ROT_0 = 0, ROT_180 = 180 };



int StartCameraTask();

extern "C" {
/*  Image Acquisition functions */
/* obtains an image from the camera server */
int GetImage(Image* cameraImage, double *timestamp);
int GetImageBlocking(Image* cameraImage, double *timestamp, double lastImageTimestamp);
/* obtains raw image string to send to PC */
int GetImageData(char** imageData, int* numBytes, double* currentImageTimestamp);
int GetImageDataBlocking(char** imageData, int* numBytes, double* timestamp, double lastImageTimestamp);

/* start the camera server */
void StartImageAcquisition();
void StopImageAcquisition();
void StartImageSignal(int taskId);

/* status & metrics */
int frcCameraInitialized();
int GetCameraMetric(FrcvCameraMetric metric);

/* camera configuration */
int ConfigureCamera(char *configString);
int GetCameraSetting(char *configString, char *cameraResponse);
int GetImageSetting(char *configString, char *cameraResponse);

/* camera task control */

int StartCameraTask(int frames, int compression, ImageResolution resolution, ImageRotation rotation);
int StopCameraTask();
}
#endif

