all: FunctionSignature MinervaGenerator

FunctionSignature: FunctionSignature.cpp
	g++ FunctionSignature.cpp Util.cpp -DFUNCTION_SIGNATURE_TEST -std=c++14 -o "FunctionSignature"

	
MinervaGenerator: MinervaGenerator.cpp
	g++ MinervaGenerator.cpp FunctionSignature.cpp Util.cpp -DMINERVA_GENERATOR_TEST -std=c++14 -o "MinervaGenerator"
	
Minerva: Minerva.cpp
	g++ Minerva.cpp -I../../hel/allwpilib/hal/src/main/native/include/ -I../../hel/allwpilib/hal/src/main/native/athena/ -I../../hel/allwpilib/ni-libraries/include -I../../hel/wpiutil/src/main/native/include
	
.PHONY: clean
clean:	
	rm -rf FunctionSignature;
	rm -rf MinervaGenerator;
	rm -rf Minerva;
