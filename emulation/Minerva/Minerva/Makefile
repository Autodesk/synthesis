all: FunctionSignature MinervaGenerator Test Minerva

FunctionSignature: FunctionSignature.cpp
	g++ FunctionSignature.cpp Util.cpp -DFUNCTION_SIGNATURE_TEST -std=c++17 -o "FunctionSignature"

MinervaGenerator: MinervaGenerator.cpp
	g++ MinervaGenerator.cpp FunctionSignature.cpp Util.cpp -DMINERVA_GENERATOR_TEST -std=c++17 -o "MinervaGenerator"

Test: Minerva
	g++ Test.cpp -L. -l:libminerva.a -DTEST_TEST -I../../external/allwpilib/hal/src/main/native/include/ -std=c++17 -o "Test"

Minerva: MinervaGenerator
	./MinervaGenerator
	g++ -I../../external/allwpilib/hal/src/main/native/include/ -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -std=c++17 -Wall -Werror -c Minerva.cpp
	g++ -I../../external/allwpilib/hal/src/main/native/include/ -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -std=c++17 -Wall -Werror -c Util.cpp
	g++ -I../../external/allwpilib/hal/src/main/native/include/ -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -std=c++17 -Wall -Werror -c FunctionSignature.cpp
	ar rcs libminerva.a Util.o FunctionSignature.o Minerva.o 

.PHONY: clean
clean:	
	rm -rf FunctionSignature;
	rm -rf MinervaGenerator;
	rm -rf Minerva;
	rm -rf minerva.o;
	rm -rf FunctionSignature.o Util.o Minerva.o Minerva.cpp;
