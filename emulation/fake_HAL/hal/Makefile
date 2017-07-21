CC = x86_64-w64-mingw32-g++
AR = x86_64-w64-mingw32-ar
TARGET = debug
CCFLAGS := -D_WIN32 -D_WIN32_WINNT=0x0602 -Iwpilibc/shared/include -Iinclude -Ini-libraries/include -Iwpiutil/include -Ilib -Iwpilibc/athena/include -Intcore/include -std=c++14
LDFLADS := -lws2_32

ifeq ($(TARGET), debug)
	CCFLAGS += -O0 -g
endif
ifeq ($(TARGET), release)
	CCflags += -O2
endif

SRC_FILES := $(shell find . -name "*.cpp" -not -name "*priority_mutex.cpp" -not -wholename "*ctre/*" -not -name "*NiFpgaState.cpp" -not -name "VisionRunner.cpp")
SRC_FILES := $(filter-out lib/athena/cpp/priority_mutex.cpp,$(SRC_FILES))
OBJ_FILES := $(addprefix build/,$(patsubst %.cpp,%.o,$(SRC_FILES)))
HEADER_FILES := $(shell find . -name "*.h")
OUT := build/HAL.a

$(OUT): $(OBJ_FILES) | Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[34mAR \e[39m$@\e[0m"
	@#$(CC) -static $^ -o $@ $(CCFLAGS) $(LDFLAGS)
	@$(AR) rcs $@ $^

build/%.o: %.cpp Makefile $(HEADER_FILES)
	@echo -e "\e[1m\e[32mCC \e[39m$<\e[0m"
	@mkdir	-p $(dir $@)
	@$(CC) -c $< -o $@ $(CCFLAGS)

all: $(OUT)

clean:
	rm -r $(OBJ_FILES)
	rm -r $(OUT)
