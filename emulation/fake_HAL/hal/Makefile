CC = cl#/frc/bin/arm-frc-linux-gnueabi-g++.exe
CCFLAGS = -fPIC -Iinclude -Ini-libraries/include -I$(FRC_PATH)/usr/include -Iwpiutil/include -Ilib -std=c++11

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
	lib/StateNetwork/StateNetworkServer.cpp \
	lib/OSAL/Synchronized.cpp \
	lib/OSAL/System.cpp \
	lib/OSAL/Task.cpp \
	lib/crc32.cpp \
	lib/ErrorBase.cpp \
	lib/Error.cpp \
	lib/Timer.cpp \
	lib/Utility.cpp \
	lib/ChipObject/NiFpga.cpp \
	lib/ChipObject/NiFpgaState.cpp \
	lib/ChipObject/NiIRQImpl.cpp \
	lib/ChipObject/niLoadOut.cpp \
	lib/ChipObject/NiRio.cpp \
	lib/ChipObject/nUsageReporting.cpp \
	lib/ChipObject/PWMDecoder.cpp \
	lib/ChipObject/tAccumulatorImpl.cpp \
	lib/ChipObject/tAIImpl.cpp \
	lib/ChipObject/tAlarmImpl.cpp \
	lib/ChipObject/tAnalogTriggerImpl.cpp \
	lib/ChipObject/tCounterImpl.cpp \
	lib/ChipObject/tDIOImpl.cpp \
	lib/ChipObject/tEncoderImpl.cpp \
	lib/ChipObject/tGlobalImpl.cpp \
	lib/ChipObject/tInterruptImpl.cpp \
	lib/ChipObject/tInterruptManager.cpp \
	lib/ChipObject/tSolenoidImpl.cpp \
	lib/ChipObject/tSystem.cpp \
	lib/ChipObject/tWatchcatImpl.cpp 

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
