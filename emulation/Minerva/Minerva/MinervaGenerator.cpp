#include "MinervaGenerator.h"

#include <iostream>
#include <fstream>

using namespace std;

const array<string,minerva::MinervaGenerator::HAL_HEADER_COUNT> minerva::MinervaGenerator::HAL_HEADER_NAMES = {
	"Accelerometer.h",
	"AnalogAccumulator.h",
	"AnalogGyro.h",
	"AnalogInput.h",
	"AnalogOutput.h",
	"AnalogTrigger.h",
	"ChipObject.h",
	"Compressor.h",
	"Constants.h",
	"Counter.h",
	"cpp/NotifierInternal.h",
	"DIO.h",
	"DriverStation.h",
	"Encoder.h",
	"Errors.h",
	"HAL.h",
	"I2C.h",
	"Interrupts.h",
	"Notifier.h",
	"OSSerialPort.h",
	"PDP.h",
	"Ports.h",
	"Power.h",
	"PWM.h",
	"Relay.h",
	"SerialPort.h",
	"Solenoid.h",
	"SPI.h",
	"Threads.h",
	"Types.h",
	"UsageReporting.h",
};

const string minerva::MinervaGenerator::MINERVA_FILE_NAME = "Minerva.cpp";

const string minerva::MinervaGenerator::MINERVA_FILE_PREFIX = "\
//Auto-generated HAL interface for emulation\n\
\n\
#include \"visa/visa.h\"\n\
#include \"AnalogInternal.h\"\n\
#include \"ConstantsInternal.h\"\n\
#include \"DigitalInternal.h\"\n\
#include \"EncoderInternal.h\"\n\
#include \"FPGAEncoder.h\"\n\
#include \"FRC_NetworkCommunication/CANSessionMux.h\"	//CAN Comm\n\
#include \"FRC_NetworkCommunication/CANSessionMux.h\"\n\
#include \"HAL/Accelerometer.h\"\n\
#include \"HAL/AnalogAccumulator.h\"\n\
#include \"HAL/AnalogGyro.h\"\n\
#include \"HAL/AnalogInput.h\"\n\
#include \"HAL/AnalogOutput.h\"\n\
#include \"HAL/AnalogTrigger.h\"\n\
#include \"HAL/ChipObject.h\"\n\
#include \"HAL/Compressor.h\"\n\
#include \"HAL/Constants.h\"\n\
#include \"HAL/Counter.h\"\n\
#include \"HAL/DIO.h\"\n\
#include \"HAL/DriverStation.h\"\n\
#include \"HAL/Encoder.h\"\n\
#include \"HAL/Errors.h\"\n\
#include \"HAL/HAL.h\"\n\
#include \"HAL/I2C.h\"\n\
#include \"HAL/Interrupts.h\"\n\
#include \"HAL/Notifier.h\"\n\
#include \"HAL/OSSerialPort.h\"\n\
#include \"HAL/PDP.h\"\n\
#include \"HAL/PWM.h\"\n\
#include \"HAL/Ports.h\"\n\
#include \"HAL/Power.h\"\n\
#include \"HAL/Relay.h\"\n\
#include \"HAL/SPI.h\"\n\
#include \"HAL/SerialPort.h\"\n\
#include \"HAL/Solenoid.h\"\n\
#include \"HAL/Threads.h\"\n\
#include \"HAL/cpp/NotifierInternal.h\"\n\
#include \"HAL/cpp/SerialHelper.h\"\n\
#include \"HAL/cpp/make_unique.h\"\n\
#include \"HAL/cpp/priority_condition_variable.h\"\n\
#include \"HAL/cpp/priority_mutex.h\"\n\
#include \"HAL/handles/HandlesInternal.h\"\n\
#include \"HAL/handles/IndexedHandleResource.h\"\n\
#include \"HAL/handles/LimitedClassedHandleResource.h\"\n\
#include \"HAL/handles/LimitedHandleResource.h\"\n\
#include \"HAL/handles/UnlimitedHandleResource.h\"\n\
#include \"PCMInternal.h\"\n\
#include \"PortsInternal.h\"\n\
#include \"ctre/CtreCanNode.h\"\n\
#include \"ctre/PCM.h\"\n\
#include \"ctre/PCM.h\"\n\
#include \"ctre/PDP.h\"\n\
#include \"ctre/PDP.h\"\n\
#include \"ctre/ctre.h\"\n\
#include \"support/SafeThread.h\"\n\
#include \"visa/visa.h\"\n\
#include <FRC_NetworkCommunication/AICalibration.h>\n\
#include <FRC_NetworkCommunication/CANSessionMux.h>\n\
#include <FRC_NetworkCommunication/FRCComm.h>\n\
#include <FRC_NetworkCommunication/LoadOut.h>\n\
#include <algorithm>\n\
#include <atomic>\n\
#include <cassert>\n\
#include <chrono>\n\
#include <cmath>\n\
#include <cstdio>\n\
#include <cstdlib>\n\
#include <cstring>\n\
#include <fcntl.h>\n\
#include <fstream>\n\
#include <i2clib/i2c-lib.h>\n\
#include <limits>\n\
#include <llvm/StringRef.h>\n\
#include <llvm/raw_ostream.h>\n\
#include <memory>\n\
#include <mutex>\n\
#include <pthread.h>\n\
#include <sched.h>\n\
#include <signal.h>  // linux for kill\n\
#include <spilib/spi-lib.h>\n\
#include <stdint.h>\n\
#include <string.h> // memset\n\
#include <string>\n\
#include <support/SafeThread.h>\n\
#include <sys/ioctl.h>\n\
#include <sys/prctl.h>\n\
#include <termios.h>\n\
#include <thread>\n\
#include <unistd.h>\n\
\n\
#include \"FunctionSignature.h\" // ParameterValueInfo\n\
#include \"Channel.h\"\n\
#include \"Handler.h\"\n\
using namespace hal;\n\
\n\
#ifdef __cplusplus\n\
extern \"C\" {\n\
#endif\n";

