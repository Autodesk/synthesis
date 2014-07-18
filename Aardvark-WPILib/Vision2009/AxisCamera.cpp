
/********************************************************************************
*  Project   		: FIRST Motor Controller
*  File Name  		: AxisCamera.cpp        
*  Contributors 	: TD, ELF, JDG, SVK
*  Creation Date 	: July 29, 2008
*  Revision History	: Source code & revision history maintained at sourceforge.WPI.edu    
*  File Description	: Axis camera access for the FIRST Vision API
*      The camera task runs as an independent thread 
*/    
/*----------------------------------------------------------------------------*/
/*        Copyright (c) FIRST 2008.  All Rights Reserved.                     */
/*  Open Source Software - may be modified and shared by FRC teams. The code  */
/*  must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib. */
/*----------------------------------------------------------------------------*/                   


#include "errno.h"
#include "signal.h"
#include <string>
#include "time.h"

#include "AxisCamera.h" 
#include "BaeUtilities.h"
#include "FrcError.h"
#include "OSAL/Task.h"
#include "Timer.h"
#include "VisionAPI.h"

#include <cstring>
#include <cstdio>

/** packet size */
#define DEFAULT_PACKET_SIZE 512

/** Private NI function to decode JPEG */ 
IMAQ_FUNC int Priv_ReadJPEGString_C(Image* _image, const unsigned char* _string, uint32_t _stringLength); 

// To locally enable debug printing: set AxisCamera_debugFlag to a 1, to disable set to 0
int AxisCamera_debugFlag = 0;
#define DPRINTF if(AxisCamera_debugFlag)dprintf

/** @brief Camera data to be accessed globally */
struct {
	int readerPID; // Set to taskID for signaling
	int index; /* -1,0,1 */
	int	acquire; /* 0:STOP CAMERA; 1:START CAMERA */
	int cameraReady;  /* 0: CAMERA NOT INITIALIZED; 1: CAMERA INITIALIZED */
	int	decode; /* 0:disable decoding; 1:enable decoding to HSL Image */
	struct {
		//
		// To find the latest image timestamp, access:
		// globalCamera.data[globalCamera.index].timestamp
		//
		double timestamp;       // when image was taken
		char*	cameraImage;    // jpeg image string
		int cameraImageSize;    // image size
		Image* decodedImage;    // image decoded to NI Image object
		int decodedImageSize;   // size of decoded image
	}data[2];
	int cameraMetrics[CAM_NUM_METRICS];
}globalCamera;

/* run flag */
static short cont = 0;

