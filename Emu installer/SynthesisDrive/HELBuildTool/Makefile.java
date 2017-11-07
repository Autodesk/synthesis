CC = x86_64-w64-mingw32-g++

#finds the java directory, if it wasn't specified as an environment variable
ifndef JAVA_HOME
	JAVA_VERSION := $(shell java -version |& \
			sed -ne 's/^java version "\([^"]*\)"/\1/gp')
	JAVA_HOME := /cygdrive/c/Program\ Files/java/jdk$(JAVA_VERSION)
endif

#java tools
JAVAC := $(JAVA_HOME)/bin/javac.exe
JAR := $(JAVA_HOME)/bin/jar.exe

#install directory
SYNTHESIS_DIR := \
	/cygdrive/c/Program Files (x86)/Autodesk/Synthesis/SynthesisDrive

#jar directory in the synthesis in stall
#in debug mode this is specified by an envvironment variable relative to the
#synthesis repository
ifndef SYNTHESIS_JARS
	SYNTHESIS_JARS := $(SYNTHESIS_DIR)/jars
endif

#finds all the java source code
JAVA_FILES := $(shell find ./ -name "*.java")

.PHONY: run

#run the user code after building the class files
run: build/java_class_files
	@echo 'Starting robot code'
	@#dlls needed because mingw has a few runtime dependencies
	@cp "$(SYNTHESIS_JARS)/libstdc++-6.dll" ./build/classes
	@cp "$(SYNTHESIS_JARS)/libgcc_s_seh-1.dll" ./build/classes
	@cp "$(SYNTHESIS_JARS)/libwinpthread-1.dll" ./build/classes
	@#run from build/classes
	@cd build/classes; \
		export PATH=".:$(PATH)"; \
		export ROBOT_CLASS="$(ENTRY_POINT)"; \
		$(JAVA_HOME)/bin/java.exe \
		-Djava.library.path="$(shell cygpath -w '$(SYNTHESIS_JARS)')" \
		-cp "$(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/ntcore.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/wpiutil.jar');." \
		edu.wpi.first.wpilibj.RobotBase

#build the class files
#the target name here is a hack to make sure that it doesn't rebuild the java
#files when it doesn't have to.
build/java_class_files: $(JAVA_FILES)
	@echo -e "\e[1m\e[32mJAVA \e[39m*.java\e[0m"
	@mkdir -p build/classes
	@$(JAVAC) -cp "$(shell cygpath -w '$(SYNTHESIS_JARS)/wpilib.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/ntcore.jar');$(shell cygpath -w '$(SYNTHESIS_JARS)/wpiutil.jar')" \
	-d build/classes $^
	@touch $@

