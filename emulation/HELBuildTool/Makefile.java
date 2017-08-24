CC = x86_64-w64-mingw32-g++

ifndef JAVA_HOME
	JAVA_VERSION := $(shell java -version |& sed -ne 's/^java version "\([^"]*\)"/\1/gp')
	JAVA_HOME := /cygdrive/c/Program\ Files/java/jdk$(JAVA_VERSION)
endif

JAVAC := $(JAVA_HOME)/bin/javac.exe
JAR := $(JAVA_HOME)/bin/jar.exe

ifndef SYNTHESIS_JARS
	SYNTHESIS_JARS := /cygdrive/c/Program Files (x86)/Autodesk/Synthesis/SynthesisDrive/jars
endif

JAVA_FILES := $(shell find ./ -name "*.java")

.PHONY: java_class_files run

run: build/java_class_files
	@cp "$(SYNTHESIS_JARS)/libstdc++-6.dll" ./build/classes
	@cp "$(SYNTHESIS_JARS)/libgcc_s_seh-1.dll" ./build/classes
	@cp "$(SYNTHESIS_JARS)/libwinpthread-1.dll" ./build/classes
	@cd build/classes; export PATH=".:$(PATH)"; export ROBOT_CLASS="$(ENTRY_POINT)"; $(JAVA_HOME)/bin/java.exe -Djava.library.path="$(shell cygpath -w '$(SYNTHESIS_JARS)')" -cp "$(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/ntcore.jar');." edu.wpi.first.wpilibj.RobotBase

build/FRC_UserProgram.jar: build/java_class_files
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@mkdir -p build
	@echo "Class-Path: $(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '${SYNTHESIS_JARS}/ntcore.jar');$@" > build/Manifest.txt
	@cd build/classes; $(JAR) cfem ../../$@ $(ENTRY_POINT) ../Manifest.txt .

build/java_class_files: $(JAVA_FILES)
	@echo -e "\e[1m\e[32mJAVA \e[39m*.java\e[0m"
	@mkdir -p build/classes
	@$(JAVAC) -cp "$(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/ntcore.jar')" -d build/classes $^
	@touch $@

