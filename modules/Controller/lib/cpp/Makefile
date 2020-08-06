CXX = gcc
CXXARGS = -Werror -g -Wall

all: bin controller_sys_jni.dll

bin: src/bin.c src/controller_sys.h
	$(CXX) $(CXXARGS) src/bin.c -o bin -lcontroller_sys -L./../native/controller_sys/target/debug

copy_dll: ../native/controller_sys/target/debug/controller_sys.dll
	cp ../native/controller_sys/target/debug/controller_sys.dll .

controller_sys_jni.dll: src/controller_sys_jni.c src/controller_sys.h
	$(CXX) $(CXXARGS) -I/cygdrive/c/Program\ Files/Java/jdk-10.0.2/include -I/cygdrive/c/Program\ Files/Java/jdk-10.0.2/include/win32 -D__int64=__int64_t -lcontroller_sys -L./../native/controller_sys/target/debug --shared src/controller_sys_jni.c -o controller_sys_jni.dll

clean:
	rm -f *.so *.dll *.exe *.out
