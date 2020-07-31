CXX = g++
CXXARGS = -std=c++11 -Werror -g -Wall

all: bin

bin: src/bin.cpp
	$(CXX) $(CXXARGS) src/bin.cpp -o bin -lcontroller_sys -L./../../native/controller_sys/target/debug

copy_dll: ../../native/controller_sys/target/debug/controller_sys.dll
	cp ../../native/controller_sys/target/debug/controller_sys.dll .

clean:
	rm -f *.so *.dll *.exe *.out
