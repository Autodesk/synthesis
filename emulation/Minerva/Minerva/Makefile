all: make_folders function_signature minerva_generator test minerva

make_folders:
	mkdir -p build

function_signature: src/function_signature.cpp
	g++ -Iinclude/ src/function_signature.cpp src/util.cpp -I../../external/allwpilib/hal/src/main/native/include/ -DFUNCTION_SIGNATURE_TEST -std=c++17 -o "build/function_signature"

minerva_generator: src/minerva_generator.cpp
	g++ -Iinclude/ src/minerva_generator.cpp src/function_signature.cpp src/util.cpp -I../../external/allwpilib/hal/src/main/native/include/ -DMINERVA_GENERATOR_TEST -std=c++17 -o "build/minerva_generator"

test: minerva
	g++ -Iinclude/ src/test.cpp -Lbuild -l:libminerva.a -DTEST_TEST -I../../external/allwpilib/hal/src/main/native/include/ -std=c++17 -o "build/test"

minerva: minerva_generator
	build/minerva_generator
	g++ -Iinclude/ -I../../external/allwpilib/hal/src/main/native/include/ -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -std=c++17 -Wall -Werror -c src/minerva.cpp -o build/minerva.o
	g++ -Iinclude/ -I../../external/allwpilib/hal/src/main/native/include/ -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -std=c++17 -Wall -Werror -c src/util.cpp -o build/util.o
	g++ -Iinclude/ -I../../external/allwpilib/hal/src/main/native/include/ -I../../external/allwpilib/hal/src/main/native/athena/ -I../../external/allwpilib/ni-libraries/include -I../../external/wpiutil/src/main/native/include -I../../external/allwpilib/wpiutil/src/main/native/include/ -std=c++17 -Wall -Werror -c src/function_signature.cpp -o build/function_signature.o
	ar rcs build/libminerva.a build/util.o build/function_signature.o build/minerva.o 

.PHONY: clean
clean:	
	rm -rf build/*;