/**
* @brief Get the most recent camera image.
* Supports IMAQ_IMAGE_RGB and IMAQ_IMAGE_HSL.
* @param image Image to return to; image must have been first created using frcCreateImage. 
* When you are done, use frcDispose.
* @param timestamp Timestamp to return; will record the time at which the image was stored.
* @param lastImageTimestamp Input - timestamp of last image; prevents serving of stale images
* @return 0 is failure, 1 is success
* @sa frcCreateImage(), frcDispose()
*/
int GetImageBlocking(Image* image, double *timestamp, double lastImageTimestamp)
{
	//char funcName[]="GetImageBlocking";
	//int success;
	//double startTime = GetTime();
	//
	//while (1)
	//{
	//	success = GetImage(image, timestamp);
	//	if (!success) return (success);
	//	
	//	if (*timestamp > lastImageTimestamp)
	//		return (1); // GOOD IMAGE RETURNED

	//	if (GetTime() > (startTime + MAX_BLOCKING_TIME_SEC))
	//	{
	//		imaqSetError(ERR_CAMERA_BLOCKING_TIMEOUT, funcName);
	//		globalCamera.cameraMetrics[CAM_BLOCKING_TIMEOUT]++;
	//		return (0); // NO IMAGE AVAILABLE WITHIN specified time
	//	}
	//	globalCamera.cameraMetrics[CAM_BLOCKING_COUNT]++;
	//	taskDelay (1);
	//}
	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Verifies that the camera is initialized
* @return 0 for failure, 1 for success
*/
int CameraInitialized()
{
	//char funcName[]="CameraInitialized";
	//int success = 0;
	///* check to see if camera is initialized */
	//if (!globalCamera.cameraReady)  {
	//	imaqSetError(ERR_CAMERA_NOT_INITIALIZED, funcName);
	//	DPRINTF (LOG_DEBUG, "Camera request before camera is initialized");
	//	globalCamera.cameraMetrics[CAM_GETIMAGE_BEFORE_INIT]++;
	//	globalCamera.cameraMetrics[CAM_GETIMAGE_FAILURE]++;
	//	return success;
	//}
	//
	//if (globalCamera.index == -1){
	//	imaqSetError(ERR_CAMERA_NO_BUFFER_AVAILABLE, funcName);
	//	DPRINTF (LOG_DEBUG, "No camera image available");
	//	globalCamera.cameraMetrics[CAM_GETIMAGE_BEFORE_AVAILABLE]++;
	//	globalCamera.cameraMetrics[CAM_GETIMAGE_FAILURE]++;
	//	return success;
	//}
	//return 1;

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Gets the most recent camera image, as long as it is not stale.
* Supported image types: IMAQ_IMAGE_RGB, IMAQ_IMAGE_HSL
* @param image Image to return, must have first been created with frcCreateImage or imaqCreate. 
* When you finish with the image, call frcDispose() to dispose of it.
* @param timestamp Returned timestamp of when the image was taken from the camera
* @return failure = 0, success = 1
*/
int GetImage(Image* image, double *timestamp)
{
	//char funcName[]="GetImage";
	//int success = 0;
	//int readIndex;
	//int	readCount = 10;
	//double currentTime = time(NULL);
	//double currentImageTimestamp;

	///* check to see if camera is initialized */
	//
	//if (!CameraInitialized()) {return success;}
	//
	///* try readCount times to get an image */
	//while (readCount) {
	//	readIndex = globalCamera.index;
	//	if (!imaqDuplicate(image, globalCamera.data[readIndex].decodedImage)) {
	//		int errorCode = GetLastVisionError(); 
	//		DPRINTF (LOG_DEBUG,"Error duplicating image= %i  %s ", errorCode, GetVisionErrorText(errorCode));			
	//	}
	//	// save the timestamp to check before returning
	//	currentImageTimestamp = globalCamera.data[readIndex].timestamp;
	//	
	//	// make sure this buffer is not being written to now
	//	if (readIndex == globalCamera.index) break;
	//	readCount--;
	//}
	//
	///* were we successful ? */
	//if (readCount){
	//	success = 1;
	//	if (timestamp != NULL)
	//		  *timestamp = currentImageTimestamp; // Return image timestamp	  
	//} else{
	//	globalCamera.cameraMetrics[CAM_GETIMAGE_FAILURE]++;
	//}
	//
	///* Ensure the buffered image is not too old - set this "stale time" above */
	//if (currentTime > globalCamera.data[globalCamera.index].timestamp + CAMERA_IMAGE_STALE_TIME_SEC){
	//	DPRINTF (LOG_CRITICAL, "STALE camera image (THIS COULD BE A BAD IMAGE)");
	//	imaqSetError(ERR_CAMERA_STALE_IMAGE, funcName);
	//	globalCamera.cameraMetrics[CAM_STALE_IMAGE]++;
	//	globalCamera.cameraMetrics[CAM_GETIMAGE_FAILURE]++;
	//	success = 0;
	//}
	//globalCamera.cameraMetrics[CAM_GETIMAGE_SUCCESS]++;
	//return success;

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Method to get a raw image from the buffer
* @param imageData returned image data
* @param numBytes returned number of bytes in buffer
* @param currentImageTimestamp returned buffer time of image data
* @return 0 if failure; 1 if success
*/
int GetImageData(char** imageData, int* numBytes, double* currentImageTimestamp)
{
	//int success = 0;
	//int readIndex;
	//int	readCount = 10;
	//int cameraImageSize = 0;
	//char *cameraImageString = NULL;

	///* check to see if camera is initialized */
	//		
	//if (!CameraInitialized()) {return success;}
	//		
	///* try readCount times to get an image */
	//while (readCount) {
	//	readIndex = globalCamera.index;
	//	cameraImageSize = globalCamera.data[readIndex].cameraImageSize;
	//	//cameraImageString = (Image *) malloc(cameraImageSize);
	//	cameraImageString = new char[cameraImageSize];
	//	if (NULL == cameraImageString) {
	//				DPRINTF (LOG_DEBUG, "Unable to allocate cameraImage");
	//				globalCamera.cameraMetrics[CAM_GETIMAGE_FAILURE]++;
	//				return success;
	//	}
	//	memcpy (cameraImageString, globalCamera.data[readIndex].cameraImage, cameraImageSize);
	//	*currentImageTimestamp = globalCamera.data[readIndex].timestamp;
	//	// make sure this buffer is not being written to now
	//	if (readIndex == globalCamera.index) break;
	//	free (cameraImageString);
	//	readCount--;
	//}
	//if (readCount){
	//	*imageData = cameraImageString;
	//	*numBytes = cameraImageSize;
	//	return 1;
	//}		
	//return (OK);

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Blocking call to get images for PC.
* This should be called from a separate task to maintain camera read performance. 
* It is intended to be used for sending raw (undecoded) image data to the PC.
* @param imageData image data to return
* @param numBytes number of bytes in buffer
* @param timestamp timestamp of buffer returned
* @param lastImageTimestamp buffer time of last image data sent to PC
* @return 0 if failure; 1 if success
*/
int GetImageDataBlocking(char** imageData, int* numBytes, double* timestamp, double lastImageTimestamp)
{

	//char funcName[]="GetImageDataBlocking";
	//int success;
	//double startTime = GetTime();
	//
	//   *imageData = NULL;
	//while (1)
	//{
	//	success = GetImageData(imageData, numBytes, timestamp);
	//	if (!success) return (success);
	//	
	//	if (*timestamp > lastImageTimestamp)
	//		return (1); // GOOD IMAGE DATA RETURNED

	//       delete *imageData;
	//       *imageData = NULL;

	//	if (GetTime() > (startTime + MAX_BLOCKING_TIME_SEC))
	//	{
	//		imaqSetError(ERR_CAMERA_BLOCKING_TIMEOUT, funcName);
	//		return (0); // NO IMAGE AVAILABLE WITHIN specified time
	//	}
	//	globalCamera.cameraMetrics[CAM_BLOCKING_COUNT]++;
	//	taskDelay (1);
	//}

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Accessor for camera instrumentation data
* @param the counter queried
* @return the counter value
*/
int GetCameraMetric(FrcvCameraMetric metric)
{	
	//return globalCamera.cameraMetrics[metric];

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Close socket & report error
* @param errstring String to print
* @param socket Socket to close
* @return error
*/
int CameraCloseSocket(const char *errstring, int socket)
{
	//DPRINTF (LOG_CRITICAL, "Closing socket - CAMERA ERROR: %s", errstring );
	//close (socket);
	//return (ERROR);

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}


/**
* @brief Reads one line from the TCP stream.
* @param camSock         The socket.
* @param buffer          A buffer with bufSize allocated for it. 
*                        On return, bufSize-1 amount of data or data upto first line ending
*                        whichever is smaller, null terminated.
* @param bufSize         The size of buffer.
* @param stripLineEnding If true, strips the line ending chacters from the buffer before return.
* @return 0 if failure; 1 if success
*/
static int CameraReadLine(int camSock, char* buffer, int bufSize, bool stripLineEnding) {
	//char funcName[]="CameraReadLine";
	//   // Need at least 3 bytes in the buffer to pull this off.
	//if (bufSize < 3) {
	//	imaqSetError(ERR_CAMERA_FAILURE, funcName);
	//	return 0;
	//}
	//   //  Reduce size by 1 to allow for null terminator.
	//   --bufSize;
	//   //  Read upto bufSize characters.
	//   for (int i=0;i < bufSize;++i, ++buffer) {
	//       //  Read one character.
	//       if (read (camSock, buffer, 1) <= 0) {
	//   		imaqSetError(ERR_CAMERA_FAILURE, funcName);
	//           return 0;
	//       }
	//       //  Line endings can be "\r\n" or just "\n". So always 
	//       //  look for a "\n". If you got just a "\n" and 
	//       //  stripLineEnding is false, then convert it into a \r\n
	//       //  because callers expect a \r\n.
	//       //  If the combination of the previous character and the current character
	//       //  is "\r\n", the line ending
	//       if (*buffer=='\n') {
	//           //  If asked to strip the line ending, then set the buffer to the previous 
	//           //  character.
	//           if (stripLineEnding) {
	//               if (i > 0 && *(buffer-1)=='\r') {
	//                   --buffer;
	//               }
	//           }
	//           else {
	//               //  If the previous character was not a '\r', 
	//               if (i == 0 || *(buffer-1)!='\r') {
	//                   //  Make the current character a '\r'.
	//                   *buffer = '\r';
	//                   //  If space permits, add back the '\n'.
	//                   if (i < bufSize-1) {
	//                       ++buffer;
	//                       *buffer = '\n';
	//                   }
	//               }
	//               //  Set the buffer past the current character ('\n')
	//               ++buffer;
	//           }
	//           break;
	//       }
	//   }
	//   //  Null terminate.
	//   *buffer = '\0';
	//   return 1;

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
@brief Skips read data until the first empty line.

@param camSock An open tcp socket to the camera to read the data from.
@return sucess 0 if failure; 1 if success
*/
static int CameraSkipUntilEmptyLine(int camSock) {
	//char buffer[1024];
	//int success = 0;
	//while(1) {
	//    success = CameraReadLine(camSock, buffer, sizeof(buffer), true);
	//    if (*buffer == '\0') {
	//        return success;
	//    }
	//}
	//return success;

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
@brief Opens a socket.

Issues the given http request with the required added information 
and authentication. It  cycles through an array of predetermined 
encrypted username, password combinations that we expect the users 
to have at any point in time. If none of the username, password 
combinations work, it outputs a "Unknown user or password" error.
If the request succeeds, it returns the socket number.

@param serverName The information about the host from which this request originates
@param request   The request to send to the camera not including boilerplate or 
authentication. This is usually in the form of "GET <string>"
@return int - failure = ERROR; success = socket number;     
*/
static int CameraOpenSocketAndIssueAuthorizedRequest(const char* serverName, const char* request) 
{
	//   char funcName[]="cameraOpenSocketAndIssueAuthorizedRequest";
	//
	//struct sockaddr_in cameraAddr;
	//int sockAddrSize;  
	//int camSock = ERROR;    

	//   // The camera is expected to have one of the following username, password combinations.
	//   // This routine will return an error if it does not find one of these.
	//   static const char* authenticationStrings[] = {
	//       "RlJDOkZSQw==",     /* FRC, FRC */
	//       "cm9vdDpwYXNz",     /* root, admin*/
	//       "cm9vdDphZG1pbg=="  /* root, pass*/
	//   };

	//   static const int numAuthenticationStrings = sizeof(authenticationStrings)/sizeof(authenticationStrings[0]);
	//
	//static const char *requestTemplate = "%s "                              \
	//                                        "HTTP/1.1\n"                       \
	//                                        "User-Agent: HTTPStreamClient\n"   \
	//                                        "Connection: Keep-Alive\n"         \
	//                                        "Cache-Control: no-cache\n"        \
	//                                        "Authorization: Basic %s\n\n";

	//int i = 0;
	//   for (;i < numAuthenticationStrings;++i) {
	//       char buffer[1024];

	//       sprintf_s(buffer, requestTemplate, request, authenticationStrings[i]);

	//       /* create camera socket */
	//       //DPRINTF (LOG_DEBUG, "creating camSock" ); 
	//       if ((camSock = socket (AF_INET, SOCK_STREAM, 0)) == ERROR) {
	//   		imaqSetError(ERR_CAMERA_SOCKET_CREATE_FAILED, funcName);
	//   		perror("Failed to create socket");
	//   		return (ERROR);
	//       }

	//       sockAddrSize = sizeof (struct sockaddr_in);
	//       bzero ((char *) &cameraAddr, sockAddrSize);
	//       cameraAddr.sin_family = AF_INET;
	//       cameraAddr.sin_len = (u_char) sockAddrSize;
	//       cameraAddr.sin_port = htons (CAMERA_PORT);

	//       if (( (int)(cameraAddr.sin_addr.s_addr = inet_addr (const_cast<char*>(serverName)) ) == ERROR) &&
	//           ( (int)(cameraAddr.sin_addr.s_addr = hostGetByName (const_cast<char*>(serverName)) ) == ERROR)) 
	//       {
	//   		imaqSetError(ERR_CAMERA_CONNECT_FAILED, funcName);
	//           return CameraCloseSocket("Failed to get IP, check hostname or IP", camSock);
	//       }

	//       //DPRINTF (LOG_INFO, "connecting camSock" ); 
	//       if (connect (camSock, (struct sockaddr *) &cameraAddr, sockAddrSize) == ERROR) 	{
	//   		imaqSetError(ERR_CAMERA_CONNECT_FAILED, funcName);
	//           return CameraCloseSocket("Failed to connect to camera - check networ", camSock);
	//       }

	//       //DPRINTF (LOG_DEBUG, "writing GET request to camSock" ); 
	//       if (write (camSock, buffer, strlen(buffer) ) == ERROR) {
	//   		imaqSetError(ERR_CAMERA_CONNECT_FAILED, funcName);
	//           return CameraCloseSocket("Failed to send GET request", camSock);
	//       }

	//       //  Read one line with the line ending removed.
	//       if (!CameraReadLine(camSock, buffer, 1024, true)) {
	//           return CameraCloseSocket("Bad response to GET request", camSock);
	//       }

	//       //  Check if the response is of the format HTTP/<version> 200 OK.
	//       float discard;
	//       if (sscanf(buffer, "HTTP/%f 200 OK", &discard) == 1) {
	//           break;
	//       }

	//       //  We have to close the connection because in the case of failure
	//       //  the server closes the connection.
	//       close(camSock);
	//   }
	//   //  If none of the attempts were successful, then let the caller know.
	//   if (numAuthenticationStrings == i) {
	//	imaqSetError(ERR_CAMERA_AUTHORIZATION_FAILED, funcName);
	//       fprintf(stderr, "Expected username/password combination not found on camera");
	//       return ERROR;
	//   }
	//   return camSock;

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}


/**
* @brief Sends a configuration message to the camera
* @param configString configuration message to the camera
* @return success: 0=failure; 1=success
*/
int ConfigureCamera(char *configString){
	//char funcName[]="ConfigureCamera";
	//const char *serverName = "192.168.0.90";		/* camera @ */
	//int success = 0;
	//int camSock = 0;    
	//
	///* Generate camera configuration string */
	//const char * getStr1 =
	//	"GET /axis-cgi/admin/param.cgi?action=update&ImageSource.I0.Sensor.";
	//
	//char cameraRequest[strlen(getStr1) + strlen(configString)];
	//   sprintf (cameraRequest, "%s%s",	getStr1, configString);
	//DPRINTF(LOG_DEBUG, "camera configuration string: \n%s", cameraRequest);
	//camSock = CameraOpenSocketAndIssueAuthorizedRequest(serverName, cameraRequest);
	//DPRINTF(LOG_DEBUG, "camera socket# = %i", camSock);
	//
	//   //read response
	//   success = CameraSkipUntilEmptyLine(camSock);
	////DPRINTF(LOG_DEBUG, "succcess from CameraSkipUntilEmptyLine: %i", success);
	//   char buffer[3];	// set property - 3
	//   success = CameraReadLine(camSock, buffer, 3, true);
	////DPRINTF(LOG_DEBUG, "succcess from CameraReadLine: %i", success);
	//DPRINTF(LOG_DEBUG, "line read from camera \n%s", buffer);
	//   if (strcmp(buffer, "OK") != 0) {
	//	imaqSetError(ERR_CAMERA_COMMAND_FAILURE, funcName);
	//	DPRINTF(LOG_DEBUG, "setting ERR_CAMERA_COMMAND_FAILURE - OK not found");
	//   }
	//DPRINTF (LOG_INFO, "\nConfigureCamera ENDING  success = %i\n", success );	

	///* clean up */
	//close (camSock);
	//return (1);

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}


/**
* @brief Sends a request message to the camera
* @param configString request message to the camera
* @param cameraResponse response from camera
* @return success: 0=failure; 1=success
*/
int GetCameraSetting(char *configString, char *cameraResponse){
	//const char *serverName = "192.168.0.90";		/* camera @ */
	//int success = 0;
	//int camSock = 0;    
	//
	///* Generate camera request string */
	//const char * getStr1 =
	//	"GET /axis-cgi/admin/param.cgi?action=list&group=ImageSource.I0.Sensor.";
	//char cameraRequest[strlen(getStr1) + strlen(configString)];
	//   sprintf (cameraRequest, "%s%s",	getStr1, configString);
	//DPRINTF(LOG_DEBUG, "camera configuration string: \n%s", cameraRequest);
	//camSock = CameraOpenSocketAndIssueAuthorizedRequest(serverName, cameraRequest);
	//DPRINTF(LOG_DEBUG, "return from CameraOpenSocketAndIssueAuthorizedRequest %i", camSock);
	//
	//   //read response
	//   success = CameraSkipUntilEmptyLine(camSock);
	//   success = CameraReadLine(camSock, cameraResponse, 1024, true);
	//DPRINTF(LOG_DEBUG, "succcess from CameraReadLine: %i", success);
	//DPRINTF(LOG_DEBUG, "line read from camera \n%s", cameraResponse);
	//DPRINTF (LOG_INFO, "\nGetCameraSetting ENDING  success = %i\n", success );	

	///* clean up */
	//close (camSock);
	//return (1);

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Sends a request message to the camera for image appearance property
* (resolution, compression, rotation)
* @param configString request message to the camera
* @param cameraResponse response from camera
* @return success: 0=failure; 1=success
*/
int GetImageSetting(char *configString, char *cameraResponse){
	//const char *serverName = "192.168.0.90";		/* camera @ */
	//int success = 0;
	//int camSock = 0;    
	//
	///* Generate camera request string */
	//const char *getStr1 = "GET /axis-cgi/admin/param.cgi?action=list&group=Image.I0.Appearance.";
	//char cameraRequest[strlen(getStr1) + strlen(configString)];
	//   sprintf (cameraRequest, "%s%s",	getStr1, configString);
	//DPRINTF(LOG_DEBUG, "camera configuration string: \n%s", cameraRequest);
	//camSock = CameraOpenSocketAndIssueAuthorizedRequest(serverName, cameraRequest);
	//DPRINTF(LOG_DEBUG, "return from CameraOpenSocketAndIssueAuthorizedRequest %i", camSock);
	//
	//   //read response
	//   success = CameraSkipUntilEmptyLine(camSock);
	//   success = CameraReadLine(camSock, cameraResponse, 1024, true);
	//DPRINTF(LOG_DEBUG, "succcess from CameraReadLine: %i", success);
	//DPRINTF(LOG_DEBUG, "line read from camera \n%s", cameraResponse);
	//DPRINTF (LOG_INFO, "\nGetCameraSetting ENDING  success = %i\n", success );	

	///* clean up */
	//close (camSock);
	//return (1);

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}


#define MEASURE_SOCKET_TIME 1   

/**
* @brief Manage access to the camera. Sets up sockets and reads images
* @param frames Frames per second 
* @param compression Camera image compression 
* @param resolution Camera image size 
* @param rotation Camera image rotation 
* @return error
*/
int cameraJPEGServer(int frames, int compression, ImageResolution resolution, ImageRotation rotation)
{
	//	char funcName[]="cameraJPEGServer";
	//	char serverName[] = "192.168.0.90";		/* camera @ */
	//	cont = 1;
	//	int errorCode = 0;
	//	int printCounter = 0;
	//	int	writeIndex;
	//	int authorizeCount = 0;
	//	int authorizeConfirmed = 0;
	//	static const int authenticationStringsCount = 3;
	//    static const char* authenticationStrings[] = {
	//    		"cm9vdDphZG1pbg==", /* root, admin*/
	//    		"RlJDOkZSQw==",     /* FRC, FRC */
	//    		"cm9vdDpwYXNz=="    /* root, pass*/
	//    };
	//
	//	DPRINTF (LOG_DEBUG, "cameraJPEGServer" ); 
	//	
	//	struct sockaddr_in cameraAddr;
	//	int sockAddrSize;  
	//	int camSock = 0;    
	//
	//	char resStr[10];
	//	switch (resolution) {
	//		case k640x480: { sprintf_s(resStr,"640x480"); break; }
	//		case k320x240: { sprintf_s(resStr,"320x240"); break; }
	//		case k160x120: { sprintf_s(resStr,"160x120"); break; }
	//		default: {DPRINTF (LOG_DEBUG, "code error - resolution input" ); break; }
	//	}
	//	
	//	/* Generate camera initialization string */
	//	/* changed resolution to 160x120 from 320x240 */
	//	/* supported resolutions are: 640x480, 640x360, 320x240, 160x120 */	
	//	const char * getStr1 =
	//	"GET /axis-cgi/mjpg/video.cgi?showlength=1&camera=1&";	
	//		
	//	char insertStr[100];
	//	sprintf (insertStr, "des_fps=%i&compression=%i&resolution=%s&rotation=%i", 
	//			frames, compression, resStr, (int)rotation);	
	//	
	//	const char * getStr2 = " HTTP/1.1\n\
	//User-Agent: HTTPStreamClient\n\
	//Host: 192.150.1.100\n\
	//Connection: Keep-Alive\n\
	//Cache-Control: no-cache\n\
	//Authorization: Basic %s;\n\n";
	//
	//	char getStr[strlen(getStr1) + strlen(insertStr) + strlen(getStr2)];      
	//    sprintf (getStr, "%s:%s%s",	getStr1, insertStr, getStr2);
	//
	//	DPRINTF(LOG_DEBUG, "revised camera string: \n%s", getStr);
	//	/* Allocation */
	//	char tempBuffer[1024];
	//    
	//	RETRY:
	//	Wait(0.1);  //bug fix - don't pester camera if it's booting
	//	while (globalCamera.acquire == 0) Wait(0.1);
	//
	//	if (!authorizeConfirmed){
	//	  if (authorizeCount < authenticationStringsCount){
	//	    sprintf (tempBuffer, getStr, authenticationStrings[authorizeCount]);
	//	  } else {
	//		imaqSetError(ERR_CAMERA_AUTHORIZATION_FAILED, funcName);
	//		fprintf(stderr, "Camera authorization failed ... Incorrect password on camera!!");
	//		return (ERROR);
	//	  }
	//	}
	//
	//	while (1)
	//	{
	//	  globalCamera.cameraMetrics[CAM_SOCKET_INIT_ATTEMPTS]++;	  
	//
	//	  /* create camera socket */
	//	  DPRINTF (LOG_DEBUG, "creating camSock" ); 
	//	  if ((camSock = socket (AF_INET, SOCK_STREAM, 0)) == ERROR) {	
	//		imaqSetError(ERR_CAMERA_SOCKET_CREATE_FAILED, funcName);
	//		perror("Failed to create socket");
	//		cont = 0;
	//		return (ERROR);
	//	  }
	//
	//	  sockAddrSize = sizeof (struct sockaddr_in);
	//	  bzero ((char *) &cameraAddr, sockAddrSize);
	//	  cameraAddr.sin_family = AF_INET;
	//	  cameraAddr.sin_len = (u_char) sockAddrSize;
	//	  cameraAddr.sin_port = htons (CAMERA_PORT);
	//
	//	  DPRINTF (LOG_DEBUG, "getting IP" );
	//	  if (( (int)(cameraAddr.sin_addr.s_addr = inet_addr (serverName) ) == ERROR) &&
	//		( (int)(cameraAddr.sin_addr.s_addr = hostGetByName (serverName) ) == ERROR))
	//	  {	
	//		  CameraCloseSocket("Failed to get IP, check hostname or IP", camSock);
	//		continue;
	//	  }
	//	  
	//	  DPRINTF (LOG_INFO, "Attempting to connect to camSock" ); 
	//	  if (connect (camSock, (struct sockaddr *) &cameraAddr, sockAddrSize) == ERROR) 	{
	//		imaqSetError(ERR_CAMERA_CONNECT_FAILED, funcName);
	//		CameraCloseSocket("Failed to connect to camera - check network", camSock);
	//		continue;
	//	  }	  
	//
	//#if MEASURE_SOCKET_SETUP
	//	  socketEndTime = GetTime(); 
	//	  setupTime = socketEndTime - socketStartTime; 
	//	  printf("\n***socket setup time = %g\n", setupTime );
	//#endif	  
	//	  
	//	  globalCamera.cameraMetrics[CAM_SOCKET_OPEN]++;
	//	  break;
	//	} // end while (trying to connect to camera)
	//
	//	DPRINTF (LOG_DEBUG, "writing GET request to camSock" ); 
	//	if (write (camSock, tempBuffer , strlen(tempBuffer) ) == ERROR) {
	//		return CameraCloseSocket("Failed to send GET request", camSock);
	//	}
	//
	//	//DPRINTF (LOG_DEBUG, "reading header" ); 
	//	/* Find content-length, then read that many bytes */
	//	int counter = 2;
	//	const char* contentString = "Content-Length: ";
	//	const char* authorizeString = "200 OK";
	//	
	//#define MEASURE_TIME 0
	//#if MEASURE_TIME
	//	//timing parameters - only measure one at the time
	//	double loopStartTime = 0.0; // measuring speed of execution loop
	//	double loopEndTime = 0.0;
	//	double cameraStartTime = 0.0;
	//	double cameraEndTime = 0.0;
	//	double previousStartTime = 0.0;
	//	int performanceLoopCounter = 0;
	//	int maxCount = 30;
	//#endif
	//	
	//	while (cont) {
	//#if MEASURE_TIME
	//		previousStartTime = loopStartTime;  // first time is bogus
	//		loopStartTime = GetTime(); 
	//#endif	
	//		// If camera has been turned OFF, jump to RETRY
	//		//if (globalCamera.acquire == 0) goto RETRY;
	//		
	//		/* Determine writer index */
	//		if (globalCamera.index == 0)
	//			writeIndex = 1;
	//		else
	//			writeIndex = 0;
	//		
	//		/* read header */
	//		//TODO: check for error in header, increment ERR_CAMERA_HEADER_ERROR
	//		char initialReadBuffer[DEFAULT_PACKET_SIZE] = "";
	//		char intermediateBuffer[1];
	//		char *trailingPtr = initialReadBuffer;
	//		int trailingCounter = 0;
	//		
	//
	//#if MEASURE_TIME
	//		cameraStartTime = GetTime(); 
	//#endif	
	//
	//		while (counter) {
	//			if (read (camSock, intermediateBuffer, 1) <= 0) {
	//				CameraCloseSocket("Failed to read image header", camSock);
	//				globalCamera.cameraMetrics[ERR_CAMERA_HEADER_ERROR]++;
	//				goto RETRY;
	//			}
	//
	//			strncat(initialReadBuffer, intermediateBuffer, 1);
	//			if (NULL != strstr(trailingPtr, "\r\n\r\n")) {
	//
	//				  if (!authorizeConfirmed){
	//
	//					  if (strstr(initialReadBuffer, authorizeString))
	//					  {
	//						  authorizeConfirmed = 1;
	//						  /* set camera to initialized */
	//						  globalCamera.cameraReady = 1; 
	//					  }
	//					  else
	//					  {
	//						  CameraCloseSocket("Not authorized to connect to camera", camSock);
	//						  authorizeCount++;
	//				  goto RETRY;
	//					  }
	//				}
	//				--counter;
	//			}
	//			if (++trailingCounter >= 4) {
	//				trailingPtr++;
	//			}
	//		}
	//	
	//		counter = 1;
	//		char *contentLength = strstr(initialReadBuffer, contentString);
	//		if (contentLength == NULL) {
	//			globalCamera.cameraMetrics[ERR_CAMERA_HEADER_ERROR]++;
	//			CameraCloseSocket("No content-length token found in packet", camSock);
	//			goto RETRY;
	//		}
	//		/* get length of image content */
	//		contentLength = contentLength + strlen(contentString);
	//		globalCamera.data[writeIndex].cameraImageSize = atol (contentLength);
	//		
	//		if(globalCamera.data[writeIndex].cameraImage)
	//			free(globalCamera.data[writeIndex].cameraImage);
	//		//globalCamera.data[writeIndex].cameraImage = (Image *) malloc(globalCamera.data[writeIndex].cameraImageSize);
	//		globalCamera.data[writeIndex].cameraImage = (char*)malloc(globalCamera.data[writeIndex].cameraImageSize);
	//		if (NULL == globalCamera.data[writeIndex].cameraImage) {
	//			return CameraCloseSocket("Failed to allocate space for imageString", camSock);
	//		}
	//		globalCamera.cameraMetrics[CAM_BUFFERS_WRITTEN]++;
	//		
	//		//
	//		// This is a blocking camera read function, and will block if the camera
	//		// has been disconnected from the cRIO.  If however the camera is
	//		// POWERED OFF while connected to the cRIO, this function NEVER RETURNS
	//		//
	//		int bytesRead = fioRead (camSock, (char *)globalCamera.data[writeIndex].cameraImage,
	//				globalCamera.data[writeIndex].cameraImageSize);
	//
	//#if MEASURE_TIME
	//		cameraEndTime = GetTime(); 
	//#endif	
	//		
	//		//DPRINTF (LOG_DEBUG, "Completed fioRead function - bytes read:%d", bytesRead);
	//		if (bytesRead <= 0) {
	//			CameraCloseSocket("Failed to read image data", camSock);
	//			goto RETRY;
	//		} else if (bytesRead != globalCamera.data[writeIndex].cameraImageSize){
	//			fprintf(stderr, "ERROR: Failed to read entire image: readLength does not match bytes read");
	//			globalCamera.cameraMetrics[CAM_BAD_IMAGE_SIZE]++;
	//		}
	//		// if decoding the JPEG to an HSL Image, do it here
	//		if (globalCamera.decode) {
	//			if(globalCamera.data[writeIndex].decodedImage)
	//				frcDispose(globalCamera.data[writeIndex].decodedImage);
	//			globalCamera.data[writeIndex].decodedImage = frcCreateImage(IMAQ_IMAGE_HSL);
	//			if (! Priv_ReadJPEGString_C(globalCamera.data[writeIndex].decodedImage, 
	//					(const unsigned char *)globalCamera.data[writeIndex].cameraImage, 
	//					globalCamera.data[writeIndex].cameraImageSize) ) {
	//				DPRINTF (LOG_DEBUG, "failure creating Image");			
	//			}
	//		}
	//		
	//		// TODO: React to partial image
	//		globalCamera.data[writeIndex].timestamp = GetTime();
	//		globalCamera.index = writeIndex;
	//		
	//		/* signal a listening task */
	//		if (globalCamera.readerPID) {
	//			if (taskKill (globalCamera.readerPID,SIGUSR1) == OK)
	//			{
	//				DPRINTF (LOG_DEBUG, "SIGNALING PID= %i", globalCamera.readerPID);
	//			}
	//			else
	//			{
	//				globalCamera.cameraMetrics[CAM_PID_SIGNAL_ERR]++;
	//				DPRINTF (LOG_DEBUG, "ERROR SIGNALING PID= %i", globalCamera.readerPID);
	//			}
	//		}
	//
	//		globalCamera.cameraMetrics[CAM_NUM_IMAGE]++;	
	//		printCounter ++;
	//		if (printCounter == 1000) { 
	//			DPRINTF (LOG_DEBUG, "imageCounter = %i", globalCamera.cameraMetrics[CAM_NUM_IMAGE]); 
	//			printCounter=0; 
	//		}
	//		
	//		taskDelay(1);  
	//		
	//#if MEASURE_TIME
	//		loopEndTime = GetTime(); 
	//		performanceLoopCounter++;
	//		if (performanceLoopCounter <= maxCount) {
	//			DPRINTF (LOG_DEBUG, "%i DONE!!!: loop = ,%g,  camera = ,%g,  difference = ,%g, loopRate= ,%g,",
	//					performanceLoopCounter, loopEndTime-loopStartTime, cameraEndTime-cameraStartTime, 
	//					(loopEndTime-loopStartTime) - (cameraEndTime-cameraStartTime),
	//					loopStartTime-previousStartTime);						
	//		}
	//#endif	
	//	}  /* end while (cont) */
	//
	//	/* clean up */
	//	close (camSock);
	//	cont = 0;
	//	DPRINTF (LOG_INFO, "\nJPEG SERVER ENDING  errorCode = %i\n", errorCode );
	//	
	//	return (OK);

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Start signaling a task when new images are available
* @param taskID number for task to get the signal
*/
void StartImageSignal(int taskId) // Start issuing a SIGUSR1 signal to the specified taskId
{	
	//globalCamera.readerPID = taskId;

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Start serving images
*/
void StartImageAcquisition()
{	
	/*globalCamera.cameraMetrics[CAM_STARTS]++;  
	globalCamera.acquire = 1; 
	DPRINTF(LOG_DEBUG, "starting acquisition");*/

	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}


/**
* @brief Stop serving images
*/
void StopImageAcquisition()
{	
	/*globalCamera.cameraMetrics[CAM_STOPS]++;  globalCamera.acquire = 0;*/
	
	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}


/**
* @brief This is the routine that is run when the task is spawned
* It initializes the camera with the image settings passed in, and
* starts image acquisition.
* @param frames Frames per second 
* @param compression Camera image compression 
* @param resolution Camera image size 
* @param rotation Camera image rotation 
*/
static int initCamera(int frames, int compression, ImageResolution resolution, ImageRotation rotation) 
{
	////SetDebugFlag ( DEBUG_SCREEN_AND_FILE  ) ;

	//DPRINTF(LOG_DEBUG, "\n+++++ camera task starting: rotation = %i", (int)rotation);
	//int errorCode;

	///* Initialize globalCamera area 
	//* Set decode to 1 - always want to decode images for processing 
	//* If ONLY sending images to the dashboard, you could set it to 0 */
	//bzero ((char *)&globalCamera, sizeof(globalCamera));
	//globalCamera.index = -1;
	//globalCamera.decode = 1;

	///* allow writing to vxWorks target */
	//Priv_SetWriteFileAllowed(1); 

	///* start acquisition immediately */
	//StartImageAcquisition();

	///*  cameraJPEGServer runs until camera is stopped */
	//DPRINTF (LOG_DEBUG, "calling cameraJPEGServer" ); 
	//errorCode = cameraJPEGServer(frames, compression, resolution, rotation);	
	//DPRINTF (LOG_INFO, "errorCode from cameraJPEGServer = %i\n", errorCode ); 
	//return (OK);
	
	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

//Task g_axisCameraTask("Camera", initCamera);

/**
* @brief Start the camera task
* @param frames Frames per second 
* @param compression Camera image compression 
* @param resolution Camera image size 
* @param rotation Camera image rotation (ROT_0 or ROT_180)
* @return TaskID of camera task, or -1 if error.
*/
int StartCameraTask()
{
	//return StartCameraTask(10, 0, k160x120, ROT_0);
	
	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}
int StartCameraTask(int frames, int compression, ImageResolution resolution, ImageRotation rotation)
{
	//char funcName[]="startCameraTask";
	//DPRINTF(LOG_DEBUG, "starting camera");

	//int cameraTaskID = 0;

	////range check
	//if (frames < 1) frames = 1;
	//else if (frames > 30) frames = 30;
	//if (compression < 0) compression = 0;		
	//else if (compression > 100) compression = 100;

	//// stop any prior copy of running task
	//StopCameraTask(); 

	//// spawn camera task
	//bool started = g_axisCameraTask.Start(frames, compression, resolution, rotation);
	//cameraTaskID = g_axisCameraTask.GetID();
	//DPRINTF(LOG_DEBUG, "spawned task id %i", cameraTaskID);

	//if (!started)	{
	//	DPRINTF(LOG_DEBUG, "camera task failed to start");
	//	imaqSetError(ERR_CAMERA_TASK_SPAWN_FAILED, funcName);
	//	return -1;
	//}
	//return cameraTaskID;
	
	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

/**
* @brief Stops the camera task
* @return TaskID of camera task killed, or -1 if none was running.
*/
int StopCameraTask()
{
	//std::string taskName("FRC_Camera");    
	//// check for prior copy of running task
	//int oldTaskID = taskNameToId(const_cast<char*>(taskName.c_str()));
	//if(oldTaskID != ERROR) { taskDelete(oldTaskID);  }
	//return oldTaskID;
	
	fprintf(stderr, "Axis camera not implemented!\n");
	exit(1);
}

#if 0
/* if you want to run this task by itself to debug  
* enable this code and make RunProgram the entry point 
*/
extern "C"
{
	void RunProgram();
	int AxisCamera_StartupLibraryInit();
}
/** * @brief Start point of the program */
void RunProgram()
{	StartCameraTask();}

/** * @brief This is the main program that is run by the debugger or the robot on boot. */
int AxisCamera_StartupLibraryInit()
{		RunProgram();		return 0;	}

#endif



