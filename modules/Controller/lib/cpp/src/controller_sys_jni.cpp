#include <stdio.h>
#include <jni.h>

#include "controller_sys.hpp"

#ifdef __cplusplus
extern "C" {
#endif

JNIEXPORT jint JNICALL Java_ControllerSys_Test(JNIEnv *env, jclass, jint value, jobject obj){
	/*
	int error_code = 0;
	const char* error_message = NULL;
	const char* error_data = NULL;

	int ret = Test(value, &error_code, &error_message, &error_data);

	jclass myclass = env->FindClass("ControllerError");

	jfieldID  fid = env->GetFieldID(myclass,"error_code","I");
	env->SetIntField(obj ,fid, error_code);

	if(error_message != NULL){
		fid = env->GetFieldID(myclass,"error_message","Ljava/lang/String;");
		env->SetObjectField(obj ,fid, env->NewStringUTF(error_message));
	}
	if(error_data != NULL){
		fid = env->GetFieldID(myclass,"error_data","Ljava/lang/String;");
		env->SetObjectField(obj ,fid, env->NewStringUTF(error_data));
	}
	*/
	return 5;//ret;
}

#ifdef __cplusplus
}
#endif
