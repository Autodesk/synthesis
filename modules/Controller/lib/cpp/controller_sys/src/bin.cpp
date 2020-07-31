#include <iostream>
#include "controller_sys.hpp"

int main(){
	int error_code = 0;
	const char* error_message = NULL;
	const char* error_data = NULL;
	
	std::cout << "No errors:\n\n";
	
	int i = Test(1425, &error_code, &error_message, &error_data);	
	std::cout << i << "\n";
	
	if(error_code != 0){
		std::cout << error_code << "\n";
	}
	if(error_message != NULL){
		printf("%s\n", error_message);
	}
	if(error_data != NULL){
		printf("%s\n", error_data);
	}
	
	i = Test(1425);
	std::cout << i << "\n";
	
	std::cout << "\nErrors:\n\n";
	
	i = Test(25, &error_code, &error_message, &error_data);	
	std::cout << i << "\n";
	
	if(error_code != 0){
		std::cout << error_code << "\n";
	}
	if(error_message != NULL){
		printf("%s\n", error_message);
	}
	if(error_data != NULL){
		printf("%s\n", error_data);
	}
	
	i = Test(25);
	std::cout << i << "\n";
}