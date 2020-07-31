#ifndef CONTROLLER_SYS_HPP
#define CONTROLLER_SYS_HPP

extern "C" {
	int Test(int value, int* error_code = NULL, const char** error_message = NULL, const char** error_data = NULL);
}

#endif