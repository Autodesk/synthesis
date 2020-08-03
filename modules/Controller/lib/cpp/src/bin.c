#include "controller_sys.h"
#include <stdio.h>
#include <string.h>

int main(){
	int error_code = 0;
	const char* error_message = NULL;
	const char* error_data = NULL;
	
	printf("No errors:\n\n");
	
	int i = Test(1425, &error_code, &error_message, &error_data);
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
	
	i = Test(25, &error_code, &error_message, &error_data);	
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