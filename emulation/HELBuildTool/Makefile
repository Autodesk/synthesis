CC = i686-w64-mingw32-g++
ifndef TARGET
	TARGET := debug
endif

ifndef SYNTHESIS_LIBS
	SYNTHESIS_LIBS := /cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/lib
endif
ifndef SYNTHESIS_INCLUDES
	SYNTHESIS_INCLUDES := /cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/include
endif

CCFLAGS := -m32 -L$(SYNTHESIS_LIBS)/ntcore -L$(SYNTHESIS_LIBS)/cpp -L$(SYNTHESIS_LIBS)/hal \
	-lwpilib -lntcore-static -lHAL -lWs2_32 \
	-I$(SYNTHESIS_INCLUDES)/allwpilib/wpilibc/athena/include -I$(SYNTHESIS_INCLUDES)/allwpilib/wpilibc/shared/include \
	-I$(SYNTHESIS_INCLUDES)/wpilibc/athena/include -I$(SYNTHESIS_INCLUDES)/wpilibc/shared/include \
	-I$(SYNTHESIS_INCLUDES)/include \
	-I$(SYNTHESIS_INCLUDES)/allwpilib/ni-libraries/include \
	-I$(SYNTHESIS_INCLUDES)/ni-libraries/include \
	-I$(SYNTHESIS_INCLUDES)/wpiutil/src/main/native/include/ \
	-I$(SYNTHESIS_INCLUDES)/ntcore/include -std=c++11 \
	-Iinclude
ifeq ($(TARGET), debug)
	CCFLAGS :=	$(CCFLAGS) -O0 -g
else
	CCFLAGs := $(CCFLAGS) -O2
endif

SRC_FILES = $(shell find ./ -name "*.cpp")
HEADER_FILES = $(shell find ./ -name "*.h")
OBJ_FILES =	$(addprefix build/,$(patsubst %.cpp,%.o,$(SRC_FILES)))

.PHONY: build/FRC_UserProgram

all: build/FRC_UserProgram

build/FRC_UserProgram: $(OBJ_FILES) | $(HEADER_FILES)
	@mkdir -p build
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@$(CC) -static -o $@ $^ $(CCFLAGS)

build/teamID.o: /cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/HELBuildTool/teamID.cpp | $(HEADER_FILES)

build/%.o: %.cpp | $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) $(CCFLAGS) -c $< -o $@
