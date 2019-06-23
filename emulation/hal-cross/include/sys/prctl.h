#ifndef _HEL_PRCTL_H
#define _HEL_PRCTL_H

#include <stdio.h>

using pid_t = int;

int prctl(int option, ...);/*{
	return 0; // TODO
}*/

int kill(pid_t pid, int sig);/*{
	return 0;
}*/

#define SIGKILL 9
#define PR_SET_PDEATHSIG 1

void setlinebuf(FILE *stream);/*} // From stdio.h
}*/

#endif
