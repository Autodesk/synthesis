EXTERNAL_PATH=../../external

CXX            = g++
ARMCXX         = arm-frc-linux-gnueabi-g++
CXXFLAGS       = -std=c++17 -g -I$(EXTERNAL_PATH)/allwpilib/wpiutil/src/main/native/include/ -I$(EXTERNAL_PATH)/wpiutil/src/main/native/include/ -I$(EXTERNAL_PATH)/allwpilib/ni-libraries/include/ -I$(EXTERNAL_PATH)/allwpilib/hal/src/main/native/include -I$(EXTERNAL_PATH)/allwpilib/hal/src/main/native/ -Iinclude/
#Assuming WPILib shared object library directories are placed one directory up from Synthesis
WPILIB_SO_PATH = ../../../../

all: make_folders analog_output pwm digital_io FRCUserProgram

analog_output: build/analog_output
pwm: build/pwm
digital_io: build/digital_io
types: build/types
socket: build/socket.o
udp_socket: build/udp_socket.o
libnetwork: build/libnetwork.a
FRCUserProgram: build/FRCUserProgram

make_folders:
	mkdir -p build

build/analog_output: src/analog_output.cpp include/roborio.h
	$(CXX) $(CXXFLAGS) src/analog_output.cpp -DANALOG_OUTPUT_TEST -o "build/analog_output"

build/pwm: src/pwm.cpp include/roborio.h
	$(CXX) $(CXXFLAGS) src/pwm.cpp -DPWM_TEST -o "build/pwm"

build/digital_io: src/digital_io.cpp include/roborio.h
	$(CXX) $(CXXFLAGS) src/digital_io.cpp -DDIGITAL_IO_TEST -o "build/digital_io"

build/socket.o: src/net/socket.cpp include/net/socket.hpp
	$(CXX) $(CXXFLAGS) src/net/socket.cpp -c -o "build/socket.o"

build/udp_socket.o: src/net/udp_socket.cpp include/net/udp_socket.hpp
	$(CXX) $(CXXFLAGS) src/net/udp_socket.cpp -c -o "build/udp_socket.o"

build/libnetwork.a: build/udp_socket.o build/socket.o
	ar rcs build/libnetwork.a build/socket.o build/udp_socket.o

build/FRCUserProgram: test_project/robot.cpp
	$(ARMCXX) -std=c++1y -I$(WPILIB_SO_PATH)wpilib/cpp/current/include -O0 -Og -g3 -Wall -c -fmessage-length=0 -pthread -o "build/robot.o" test_project/robot.cpp
	$(ARMCXX) -L$(WPILIB_SO_PATH)wpilib/common/current/lib/linux/athena/shared -L$(WPILIB_SO_PATH)wpilib/cpp/current/reflib/linux/athena/shared -pthread -rdynamic -Wl,-rpath,/opt/GenICam_v3_0_NI/bin/Linux32_ARM,-rpath,/usr/local/frc/lib -o build/FRCUserProgram build/robot.o -lwpi
	
.PHONY: clean
clean:	
	rm -rf build/*;
