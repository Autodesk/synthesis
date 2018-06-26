CXX            = g++
CXXFLAGS       = -std=c++17 -g -I../../external/allwpilib/hal/src/main/native/include/ -Iinclude/
#Assuming WPILib shared object library directories are placed one directory up from Synthesis
WPILIB_SO_PATH = ../../../../

all: make_folders function_signature minerva_generator test network minerva FRCUserProgram

make_folders:
	mkdir -p build

function_signature: src/function_signature.cpp
	$(CXX) $(CXXFLAGS) src/function_signature.cpp src/util.cpp -DFUNCTION_SIGNATURE_TEST -o "build/function_signature"

minerva_generator: src/minerva_generator.cpp
	$(CXX) $(CXXFLAGS) src/minerva_generator.cpp src/function_signature.cpp src/util.cpp -DMINERVA_GENERATOR_TEST -o "build/minerva_generator"

test: minerva
	$(CXX) $(CXXFLAGS) src/test.cpp -Lbuild -l:libminerva.a -DTEST_TEST -o "build/test"

network:
	$(CXX) $(CXXFLAGS) -Iinclude src/net/socket.cpp -c -o build/socket.o
	$(CXX) $(CXXFLAGS) -Iinclude src/net/udp_socket.cpp -c -o build/udp_socket.o
	ar rcs build/libnetwork.a build/socket.o build/udp_socket.o

minerva: minerva_generator
	build/minerva_generator
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/minerva.cpp -o build/minerva.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/util.cpp -o build/util.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/function_signature.cpp -o build/function_signature.o
	ar rcs build/libminerva.a build/util.o build/function_signature.o build/minerva.o 

FRCUserProgram: test_project/robot.cpp
	arm-frc-linux-gnueabi-g++ -std=c++1y -I$(WPILIB_SO_PATH)wpilib/cpp/current/include -O0 -Og -g3 -Wall -c -fmessage-length=0 -pthread -o "build/robot.o" test_project/robot.cpp
	arm-frc-linux-gnueabi-g++ -L$(WPILIB_SO_PATH)wpilib/common/current/lib/linux/athena/shared -L$(WPILIB_SO_PATH)wpilib/cpp/current/reflib/linux/athena/shared -pthread -rdynamic -Wl,-rpath,/opt/GenICam_v3_0_NI/bin/Linux32_ARM,-rpath,/usr/local/frc/lib -o build/FRCUserProgram build/robot.o -lwpi
	
.PHONY: clean
clean:	
	rm -rf build/*;
