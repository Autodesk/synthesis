/********************************************************************************
*  Project   		: FIRST Motor Controller
*  File Name  		: BaeUtilities.h          
*  Contributors   	: JDG, ELF
*  Creation Date 	: August 12, 2008
*  Revision History	: Source code & revision history maintained at sourceforge.WPI.edu      
*  File Description	: Globally defined values for utilities
*/
/*----------------------------------------------------------------------------*/
/*        Copyright (c) FIRST 2008.  All Rights Reserved.                     */
/*     Open Source Software - may be modified and shared by FRC teams.        */
/*   Must be accompanied by the BSD license file in $(WIND_BASE)/WPILib.      */
/*----------------------------------------------------------------------------*/

#ifndef __BAEUTILITIES_H__
#define __BAEUTILITIES_H__

/*  Constants */
#define LOG_DEBUG    __FILE__,__FUNCTION__,__LINE__,DEBUG_TYPE
#define LOG_INFO     __FILE__,__FUNCTION__,__LINE__,INFO_TYPE
#define LOG_ERROR    __FILE__,__FUNCTION__,__LINE__,ERROR_TYPE
#define LOG_CRITICAL __FILE__,__FUNCTION__,__LINE__,CRITICAL_TYPE
#define LOG_FATAL    __FILE__,__FUNCTION__,__LINE__,FATAL_TYPE
#define LOG_DEBUG    __FILE__,__FUNCTION__,__LINE__,DEBUG_TYPE

/*   Enumerated Types */

/** debug levels */
enum dprint_type {DEBUG_TYPE, INFO_TYPE, ERROR_TYPE, CRITICAL_TYPE, FATAL_TYPE};

/** debug output setting */
typedef enum DebugOutputType_enum { 
	DEBUG_OFF, DEBUG_MOSTLY_OFF, DEBUG_SCREEN_ONLY, DEBUG_FILE_ONLY, DEBUG_SCREEN_AND_FILE
}DebugOutputType;

/*  Enumerated Types */

/* Utility functions */

/* debug */
void SetDebugFlag ( DebugOutputType flag  ); 
void dprintf ( const char * tempString, ...  );  /* Variable argument list */

/* set FRC ranges for drive */
double RangeToNormalized(double pixel, int range);
/* change normalized value to any range - used for servo */
float NormalizeToRange(float normalizedValue, float minRange, float maxRange);
float NormalizeToRange(float normalizedValue);

/* system utilities */
void ShowActivity (char *fmt, ...);
double ElapsedTime (double startTime);

/* servo panning utilities */
class Servo;
double SinPosition (double *period, double sinStart);
void panInit();
void panInit(double period);
void panForTarget(Servo *panServo);
void panForTarget(Servo *panServo, double sinStart);

/* config file read utilities */
int processFile(char *inputFile, char *outputString, int lineNumber);
int emptyString(char *string);
void stripString(char *string);

#endif

