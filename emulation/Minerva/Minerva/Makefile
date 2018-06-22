CXX      = g++
CXXFLAGS = -std=c++17 -g -I../../external/allwpilib/hal/src/main/native/include/ -Iinclude/

all: make_folders function_signature minerva_generator test minerva

make_folders:
	mkdir -p build

function_signature: src/function_signature.cpp
	$(CXX) $(CXXFLAGS) src/function_signature.cpp src/util.cpp -DFUNCTION_SIGNATURE_TEST -o "build/function_signature"

minerva_generator: src/minerva_generator.cpp
	$(CXX) $(CXXFLAGS) src/minerva_generator.cpp src/function_signature.cpp src/util.cpp -DMINERVA_GENERATOR_TEST -o "build/minerva_generator"

test: minerva
	$(CXX) $(CXXFLAGS) src/test.cpp -Lbuild -l:libminerva.a -DTEST_TEST -o "build/test"

minerva: minerva_generator
	build/minerva_generator
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/minerva.cpp -o build/minerva.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/util.cpp -o build/util.o
	$(CXX) $(CXXFLAGS) -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -Wall -Werror -c src/function_signature.cpp -o build/function_signature.o
	ar rcs build/libminerva.a build/util.o build/function_signature.o build/minerva.o 

.PHONY: clean
clean:	
	rm -rf build/*;
