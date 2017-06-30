CC := $(FRC_PATH)/bin/arm-frc-linux-gnueabi-g++.exe
CCFLAGS := -Iinclude

SRC_FILES := athena/PWM.cpp
OBJ_FILES := $(addprefix build/,$(subst .cpp,.o,$(SRC_FILES)))
OUT := build/libHALAthena.so

build/%.o: lib/%.cpp
	mkdir -p build/$(dir $@)
	$(CC) -fPIC -c $< -o $@ $(CCFLAGS)

$(OUT): $(OBJ_FILES)
	$(CC) -shared $^ -o $@ $(CCFLAGS)

all: $(OUT)

clean:
	rm -r build/*
