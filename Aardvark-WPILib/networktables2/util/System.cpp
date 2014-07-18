/*
* System.cpp
*
*  Created on: Sep 26, 2012
*      Author: Mitchell Wills
*/

#include "networktables2/util/System.h"
#include <stdio.h>
#include <Windows.h>
#include <stdint.h>

void sleep_ms(unsigned long ms){
	Sleep(ms);
}
unsigned long currentTimeMillis(){
	FILETIME ft_now;
	GetSystemTimeAsFileTime(&ft_now);
	uint64_t ll_now = (LONGLONG)ft_now.dwLowDateTime + ((LONGLONG)(ft_now.dwHighDateTime) << 32LL);
	return (long)(ll_now / 10000);
}
void writeWarning(const char* message){
	fprintf(stderr, "%s\n", message);
	fflush(stderr);
	//TODO implement write warning with wpilib error stuff
}
