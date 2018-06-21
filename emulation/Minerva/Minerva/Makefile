all: FunctionSignature MinervaGenerator

FunctionSignature: FunctionSignature.cpp
	g++ FunctionSignature.cpp Util.cpp -DFUNCTION_SIGNATURE_TEST -std=c++14 -o "FunctionSignature"

	
MinervaGenerator: MinervaGenerator.cpp
	g++ MinervaGenerator.cpp FunctionSignature.cpp Util.cpp -DMINERVA_GENERATOR_TEST -std=c++14 -o "MinervaGenerator"
	
.PHONY: clean
clean:	
	rm -rf FunctionSignature;
	rm -rf MinervaGenerator;