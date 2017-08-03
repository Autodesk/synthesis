CC = x86_64-w64-mingw32-g++
ifndef TARGET
	TARGET := debug
endif

ifndef SYNTHESIS_LIBS
ifeq ($(TARGET), dev)
	SYNTHESIS_LIBS := /home/t_leeb/synthesis/emulation/hel/build/
else
	SYNTHESIS_LIBS := /cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/lib
endif
endif
ifndef SYNTHESIS_INCLUDES
ifeq ($(TARGET), dev)
	SYNTHESIS_INCLUDES := /home/t_leeb/synthesis/emulation/hel
else
	SYNTHESIS_INCLUDES := /cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/include
endif
endif

CCFLAGS := -L$(SYNTHESIS_LIBS)/ntcore -L$(SYNTHESIS_LIBS)/cpp -L$(SYNTHESIS_LIBS)/hal -lwpilib -lntcore-static -lHAL -lWs2_32 -I$(SYNTHESIS_INCLUDES)/include -I$(SYNTHESIS_INCLUDES)/ni-libraries/include -I$(SYNTHESIS_INCLUDES)/wpilibc/athena/include -I$(SYNTHESIS_INCLUDES)/wpilibc/shared/include -I$(SYNTHESIS_INCLUDES)/wpiutil/src/main/native/include/ -I$(SYNTHESIS_INCLUDES)/ntcore/include -std=c++11
ifeq ($(TARGET), debug)
	CCFLAGS :=	$(CCFLAGS) -O0 -g
else
	CCFLAGs := $(CCFLAGS) -O2
endif

SRC_FILES = $(shell find ./ -name "*.cpp")
HEADER_FILES = $(shell find ./ -name "*.h")
OBJ_FILES =	$(addprefix build/,$(patsubst %.cpp,%.o,$(SRC_FILES)))

all: build/FRC_UserProgram

build/FRC_UserProgram: $(OBJ_FILES) | $(HEADER_FILES) Makefile
	@echo -e "\e[1m\e[32mLD \e[39m$@\e[0m"
	@$(CC) -static -o $@ $^ $(CCFLAGS)

build/%.o: %.cpp | $(HEADER_FILES) Makefile
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir -p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS)
