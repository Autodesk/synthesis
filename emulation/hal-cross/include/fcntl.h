#ifndef _HEL_FCNTL_H_
#define _HEL_FCNTL_H_

#define O_RDWR 2

int open(const char *path, int oflag, ... );/*{
	return 1;
}*/

int close(int fildes);/*{
	return 0;
}*/

#endif
