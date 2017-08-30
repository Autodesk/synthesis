#c++ compiler
CC = x86_64-w64-mingw32-g++
#static library tool
AR = x86_64-w64-mingw32-ar

#default to debug mode
ifndef TARGET
	TARGET := debug
endif

#the java tool directory
ifndef JAVA_HOME
	#we get this by parsing java's version to find the correct subdirectory
	#this would be a lot easier if java either put it's binary path in
	#`PATH` or if it hadd a separate environment variable for this
	JAVA_VERSION := $(shell java -version |& \
		sed -ne 's/^java version "\([^"]*\)"/\1/gp')
	JAVA_HOME := /cygdrive/c/Program\ Files/java/jdk$(JAVA_VERSION)
endif

#java compiler
JAVAC := $(JAVA_HOME)/bin/javac.exe
#jar tool
JAR := $(JAVA_HOME)/bin/jar.exe

CCFLAGS := -m64
#this tricks the compiler into thinking we are on windows vista
CCFLAGS += -D_WIN32 -D_WIN32_WINNT=0x0602
#header directories
CCFLAGS += -Iinclude \
	-Iallwpilib/ni-libraries/include \
	-Iwpiutil/src/main/native/include
#sstatic linking for libgcc and libstdc++
CCFLAGS += -static-libgcc -static-libstdc++
#use c++14 standard
CCFLAGS += -std=c++14
#needed for JNI stuff with mingw
#black magic copied from stack overflow
CCFLAGS += -Wl,--add-stdcall-alias

#turn on optinizations if release mode, or debug info if debug mode
ifeq ($(TARGET), debug)
	CCFLAGS += -O0 -g
endif
ifeq ($(TARGET), release)
	CCFLAGS += -O2
endif

#cflags for the C++ wpilib
#header directories
CCFLAGS_CPP := \
	-Iallwpilib/wpilibc/src/main/native/include \
	-Iinclude \
	-Intcore/src/main/native/include
#keep all the global cflags
CCFLAGS_CPP += $(CCFLAGS)

#cflags for the HAL
#header directories
CCFLAGS_HAL := \
	-Ilib \
	-Ilib/athena \
	-Intcore/src/main/native/include
#keep all the global clfags
CCFLAGS_HAL += $(CCFLAGS)
#prefer this include path before the global ones if conflicts come up
CCFLAGS_HAL += -Iallwpilib/wpilibc/src/main/native/include

#cflags for ntcore
#header directories
CCFLAGS_NTCORE := \
	-Intcore/src/main/native/include
#kepp all the global cflags
CCFLAGS_NTCORE += $(CCFLAGS)

#ccflags for wpilib JNI stuff
#header directories
CCFLAGS_JAVA := \
	-I$(JAVA_HOME)/include \
	-I$(JAVA_HOME)/include/win32 \
	-Ibuild/java/header \
	-Intcore/src/main/native/include
#allow bad stuff for java
CCFLAGS_JAVA += -fpermissive
#let java know that this is JNI
CCFLAGS_JAVA += -D_JNI_IMPLEMENTATION_
#keep all the global cflags
CCFLAGS_JAVA += $(CCFLAGS)

#cflags for ntcore's JNI bindings
#header directories
CCFLAGS_NTCORE_JAVA := \
	-I$(JAVA_HOME)/include \
	-I$(JAVA_HOME)/include/win32 \
	-Ibuild/ntcore/java/header \
	-Intcore/src/main/native/include
#let java know that this is JNI
CCFLAGS_NTCORE_JAVA += -D_JNI_IMPLEMENTATION_
#keep all the global cflags
CCFLAGS_NTCORE_JAVA += $(CCFLAGS)

#find all the C++ source code files in the HAL directory
#this also includes some wpiutil stuff and should not include
#priority_mutex.cpp (because it doesn't work on windows), or ctre/fpga stuff
#(because it make sense off of the roborio)
SRC_FILES_HAL := $(shell find ./lib \
	./wpiutil/src/main/native/cpp -name "*.cpp" \
	-not -name "*priority_mutex.cpp" \
	-not -wholename "*ctre/*" \
	-not -name "*NiFpgaState.cpp")

#find all the C++ source code files in ntcore, but exclude JNI because that
#goes in a dll
SRC_FILES_NTCORE := $(shell find ./ntcore/src/main/native -name "*.cpp" \
	-not -wholename "*jni/*")

#the JNI file for ntcore
SRC_FILES_NTCORE_JAVA := ntcore/src/main/native/cpp/jni/NetworkTablesJNI.cpp

#find all the java source code files in ntcore
JAVA_FILES_NTCORE_JAVA := $(shell find ./ntcore/src/main/java -name "*.java")

#find all the C++ source code files for wpilib, excluding opencv related
#files and frcsim related files
SRC_FILES_CPP := $(shell find ./allwpilib/wpilibc/src/main/native \
	-name "*.cpp" \
	-not -name "VisionRunner.cpp" \
	-not -name "CameraServer.cpp" \
	-not -wholename "*sim/*")

