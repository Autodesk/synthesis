// DO NOT DELETE THIS EMPTY LINE
// Don't compile this file. It will never work. It is a shim that is injected into WPILib so it compiles for x86

namespace nFPGA {
    namespace nRoboRIO_FPGANamespace {
        unsigned int g_currentTargetClass; //Ni FPGA declares this as extern, so define it here
    }
}
namespace sim{
	jint SimOnLoad(JavaVM* vm, void* reserved){
		return JNI_VERSION_1_6;
	}

	void SimOnUnload(JavaVM* vm, void* reserved){}
}