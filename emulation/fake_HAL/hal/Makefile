CC = /frc/bin/arm-frc-linux-gnueabi-g++.exe
CCFLAGS = -fPIC -Iinclude -Ini-libraries/include -I$(FRC_PATH)/usr/include -Iwpiutil/include -std=c++11 -Wpedantic

SRC_FILES = \
	lib/athena/Accelerometer.cpp \
	lib/athena/AnalogAccumulator.cpp \
	lib/athena/AnalogGyro.cpp \
	lib/athena/AnalogInput.cpp \
	lib/athena/AnalogInternal.cpp \
	lib/athena/AnalogTrigger.cpp \
	lib/athena/Compressor.cpp \
	lib/athena/Constants.cpp \
	lib/athena/Counter.cpp \
	lib/athena/DigitalInternal.cpp \
	lib/athena/DIO.cpp \
	lib/athena/Encoder.cpp \
	lib/athena/FPGAEncoder.cpp \
	lib/athena/FRCDriverStation.cpp \
	lib/athena/HAL.cpp \
	lib/athena/I2C.cpp \
	lib/athena/Interrupts.cpp \
	lib/athena/Notifier.cpp \
	lib/athena/OSSerialPort.cpp \
	lib/athena/PCMInternal.cpp \
	lib/athena/PDP.cpp \
	lib/athena/Ports.cpp \
	lib/athena/Power.cpp \
	lib/athena/PWM.cpp \
	lib/athena/Relay.cpp \
	lib/athena/SerialPort.cpp \
	lib/athena/Solenoid.cpp \
	lib/athena/SPI.cpp \
	lib/athena/Threads.cpp \
	lib/athena/cpp/priority_mutex.cpp \
	lib/athena/cpp/SerialHelper.cpp \
	lib/athena/cpp/StateNetworkServer.cpp \
	lib/athena/cpp/Synchronized.cpp \
	lib/athena/cpp/System.cpp \
	lib/athena/cpp/Task.cpp 

OBJ_FILES = $(SRC_FILES:.cpp=.o)
OUT = build/libHALAthena.so

!MESSAGE $(OBJ_FILES)
.cpp.o:
	$(CC) -c $*.cpp -o $@ $(CCFLAGS)

$(OUT): $(OBJ_FILES)
	$(CC) -shared $** -o $@ $(CCFLAGS)

all: $(OUT)

clean:
	del $(OBJ_FILES)
	del $(OUT)
