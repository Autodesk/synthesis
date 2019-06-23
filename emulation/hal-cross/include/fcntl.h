#ifndef _HEL_FCNTL_H_
#define _HEL_FCNTL_H_

#define O_RDWR 2

int open(const char *path, int oflag, ... );

int close(int fildes);

#endif
