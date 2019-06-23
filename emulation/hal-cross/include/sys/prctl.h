#ifndef _HEL_PRCTL_H_
#define _HEL_PRCTL_H_

#include <stdio.h>

#define SIGKILL 9
#define PR_SET_PDEATHSIG 1

using pid_t = int;

int prctl(int option, ...);

int kill(pid_t pid, int sig);

void setlinebuf(FILE *stream); // From stdio.h

#endif