const string minerva::MinervaGenerator::MINERVA_FILE_SUFFIX = "\
#ifdef __cplusplus\n\
}\n\
#endif\n";

vector<minerva::FunctionSignature> minerva::MinervaGenerator::parseHALFunctionSignatures(const string HAL_HEADER_PATH){
	vector<minerva::FunctionSignature> HAL_function_signatures;
	
	for(string HAL_header: minerva::MinervaGenerator::HAL_HEADER_NAMES){
		for(minerva::FunctionSignature function_signature: minerva::parseFunctionSignatures(HAL_HEADER_PATH + HAL_header)){
			HAL_function_signatures.push_back(function_signature);
		}
	}
	return HAL_function_signatures;
}

void minerva::MinervaGenerator::generateMinerva(const string HAL_HEADER_PATH){
	ofstream minerva_file;
	minerva_file.open(MINERVA_FILE_NAME);
	
	minerva_file<<MINERVA_FILE_PREFIX<<"\n";
	
	for(minerva::FunctionSignature function_signature: minerva::MinervaGenerator::parseHALFunctionSignatures(HAL_HEADER_PATH)){
		minerva_file<<function_signature.toString()<<"{\n";
		minerva_file<<"\tstd::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;\n";
		for(minerva::FunctionSignature::ParameterNameInfo parameter_name_info: function_signature.parameters){
			minerva_file<<"\tparameters.push_back({\""<<parameter_name_info.type<<"\","<<parameter_name_info.name<<"});\n";
		}
		if(function_signature.return_type == "void"){
			minerva_file<<"\tcallFunc(std::string(\""<<function_signature.name<<"\"),parameters);\n";
		} else {
			minerva_file<<"\tminerva::Channel<"<<function_signature.return_type<<"> c;\n";
			minerva_file<<"\tcallFunc(std::string(\""<<function_signature.name<<"\"),parameters,c);\n";
			minerva_file<<"\t"<<function_signature.return_type<<" x;\n";
			minerva_file<<"\tc.get(x);\n";
			minerva_file<<"\treturn x;\n";
		}
		minerva_file<<"}\n\n";
	}
	
	minerva_file<<MINERVA_FILE_SUFFIX;
	
	minerva_file.close();
}

#ifdef MINERVA_GENERATOR_TEST

int main(){
	const string HAL_HEADER_PATH = "../../hel/allwpilib/hal/src/main/native/include/HAL/";
	
	/*
	for(minerva::FunctionSignature function_signature: minerva::MinervaGenerator::parseHALFunctionSignatures(HAL_HEADER_PATH)){
		cout<<function_signature<<"\n";
	}
	*/
	
	/*
	minerva::FunctionSignature::ParameterValueInfo a = {"type",123};
	cout<<a<<"\n";
	*/
	
	minerva::MinervaGenerator::generateMinerva(HAL_HEADER_PATH);
}

#endif