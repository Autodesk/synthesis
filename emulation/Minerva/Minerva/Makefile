CXX            = g++
ARMCXX         = arm-linux-gnueabihf-g++
CXXFLAGS       = -std=c++17 -g -I../../external/allwpilib/hal/src/main/native/include/ -Iinclude/
#Assuming WPILib shared object library directories are placed one directory up from Synthesis
WPILIB_SO_PATH = ../../../../

all: make_folders function_signature minerva_generator socket udp_socket libnetwork minerva test FRCUserProgram

function_signature: build/function_signature
minerva_generator: build/minerva_generator
types: build/types
socket: build/socket.o
udp_socket: build/udp_socket.o
libnetwork: build/libnetwork.a
test: build/test 
minerva: build/libminerva.a
FRCUserProgram: build/FRCUserProgram

make_folders:
	mkdir -p build

build/function_signature: src/function_signature.cpp include/function_signature.h src/util.cpp include/util.h src/types.cpp include/types.h
	$(CXX) $(CXXFLAGS) src/function_signature.cpp src/util.cpp src/types.cpp -DFUNCTION_SIGNATURE_TEST -o "build/function_signature"

build/minerva_generator: src/minerva_generator.cpp include/minerva_generator.h src/function_signature.cpp include/function_signature.h src/util.cpp include/util.h src/types.cpp include/types.h
	$(CXX) $(CXXFLAGS) src/minerva_generator.cpp src/function_signature.cpp src/util.cpp src/types.cpp -DMINERVA_GENERATOR_TEST -o "build/minerva_generator"

build/test: src/test.cpp include/test.h
	$(CXX) $(CXXFLAGS) src/test.cpp -Lbuild -l:libminerva.a -DTEST_TEST -o "build/test"

build/types: src/types.cpp include/types.h
	$(CXX) $(CXXFLAGS) src/types.cpp -DTYPES_TEST -o "build/types"

build/socket.o: src/net/socket.cpp include/net/socket.hpp
	$(CXX) $(CXXFLAGS) src/net/socket.cpp -c -o "build/socket.o"

build/udp_socket.o: src/net/udp_socket.cpp include/net/udp_socket.hpp
	$(CXX) $(CXXFLAGS) src/net/udp_socket.cpp -c -o "build/udp_socket.o"

build/libnetwork.a: build/udp_socket.o build/socket.o
	ar rcs build/libnetwork.a build/socket.o build/udp_socket.o
#src/net/socket.cpp include/net/socket.hpp src/net/udp_socket.cpp include/net/udp_socket.hpp include/net/socket_exception.hpp

build/libminerva.a: src/minerva_generator.cpp include/minerva_generator.h src/function_signature.cpp include/function_signature.h src/util.cpp include/util.h src/types.cpp include/types.h include/handler.h include/globals.h
	build/minerva_generator
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/minerva.cpp -o build/minerva.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/util.cpp -o build/util.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/types.cpp -o build/types.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/function_signature.cpp -o build/function_signature.o
	ar rcs build/libminerva.a build/util.o build/types.o build/function_signature.o build/minerva.o 

build/FRCUserProgram: test_project/robot.cpp
	$(ARMCXX) -std=c++1y -I$(WPILIB_SO_PATH)wpilib/cpp/current/include -O0 -Og -g3 -Wall -c -fmessage-length=0 -pthread -o "build/robot.o" test_project/robot.cpp
	$(ARMCXX) -L$(WPILIB_SO_PATH)wpilib/common/current/lib/linux/athena/shared -L$(WPILIB_SO_PATH)wpilib/cpp/current/reflib/linux/athena/shared -pthread -rdynamic -Wl,-rpath,/opt/GenICam_v3_0_NI/bin/Linux32_ARM,-rpath,/usr/local/frc/lib -o build/FRCUserProgram build/robot.o -lwpi
	
.PHONY: clean
clean:	
	rm -rf build/*;
