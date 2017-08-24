/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "Utility.h"

#include "NetworkCommunication/FRCComm.h"
#include "ChipObject.h"
#include "OSAL/Task.h"
#include <stdio.h>
#include "nivision.h"
#include <stdint.h>
#include <string.h>

#define DBG_DEMANGLE_PRINT_LEN 256  /* Num chars of demangled names to print */

char *wpi_getLabel(uint32_t addr, int32_t *found)
{
	return "TROLLLL";// Yeah no
}

static int32_t wpiStackTask(int32_t taskId)
{
	return 0;
}

void wpi_selfTrace()
{
}

static bool stackTraceEnabled = false;
static bool suspendOnAssertEnabled = false;

/**
* Enable Stack trace after asserts.
*/
void wpi_stackTraceOnAssertEnable(bool enabled)
{
	stackTraceEnabled = enabled;
}

/**
* Enable suspend on asssert.
* If enabled, the user task will be suspended whenever an assert fails. This
* will allow the user to attach to the task with the debugger and examine variables
* around the failure.
*/
void wpi_suspendOnAssertEnabled(bool enabled)
{
	suspendOnAssertEnabled = enabled;
}

static void wpi_handleTracing()
{
	if (stackTraceEnabled)
	{
		printf("\n-----------<Stack Trace>----------------\n");
		wpi_selfTrace();
	}
	printf("\n");
}

/**
* Assert implementation.
* This allows breakpoints to be set on an assert.
* The users don't call this, but instead use the wpi_assert macros in Utility.h.
*/
bool wpi_assert_impl(bool conditionValue, 
					 const char *conditionText,
					 const char *message,
					 const char *fileName,
					 uint32_t lineNumber, 
					 const char *funcName)
{
	if (!conditionValue)
	{   
		// Error string buffer
		char error[256];

		// If an error message was specified, include it
		// Build error string
		if(message != NULL) {
			sprintf(error, "Assertion failed: \"%s\", \"%s\" failed in %s() in %s at line %d\n",
				message, conditionText, funcName, fileName, lineNumber);
		} else {
			sprintf(error, "Assertion failed: \"%s\" in %s() in %s at line %d\n",
				conditionText, funcName, fileName, lineNumber);
		}

		// Print to console and send to remote dashboard
		printf("\n\n>>>>%s", error);
		setErrorData(error, strlen(error), 100);

		wpi_handleTracing();
	}
	return conditionValue;
}

/**
* Common error routines for wpi_assertEqual_impl and wpi_assertNotEqual_impl
* This should not be called directly; it should only be used by wpi_assertEqual_impl
* and wpi_assertNotEqual_impl.
*/
void wpi_assertEqual_common_impl(int valueA,
								 int valueB,
								 const char *equalityType,
								 const char *message,
								 const char *fileName,
								 uint32_t lineNumber, 
								 const char *funcName)
{
	// Error string buffer
	char error[256];

	// If an error message was specified, include it
	// Build error string
	if(message != NULL) {
		sprintf(error, "Assertion failed: \"%s\", \"%d\" %s \"%d\" in %s() in %s at line %d\n",
			message, valueA, equalityType, valueB, funcName, fileName, lineNumber);
	} else {
		sprintf(error, "Assertion failed: \"%d\" %s \"%d\" in %s() in %s at line %d\n",
			valueA, equalityType, valueB, funcName, fileName, lineNumber);
	}

	// Print to console and send to remote dashboard
	printf("\n\n>>>>%s", error);
	setErrorData(error, strlen(error), 100);

	wpi_handleTracing();
}

/**
* Assert equal implementation.
* This determines whether the two given integers are equal. If not,
* the value of each is printed along with an optional message string.
* The users don't call this, but instead use the wpi_assertEqual macros in Utility.h.
*/
bool wpi_assertEqual_impl(int valueA,
						  int valueB,
						  const char *message,
						  const char *fileName,
						  uint32_t lineNumber, 
						  const char *funcName)
{
	if(!(valueA == valueB))
	{
		wpi_assertEqual_common_impl(valueA, valueB, "!=", message, fileName, lineNumber, funcName);
	}
	return valueA == valueB;
}

