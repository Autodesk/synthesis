
namespace sim{
	jint SimOnLoad(JavaVM* vm, void* reserved){
		return JNI_VERSION_1_6;
	}

	void SimOnUnload(JavaVM* vm, void* reserved){}
}