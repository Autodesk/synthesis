all: FunctionSignature

FunctionSignature: FunctionSignature.cpp
	g++ FunctionSignature.cpp Util.cpp -DFUNCTION_SIGNATURE_TEST -std=c++14 -o "FunctionSignature"

.PHONY: clean
clean:	
	rm -rf FunctionSignature;