#find all the JNI c++ source code for wpilib, excluding CAN
SRC_FILES_JAVA := $(shell find ./allwpilib/wpilibj/src/main -name "*.cpp" \
	-not -name "CANJNI.cpp")

#find all the java source code filese for wpilibj, excluding computer vision
#unit tests, and frcsim stuff
JAVA_FILES_JAVA := $(shell find ./allwpilib/wpilibj/src/main -name "*.java" \
	-not -wholename "*sim/*" \
	-not -wholename "*test/*" \
	-not -name "CameraServer.java" \
	-not -name "Vision*.java")
#also include our fake WPILibVersion file
JAVA_FILES_JAVA += java_overrides/edu/wpi/first/wpilibj/WPILibVersion.java

#find all the java source code files for wpiutil
JAVA_FILES_WPIUTIL_JAVA := \
	$(shell find ./wpiutil/src/main/java/ -name "*.java")

OBJ_FILES_CPP := $(addprefix build/cpp/,$(patsubst %.cpp,%.o,$(SRC_FILES_CPP)))
OBJ_FILES_JAVA := $(addprefix build/java/,$(patsubst %.cpp,%.o,$(SRC_FILES_JAVA)))
OBJ_FILES_HAL := $(addprefix build/hal/,$(patsubst %.cpp,%.o,$(SRC_FILES_HAL)))
OBJ_FILES_NTCORE := $(addprefix build/ntcore/,$(patsubst %.cpp,%.o,$(SRC_FILES_NTCORE)))
OBJ_FILES_NTCORE_JAVA := $(addprefix build/ntcore/java/,$(patsubst %.cpp,%.o,$(SRC_FILES_NTCORE_JAVA)))

HEADER_FILES := $(shell find . -name "*.h")
OUT_HAL := build/hal/libHAL.a
OUT_CPP := build/cpp/libwpilib.a
OUT_NTCORE := build/ntcore/libntcore-static.a
LIB_JAVA := build/java/wpilibJNI.dll
LIB_NTCORE_JAVA := build/java/ntcore.dll
OUT_JAVA := build/java/wpilib.jar
OUT_NTCORE_JAVA := build/java/ntcore.jar
OUT_WPIUTIL_JAVA := build/java/wpiutil.jar

all: cpp java hal
	
.PHONY: all cpp java hal clean patch

patch: wpilib.patch Makefile
	@echo -e "\e[1m\e[32mPATCH \e[39m$<\e[0m"
	@-cd allwpilib; patch -p1 < ../wpilib.patch

$(OUT_JAVA): $(LIB_JAVA) | build/java_class_files Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@cp /usr/x86_64-w64-mingw32/sys-root/mingw/bin/libstdc++-6.dll build/java
	@cp /usr/x86_64-w64-mingw32/sys-root/mingw/bin/libgcc_s_seh-1.dll build/java
	@cp /usr/x86_64-w64-mingw32/sys-root/mingw/bin/libwinpthread-1.dll build/java
	@$(JAR) cf $@ -C build/java/class .

$(LIB_JAVA): $(OBJ_FILES_JAVA) | patch Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -shared -o $@ $^ -Lbuild/hal -lHAL -lWs2_32 -lole32 -lshell32 -luuid

$(OUT_HAL): $(OBJ_FILES_HAL) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(AR) rcs $@ $^

$(OUT_CPP): $(OBJ_FILES_CPP) $(OUT_HAL) $(OUT_NTCORE) |	patch Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(AR) rcs $@ $^

$(OUT_NTCORE): $(OBJ_FILES_NTCORE) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(AR) rcs $@ $^

$(LIB_NTCORE_JAVA): $(OBJ_FILES_NTCORE_JAVA) | build/ntcore_java_class_files $(OUT_NTCORE) $(OUT_HAL) Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -shared -o $@ $^ -Lbuild/ntcore -Lbuild/hal -lntcore-static -lHAL -lWs2_32 -lole32 -lshell32 -luuid

$(OUT_NTCORE_JAVA): | $(LIB_NTCORE_JAVA) build/ntcore_java_class_files $(OUT_NTCORE) Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(JAR) cf $@ -C build/ntcore/java/class .

$(OUT_WPIUTIL_JAVA): | $(LIB_WPIUTIL_JAVA) build/wpiutil_java_class_files $(OUT_NTCORE) Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@mkdir -p $(dir $@)
	@$(JAR) cf $@ -C build/wpiutil/java/class .

build/cpp/%.o: %.cpp patch Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_CPP)

build/hal/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_HAL)

build/java/%.o: %.cpp patch Makefile $(HEADER_FILES) build/java_class_files
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_JAVA)

build/ntcore/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_NTCORE)

build/ntcore/java/%.o: %.cpp build/ntcore_java_class_files Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_NTCORE_JAVA)

build/java_class_files: $(JAVA_FILES_JAVA) | patch $(OUT_WPIUTIL_JAVA) $(OUT_NTCORE_JAVA) Makefile
	@echo -e "\e[1m\e[32mJAVA \e[39mwpilibj/*.java\e[0m"
	@mkdir -p build/java/class
	@mkdir -p build/java/header
	@$(JAVAC) -cp "$(OUT_NTCORE_JAVA);$(OUT_WPIUTIL_JAVA)" -d build/java/class -h build/java/header $^
	@touch $@

