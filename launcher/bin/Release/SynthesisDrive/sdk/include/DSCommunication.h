/*
 * DSCommunication.h
 *
 *  Created on: Aug 24, 2015
 *      Author: Jake Springer
 */

#include "AardvarkGate.h"

#ifndef DSCOMMUNICATION_H_
#define DSCOMMUNICATION_H_

void RunDriverStationThread(void);
void RunStateNetworkThread(void);

typedef void (*DSThread)(void);
constexpr DSThread g_DS_THREAD = &RunDriverStationThread;

extern volatile bool __DSCommunication_dsRunning;
extern volatile unsigned int TeamID;

#endif /* DSCOMMUNICATION_H_ */
