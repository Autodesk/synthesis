CC = x86_64-w64-mingw32-g++

#default to no optimizationss
ifndef TARGET
	TARGET := debug
endif

#install directory
SYNTHESIS_DIR := \
	/cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/

#path to dynamic libraries
ifndef SYNTHESIS_LIBS
	SYNTHESIS_LIBS := $(SYNTHESIS_DIR)/lib
endif

#path to header files
ifndef SYNTHESIS_INCLUDES
	SYNTHESIS_INCLUDES := $(SYNTHESIS_DIR)/include
endif

#dynamic library directories
CCFLAGS := -m64 \
	-L$(SYNTHESIS_LIBS)/ntcore \
	-L$(SYNTHESIS_LIBS)/cpp \
	-L$(SYNTHESIS_LIBS)/hal
#dynamic libraries
CCFLAGS += -lwpilib -lntcore-static -lHAL -lWs2_32 -lshell32 -lole32 -luuid
#header directories
CCFLAGS += -I$(SYNTHESIS_INCLUDES)/allwpilib/wpilibc/athena/include \
	-I$(SYNTHESIS_INCLUDES)/allwpilib/wpilibc/shared/include \
	-I$(SYNTHESIS_INCLUDES)/wpilibc/athena/include \
	-I$(SYNTHESIS_INCLUDES)/wpilibc/shared/include \
	-I$(SYNTHESIS_INCLUDES)/include \
	-I$(SYNTHESIS_INCLUDES)/allwpilib/ni-libraries/include \
	-I$(SYNTHESIS_INCLUDES)/ni-libraries/include \
	-I$(SYNTHESIS_INCLUDES)/wpiutil/src/main/native/include \
	-I$(SYNTHESIS_INCLUDES)/ntcore/include -std=c++11 \
	-Iinclude

#turn on optimizations if in release mode, or debug info if in debug mode
ifeq ($(TARGET), debug)
	CCFLAGS :=	$(CCFLAGS) -O0 -g
else
	CCFLAGs := $(CCFLAGS) -O2
endif

#find the source and header files in the user code directory
#the headder files are needed to ddetect changes
SRC_FILES = $(shell find ./ -name "*.cpp")
HEADER_FILES = $(shell find ./ -name "*.h")
#the compiled objecct file names
OBJ_FILES =	$(addprefix build/,$(patsubst %.cpp,%.o,$(SRC_FILES)))

.PHONY: build/FRC_UserProgram clean

all: build/FRC_UserProgram

#link all the object files together into the executable
build/FRC_UserProgram: $(OBJ_FILES) | $(HEADER_FILES)
	@mkdir -p build
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@$(CC) -static -o $@ $^ $(CCFLAGS)

#compile an individual object file
build/%.o: %.cpp | $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) $(CCFLAGS) -c $< -o $@

#remove the build directory
clean:
	rm -r build
