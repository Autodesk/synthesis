CC = x86_64-w64-mingw32-g++
AR = x86_64-w64-mingw32-ar
TARGET = debug

ifndef JAVA_HOME
	JAVA_VERSION := $(shell java -version |& sed -ne 's/^java version "\([^"]*\)"/\1/gp')
	JAVA_HOME := /cygdrive/c/Program\ Files/java/jdk$(JAVA_VERSION)
endif

JAVAC := $(JAVA_HOME)/bin/javac.exe

CCFLAGS := -D_WIN32 -D_WIN32_WINNT=0x0602 -Iinclude -Ini-libraries/include -Iwpiutil/include -std=c++14	
ifeq ($(TARGET), debug)
	CCFLAGS += -O0 -g
endif
ifeq ($(TARGET), release)
	CCflags += -O2
endif

CCFLAGS_CPP := -Iwpilibc/shared/include -Iwpilibc/athena/include -Intcore/include $(CCFLAGS)
CCFLAGS_HAL := -Ilib -Ilib/athena -Intcore/include $(CCFLAGS) -Iwpilibc/shared/include -Iwpilibc/athena/include

CCFLAGS_JAVA := -I$(JAVA_HOME)/include -I$(JAVA_HOME)/include/win32 -Ibuild/java/header $(CCFLAGS)

SRC_FILES_HAL := $(shell find ./lib ./wpiutil -name "*.cpp" -not -name "*priority_mutex.cpp" -not -wholename "*ctre/*" -not -name "*NiFpgaState.cpp")
SRC_FILES_CPP := $(shell find ./ntcore ./wpilibc -name "*.cpp" -not -name "VisionRunner.cpp")
SRC_FILES_JAVA := $(shell find ./wpilibj -name "*.cpp") ntcore/java/lib/NetworkTablesJNI.cpp
JAVA_FILES_JAVA := $(shell find ./wpilibj -name "*.java" -not -wholename "*sim/*" -not -wholename "*test/*" -not -name "CameraServer.java" -not -name "Vision*.java") $(shell find ./ntcore/java -name "*.java")

OBJ_FILES_CPP := $(addprefix build/cpp/,$(patsubst %.cpp,%.o,$(SRC_FILES_CPP)))
OBJ_FILES_JAVA := $(addprefix build/java/,$(patsubst %.cpp,%.o,$(SRC_FILES_JAVA)))
OBJ_FILES_HAL := $(addprefix build/hal/,$(patsubst %.cpp,%.o,$(SRC_FILES_HAL)))

HEADER_FILES := $(shell find . -name "*.h")
OUT_HAL := build/hal/libHAL.a
OUT_CPP := build/cpp/libwpilib.a
OUT_JAVA := build/java/wpilib.jar

all: cpp java hal
	
.PHONY: all cpp java java_class_files hal clean

$(OUT_JAVA): $(OBJ_FILES_JAVA) | $(OUT_HAL) Makefile $(HEADER_FILES)

$(OUT_HAL): $(OBJ_FILES_HAL) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@#$(CC) -static $^ -o $@ $(CCFLAGS) $(LDFLAGS)
	@$(AR) rcs $@ $^

$(OUT_CPP): $(OBJ_FILES_CPP) | $(OUT_HAL) Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@#$(CC) -static $^ -o $@ $(CCFLAGS) $(LDFLAGS)
	@$(AR) rcs $@ $^

build/cpp/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_CPP)

build/hal/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_HAL)

build/java/%.o: %.cpp Makefile $(HEADER_FILES) java_class_files
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_JAVA)

java_class_files: $(JAVA_FILES_JAVA) | Makefile
	@echo -e "\e[1m\e[32mJAVA \e[39m*.java\e[0m"
	@mkdir -p build/java/class
	@mkdir -p build/java/header
	@$(JAVAC) -d build/java/class -h build/java/header $^

cpp: $(OUT_CPP)

java: $(OUT_JAVA)

hal: $(OUT_HAL)

clean:
	rm -r build