/**
* Assert not equal implementation.
* This determines whether the two given integers are equal. If so,
* the value of each is printed along with an optional message string.
* The users don't call this, but instead use the wpi_assertNotEqual macros in Utility.h.
*/
bool wpi_assertNotEqual_impl(int valueA,
							 int valueB,
							 const char *message,
							 const char *fileName,
							 uint32_t lineNumber, 
							 const char *funcName)
{
	if(!(valueA != valueB))
	{
		wpi_assertEqual_common_impl(valueA, valueB, "==", message, fileName, lineNumber, funcName);
	}
	return valueA != valueB;
}


/**
* Return the FPGA Version number.
* For now, expect this to be competition year.
* @return FPGA Version number.
*/
uint16_t GetFPGAVersion()
{
	tRioStatusCode status = 0;
	tGlobal *global = tGlobal::create(&status);
	uint16_t version = global->readVersion(&status);
	delete global;
	wpi_setGlobalError(status);
	return version;
}

/**
* Return the FPGA Revision number.
* The format of the revision is 3 numbers.
* The 12 most significant bits are the Major Revision.
* the next 8 bits are the Minor Revision.
* The 12 least significant bits are the Build Number.
* @return FPGA Revision number.
*/
uint32_t GetFPGARevision()
{
	tRioStatusCode status = 0;
	tGlobal *global = tGlobal::create(&status);
	uint32_t revision = global->readRevision(&status);
	delete global;
	wpi_setGlobalError(status);
	return revision;
}

/**
* Read the microsecond-resolution timer on the FPGA.
* 
* @return The current time in microseconds according to the FPGA (since FPGA reset).
*/
uint32_t GetFPGATime()
{
	tRioStatusCode status = 0;
	tGlobal *global = tGlobal::create(&status);
	uint32_t time = global->readLocalTime(&status);
	delete global;
	wpi_setGlobalError(status);
	return time;
}

// RT hardware access functions exported from ni_emb.out
extern "C"
{
	int32_t UserSwitchInput(int32_t nSwitch);
	int32_t LedInput(int32_t led);
	int32_t LedOutput(int32_t led, int32_t value);
}

/**
* Read the value of the USER1 DIP switch on the cRIO.
*/
int32_t GetRIOUserSwitch()
{
	int32_t switchValue = UserSwitchInput(0);
	wpi_assert(switchValue >= 0);
	return switchValue > 0;
}

/**
* Set the state of the USER1 status LED on the cRIO.
*/
void SetRIOUserLED(uint32_t state)
{
	LedOutput(0, state > 0);
}

/**
* Get the current state of the USER1 status LED on the cRIO.
* @return The curent state of the USER1 LED.
*/
int32_t GetRIOUserLED()
{
	return LedInput(0);
}

/**
* Toggle the state of the USER1 status LED on the cRIO.
* @return The new state of the USER1 LED.
*/
int32_t ToggleRIOUserLED()
{
	int32_t ledState = !GetRIOUserLED();
	SetRIOUserLED(ledState);
	return ledState;
}

/**
* Set the state of the FPGA status LED on the cRIO.
*/
void SetRIO_FPGA_LED(uint32_t state)
{
	tRioStatusCode status = 0;
	tGlobal *global = tGlobal::create(&status);
	global->writeFPGA_LED(state, &status);
	wpi_setGlobalError(status);
	delete global;
}

/**
* Get the current state of the FPGA status LED on the cRIO.
* @return The curent state of the FPGA LED.
*/
int32_t GetRIO_FPGA_LED()
{
	tRioStatusCode status = 0;
	tGlobal *global = tGlobal::create(&status);
	bool ledValue = global->readFPGA_LED(&status);
	wpi_setGlobalError(status);
	delete global;
	return ledValue;
}

/**
* Toggle the state of the FPGA status LED on the cRIO.
* @return The new state of the FPGA LED.
*/
int32_t ToggleRIO_FPGA_LED()
{
	int32_t ledState = !GetRIO_FPGA_LED();
	SetRIO_FPGA_LED(ledState);
	return ledState;
}


