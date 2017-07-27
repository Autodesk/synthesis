CC = x86_64-w64-mingw32-g++
AR = x86_64-w64-mingw32-ar
TARGET = debug

ifndef JAVA_HOME
	JAVA_VERSION := $(shell java -version |& sed -ne 's/^java version "\([^"]*\)"/\1/gp')
	JAVA_HOME := /cygdrive/c/Program\ Files/java/jdk$(JAVA_VERSION)
endif

JAVAC := $(JAVA_HOME)/bin/javac.exe
JAR := $(JAVA_HOME)/bin/jar.exe

CCFLAGS := -D_WIN32 -D_WIN32_WINNT=0x0602 -Iinclude -Ini-libraries/include -Iwpiutil/include -std=c++14	
ifeq ($(TARGET), debug)
	CCFLAGS += -O0 -g
endif
ifeq ($(TARGET), release)
	CCflags += -O2
endif

CCFLAGS_CPP := -Iwpilibc/shared/include -Iwpilibc/athena/include -Intcore/include $(CCFLAGS)
CCFLAGS_HAL := -Ilib -Ilib/athena -Intcore/include $(CCFLAGS) -Iwpilibc/shared/include -Iwpilibc/athena/include
CCFLAGS_NTCORE := -Intcore/include $(CCFLAGS)
CCFLAGS_JAVA := -I$(JAVA_HOME)/include -I$(JAVA_HOME)/include/win32 -Ibuild/java/header -Intcore/include -fpermissive $(CCFLAGS)
CCFLAGS_NTCORE_JAVA := -I$(JAVA_HOME)/include -I$(JAVA_HOME)/include/win32 -Ibuild/ntcore/java/header -Intcore/include $(CCFLAGS)

SRC_FILES_HAL := $(shell find ./lib ./wpiutil -name "*.cpp" -not -name "*priority_mutex.cpp" -not -wholename "*ctre/*" -not -name "*NiFpgaState.cpp")

SRC_FILES_NTCORE := $(shell find ./ntcore/src -name "*.cpp")
SRC_FILES_NTCORE_JAVA := ntcore/java/lib/NetworkTablesJNI.cpp
JAVA_FILES_NTCORE_JAVA := $(shell find ./ntcore/java -name "*.java")

SRC_FILES_CPP := $(shell find ./wpilibc -name "*.cpp" -not -name "VisionRunner.cpp")

SRC_FILES_JAVA := $(shell find ./wpilibj -name "*.cpp")

JAVA_FILES_JAVA := $(shell find ./wpilibj -name "*.java" -not -wholename "*sim/*" -not -wholename "*test/*" -not -name "CameraServer.java" -not -name "Vision*.java")

OBJ_FILES_CPP := $(addprefix build/cpp/,$(patsubst %.cpp,%.o,$(SRC_FILES_CPP)))
OBJ_FILES_JAVA := $(addprefix build/java/,$(patsubst %.cpp,%.o,$(SRC_FILES_JAVA)))
OBJ_FILES_HAL := $(addprefix build/hal/,$(patsubst %.cpp,%.o,$(SRC_FILES_HAL)))
OBJ_FILES_NTCORE := $(addprefix build/ntcore/,$(patsubst %.cpp,%.o,$(SRC_FILES_NTCORE)))
OBJ_FILES_NTCORE_JAVA := $(addprefix build/ntcore/java/,$(patsubst %.cpp,%.o,$(SRC_FILES_NTCORE_JAVA)))

HEADER_FILES := $(shell find . -name "*.h")
OUT_HAL := build/hal/libHAL.a
OUT_CPP := build/cpp/libwpilib.a
OUT_NTCORE := build/ntcore/libntcore-static.a
LIB_JAVA := build/java/wpilibJavaJNI.dll
LIB_NTCORE_JAVA := build/ntcore/java/ntcore.dll
OUT_JAVA := build/java/wpilib.jar
OUT_NTCORE_JAVA := build/ntcore/ntcore.jar

all: cpp java hal
	
.PHONY: all cpp java hal clean

$(OUT_JAVA): $(LIB_JAVA) | build/java_class_files Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@$(JAR) cf $@ -C build/java/class .

$(LIB_JAVA): $(OBJ_FILES_JAVA) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@$(CC) -shared -o $@ $^ -Lbuild/hal -lHAL -lWs2_32

$(OUT_HAL): $(OBJ_FILES_HAL) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@$(AR) rcs $@ $^

$(OUT_CPP): $(OBJ_FILES_CPP) $(OUT_HAL) |	Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@$(AR) rcs $@ $^

$(OUT_NTCORE): $(OBJ_FILES_NTCORE) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@$(AR) rcs $@ $^

$(LIB_NTCORE_JAVA): $(OBJ_FILES_NTCORE_JAVA) | build/ntcore_java_class_files $(OUT_NTCORE) $(OUT_HAL) Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@$(CC) -shared -o $@ $^ -Lbuild/ntcore -Lbuild/hal -lntcore-static -lHAL -lWs2_32

$(OUT_NTCORE_JAVA): | $(LIB_NTCORE_JAVA) build/ntcore_java_class_files $(OUT_NTCORE) Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@$(JAR) cf $@ -C build/ntcore/java/class .

build/cpp/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_CPP)

build/hal/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS_HAL)

build/java/%.o: %.cpp Makefile $(HEADER_FILES) build/java_class_files
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

build/java_class_files: $(JAVA_FILES_JAVA) | $(OUT_NTCORE_JAVA) Makefile
	@echo -e "\e[1m\e[32mJAVA \e[39mwpilibj/*.java\e[0m"
	@mkdir -p build/java/class
	@mkdir -p build/java/header
	@$(JAVAC) -cp $(OUT_NTCORE_JAVA) -d build/java/class -h build/java/header $^
	@touch $@

build/ntcore_java_class_files: $(JAVA_FILES_NTCORE_JAVA) | Makefile
	@echo -e "\e[1m\e[32mJAVA \e[39mntcore/*.java\e[0m"
	@mkdir -p build/ntcore/java/class
	@mkdir -p build/ntcore/java/header
	@$(JAVAC) -d build/ntcore/java/class -h build/ntcore/java/header $^
	@touch $@

cpp: $(OUT_CPP)

java: $(OUT_JAVA)

hal: $(OUT_HAL)

clean:
	rm -r build
