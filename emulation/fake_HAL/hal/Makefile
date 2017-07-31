CC := $(FRC_PATH)/bin/arm-frc-linux-gnueabi-g++.exe
CCFLAGS := -fPIC -Iinclude -Ini-libraries/include -I$(FRC_PATH)/usr/include -Iwpiutil/include -std=c++11 -Wpedantic

SRC_FILES := $(wildcard lib/athena/*.cpp) \
	$(wildcard lib/athena/cpp/*.cpp)
OBJ_FILES := $(patsubst lib/athena/%.cpp,build/%.o,$(SRC_FILES))
OUT := build/libHALAthena.so

build/%.o: lib/athena/%.cpp
	mkdir -p $(dir $@)
	$(CC) -c $< -o $@ $(CCFLAGS)

$(OUT): $(OBJ_FILES)
	$(CC) -shared $^ -o $@ $(CCFLAGS)

all: $(OUT)

clean:
	rm -r build/*
