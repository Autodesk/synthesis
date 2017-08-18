CC = i686-w64-mingw32-g++

ifndef JAVA_HOME
	JAVA_VERSION := $(shell java -version |& sed -ne 's/^java version "\([^"]*\)"/\1/gp')
	JAVA_HOME := /cygdrive/c/Program\ Files/java/jdk$(JAVA_VERSION)
endif

JAVAC := $(JAVA_HOME)/bin/javac.exe
JAR := $(JAVA_HOME)/bin/jar.exe

ifndef SYNTHESIS_JARS
	SYNTHESIS_JARS := /cygdrive/c/Program\ Files\ \(x86\)/Autodesk/Synthesis/SynthesisDrive/jar
endif

JAVA_FILES := $(shell find ./ -name "*.java")

.PHONY: java_class_files run

run: build/FRC_UserProgram.jar
	java -cp "$(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/ntcore.jar')" -jar build/FRC_UserProgram.jar

build/FRC_UserProgram.jar: build/java_class_files
	@echo -e "\e[1m\e[32mJAR \e[39m$@\e[0m"
	@mkdir -p build
	@echo "Class-Path: $(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$@" > build/Manifest.txt
	@cd build/classes; $(JAR) cfem ../../$@ $(ENTRY_POINT) ../Manifest.txt .

build/java_class_files: $(JAVA_FILES)
	@echo -e "\e[1m\e[32mJAVA \e[39m*.java\e[0m"
	@mkdir -p build/classes
	@$(JAVAC) -cp "$(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/ntcore.jar')" -d build/classes $^
	@touch $@
