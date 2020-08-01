CXX = g++
CXXARGS = -std=c++11 -Werror -g -Wall

all: bin controller_sys_jni.dll

bin: src/bin.cpp
	$(CXX) $(CXXARGS) src/bin.cpp -o bin -lcontroller_sys -L./../native/controller_sys/target/debug

copy_dll: ../native/controller_sys/target/debug/controller_sys.dll
	cp ../native/controller_sys/target/debug/controller_sys.dll .

controller_sys_jni.dll: src/controller_sys_jni.cpp
	$(CXX) $(CXXARGS) -I/cygdrive/c/Program\ Files/Java/jdk-10.0.2/include -I/cygdrive/c/Program\ Files/Java/jdk-10.0.2/include/win32 -D__int64=__int64_t -lcontroller_sys -L./../native/controller_sys/target/debug --shared src/controller_sys_jni.cpp -o controller_sys_jni.dll

clean:
	rm -f *.so *.dll *.exe *.out
