/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef _ERROR_H
#define _ERROR_H

#include "Base.h"
#include "ChipObject/NiRio.h"
#include <string>
#include <vxWorks.h>

//  Forward declarations
class ErrorBase;

/**
 * Error object represents a library error.
 */
class Error
{
public:
	typedef tRioStatusCode Code;

	Error();
	~Error();
	void Clone(Error &error);
	Code GetCode() const;
	const char *GetMessage() const;
	const char *GetFilename() const;
	const char *GetFunction() const;
	uint32_t GetLineNumber() const;
	const ErrorBase* GetOriginatingObject() const;
	double GetTime() const;
	void Clear();
	void Set(Code code, const char* contextMessage, const char* filename,
		const char *function, uint32_t lineNumber, const ErrorBase* originatingObject);
	static void EnableStackTrace(bool enable) { m_stackTraceEnabled=enable; }
	static void EnableSuspendOnError(bool enable) { m_suspendOnErrorEnabled=enable; }

private:
	void Report();

	Code m_code;
	std::string m_message;
	std::string m_filename;
	std::string m_function;
	uint32_t m_lineNumber;
	const ErrorBase* m_originatingObject;
	double m_timestamp;

	static bool m_stackTraceEnabled;
	static bool m_suspendOnErrorEnabled;
	DISALLOW_COPY_AND_ASSIGN(Error);
};

#endif