build/ntcore_java_class_files: $(JAVA_FILES_NTCORE_JAVA) | $(OUT_WPIUTIL_JAVA) Makefile
	@echo -e "\e[1m\e[32mJAVA \e[39mntcore/*.java\e[0m"
	@mkdir -p build/ntcore/java/class
	@mkdir -p build/ntcore/java/header
	@$(JAVAC) -cp $(OUT_WPIUTIL_JAVA) -d build/ntcore/java/class -h build/ntcore/java/header $^
	@touch $@

build/wpiutil_java_class_files: $(JAVA_FILES_WPIUTIL_JAVA) | Makefile
	@echo -e "\e[1m\e[32mJAVA \e[39mwpiutil/*.java\e[0m"
	@mkdir -p build/wpiutil/java/class
	@$(JAVAC) -d build/wpiutil/java/class $^
	@touch $@

cpp: $(OUT_CPP)

java: $(OUT_JAVA)

hal: $(OUT_HAL)

install: $(OUT_CPP) $(OUT_JAVA)
	@echo -e "\e[1m\e[32mINSTALL\e[39m include\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/include
	@cp -rf include ../../installer/SynthesisDrive/include
	@echo -e "\e[1m\e[32mINSTALL\e[39m include/ni-libraries\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/include/ni-libraries
	@cp -rf allwpilib/ni-libraries/include ../../installer/SynthesisDrive/include/ni-libraries/include
	@echo -e "\e[1m\e[32mINSTALL\e[39m include/wpilibc/athena\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/include/wpilibc/athena
	@mkdir -p ../../installer/SynthesisDrive/include/wpilibc/athena/vision
	@cp -rf allwpilib/wpilibc/src/main/native/include ../../installer/SynthesisDrive/include/wpilibc/athena/include
	@cp -rf CameraServer.h ../../installer/SynthesisDrive/include/wpilibc/athena/include
	@cp -rf VisionRunner.h ../../installer/SynthesisDrive/include/wpilibc/athena/include/vision
	@echo -e "\e[1m\e[32mINSTALL\e[39m include/wpilibc/shared\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/include/wpilibc/shared
	@cp -rf allwpilib/wpilibc/src/main/native/include ../../installer/SynthesisDrive/include/wpilibc/shared/include
	@echo -e "\e[1m\e[32mINSTALL\e[39m include/wpiutil\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/include/wpiutil/src/main/native
	@cp -rf wpiutil/src/main/native/include ../../installer/SynthesisDrive/include/wpiutil/src/main/native/include
	@echo -e "\e[1m\e[32mINSTALL\e[39m include/ntcore\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/include/ntcore
	@cp -rf ntcore/src/main/native/include ../../installer/SynthesisDrive/include/ntcore/include
	@echo -e "\e[1m\e[32mINSTALL\e[39m lib/hal\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/lib/hal
	@cp -rf build/hal/libHAL.a ../../installer/SynthesisDrive/lib/hal/libHAL.a
	@echo -e "\e[1m\e[32mINSTALL\e[39m lib/ntcore\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/lib/ntcore
	@cp -rf build/ntcore/libntcore-static.a ../../installer/SynthesisDrive/lib/ntcore/libntcore-static.a
	@echo -e "\e[1m\e[32mINSTALL\e[39m lib/cpp\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/lib/cpp
	@cp -rf build/cpp/libwpilib.a ../../installer/SynthesisDrive/lib/cpp/libwpilib.a
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/wpilib.jar\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/jars
	@cp -rf build/java/wpilib.jar ../../installer/SynthesisDrive/jars/wpilib.jar
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/ntcore.jar\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/jars
	@cp -rf build/java/ntcore.jar ../../installer/SynthesisDrive/jars/ntcore.jar
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/wpiutil.jar\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/jars
	@cp -rf build/java/wpiutil.jar ../../installer/SynthesisDrive/jars/wpiutil.jar
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/wpilibJNI.dll\e[0m"
	@mkdir -p ../../installer/SynthesisDrive/jars
	@cp -rf build/java/wpilibJNI.dll ../../installer/SynthesisDrive/jars/wpilibJNI.dll
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/ntcore.dll\e[0m"
	@cp -rf build/java/ntcore.dll ../../installer/SynthesisDrive/jars/
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/libstc++-6.dll\e[0m"
	@cp -rf build/java/libstdc++-6.dll ../../installer/SynthesisDrive/jars/
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/libgcc_s_seh.dll\e[0m"
	@cp -rf build/java/libgcc_s_seh-1.dll ../../installer/SynthesisDrive/jars/
	@echo -e "\e[1m\e[32mINSTALL\e[39m jars/libwinpthread-1.dll\e[0m"
	@cp -rf build/java/libwinpthread-1.dll ../../installer/SynthesisDrive/jars/

clean:
	rm -r build
