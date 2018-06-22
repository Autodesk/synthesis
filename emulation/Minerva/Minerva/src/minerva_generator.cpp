
#include <iostream>
#include <fstream>

#include "minerva_generator.h"

using namespace std;

const array<string,minerva::MinervaGenerator::HAL_HEADER_COUNT> minerva::MinervaGenerator::HAL_HEADER_NAMES = {
	"Accelerometer.h",
	"AnalogAccumulator.h",
	"AnalogGyro.h",
	"AnalogInput.h",
	"AnalogOutput.h",
	"AnalogTrigger.h",
	"CAN.h",
	"CANAPI.h",
	"ChipObject.h",
	"Compressor.h",
	"Constants.h",
	"Counter.h",
	"DIO.h",
	"DriverStation.h",
	"Encoder.h",
	"Errors.h",
	"Extensions.h",
	"HAL.h",
	"I2C.h",
	"Interrupts.h",
	"Notifier.h",
	"PDP.h",
	"Ports.h",
	"Power.h",
	"PWM.h",
	"Relay.h",
	"SerialPort.h",
	"Solenoid.h",
	"SPI.h",
	"Threads.h",
	"Types.h"
	};

const string minerva::MinervaGenerator::MINERVA_FILE_NAME = "src/minerva.cpp";

const string minerva::MinervaGenerator::MINERVA_FILE_PREFIX = "\
//Auto-generated HAL interface for emulation\n\
\n\
#include \"HAL/Accelerometer.h\"\n\
#include \"HAL/AnalogAccumulator.h\"\n\
#include \"HAL/AnalogGyro.h\"\n\
#include \"HAL/AnalogInput.h\"\n\
#include \"HAL/AnalogOutput.h\"\n\
#include \"HAL/AnalogTrigger.h\"\n\
#include \"HAL/CAN.h\"\n\
#include \"HAL/ChipObject.h\"\n\
#include \"HAL/Compressor.h\"\n\
#include \"HAL/Constants.h\"\n\
#include \"HAL/Counter.h\"\n\
#include \"HAL/DIO.h\"\n\
#include \"HAL/DriverStation.h\"\n\
#include \"HAL/Errors.h\"\n\
#include \"HAL/HAL.h\"\n\
#include \"HAL/I2C.h\"\n\
#include \"HAL/Interrupts.h\"\n\
#include \"HAL/Notifier.h\"\n\
#include \"HAL/PDP.h\"\n\
#include \"HAL/PWM.h\"\n\
#include \"HAL/Ports.h\"\n\
#include \"HAL/Power.h\"\n\
#include \"HAL/Relay.h\"\n\
#include \"HAL/SPI.h\"\n\
#include \"HAL/SerialPort.h\"\n\
#include \"HAL/Solenoid.h\"\n\
#include \"HAL/Types.h\"\n\
#include \"HAL/Threads.h\"\n\
#include \"HAL/CANAPI.h\"\n\
#include \"HAL/handles/HandlesInternal.h\"\n\
#include \"HAL/UsageReporting.h\"\n\
#include <FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/nInterfaceGlobals.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAI.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAO.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccel.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAccumulator.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAlarm.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tAnalogTrigger.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tBIST.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tCounter.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDIO.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tDMA.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tEncoder.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tGlobal.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tInterrupt.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPWM.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPower.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tRelay.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSPI.h>\n\
#include <FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tSysWatchdog.h>\n\
#include <FRC_FPGA_ChipObject/tDMAChannelDescriptor.h>\n\
#include <FRC_FPGA_ChipObject/tDMAManager.h>\n\
#include <FRC_FPGA_ChipObject/tInterruptManager.h>\n\
#include <FRC_FPGA_ChipObject/tSystem.h>\n\
#include <FRC_FPGA_ChipObject/tSystemInterface.h>\n\
#include <array>\n\
#include <chrono>\n\
#include <cstddef>\n\
#include <limits>\n\
#include <memory>\n\
#include <pthread.h>\n\
#include <stdint.h>\n\
#include <stdlib.h>\n\
#include <string>\n\
#include <utility>\n\
#include <vector>\n\
#include <wpi/SmallString.h>\n\
#include <wpi/SmallVector.h>\n\
#include <wpi/mutex.h>\n\
#include <wpi/raw_ostream.h>\n\
\n\
#include \"function_signature.h\" // ParameterValueInfo\n\
#include \"channel.h\"\n\
#include \"handler.h\"\n\
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
		bool skip_function = false;
		for(minerva::FunctionSignature::ParameterNameInfo parameter_name_info: function_signature.parameters){
			if(parameter_name_info.type.find("=") != string::npos){
				skip_function = true; //exclude parameters with default values
			}
		}
		if(skip_function){
			continue;
		}
		minerva_file<<function_signature.toString()<<"{\n";
		minerva_file<<"\tstd::vector<minerva::FunctionSignature::ParameterValueInfo> parameters;\n";
		for(minerva::FunctionSignature::ParameterNameInfo parameter_name_info: function_signature.parameters){
			minerva_file<<"\tparameters.push_back({\""<<parameter_name_info.type<<"\","<<parameter_name_info.name<<"});\n";
		}
		if(function_signature.return_type == "void"){
			minerva_file<<"\tcallFunc(\""<<function_signature.name<<"\",parameters);\n";
		} else {
			minerva_file<<"\tminerva::Channel<"<<function_signature.return_type<<"> c;\n";
			minerva_file<<"\tcallFunc(\""<<function_signature.name<<"\",parameters,c);\n";
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
	const string HAL_HEADER_PATH = "../../external/allwpilib/hal/src/main/native/include/HAL/";
	
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