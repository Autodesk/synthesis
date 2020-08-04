#include "controller_sys.h"
#include <stdio.h>
#include <string.h>

int main(){
	int error_code = 0;
	const char* error_message = NULL;
	const char* error_data = NULL;
	
	log_str("Hello World!\0", 2, &error_code, &error_message, &error_data);
		if(error_code != 0){
		printf("%d\n", error_code);
	}
	if(error_message != NULL){
		printf("%s\n", error_message);
	}
	if(error_data != NULL){
		printf("%s\n", error_data);
	}
	
	printf("No errors:\n\n");
	
	int i = test(1425, &error_code, &error_message, &error_data);
	printf("%d\n", i);
	
	if(error_code != 0){
		printf("%d\n", error_code);
	}
	if(error_message != NULL){
		printf("%s\n", error_message);
	}
	if(error_data != NULL){
		printf("%s\n", error_data);
	}
	
	printf("\nErrors:\n\n");
	
	i = test(25, &error_code, &error_message, &error_data);	
	printf("%d\n", i);
	
	if(error_code != 0){
		printf("%d\n", error_code);
	}
	if(error_message != NULL){
		printf("%s\n", error_message);
	}
	if(error_data != NULL){
		printf("%s\n", error_data);
	}
}