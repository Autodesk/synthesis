#include <iostream>
#include "controller_sys.hpp"

int main(){
	int error_code = 0;
	const char* error_message;
	const char* error_data;
	int i = Test(1425, &error_code, &error_message, &error_data);
	std::cout << i << "\n" << error_code << "\n";
	printf("%s\n", error_message);
	printf("%s\n", error_data);
}
