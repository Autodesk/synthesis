#!/usr/bin/env bash
#*----------------------------------------------------------------------------*#
#* Copyright (c) FIRST 2014. All Rights Reserved.							  *#
#* Open Source Software - may be modified and shared by FRC teams. The code   *#
#* must be accompanied by the FIRST BSD license file in the root directory of *#
#* the project.															      *#
#*----------------------------------------------------------------------------*#

# If this is changed, update the .gitignore
# so that test results are not commited to the repo
DEFAULT_LOCAL_TEST_RESULTS_DIR=../test-reports

ROBOT_ADDRESS=admin@roboRIO-190.local
DEFAULT_LOCAL_RUN_TEST_SCRIPT="run-tests-on-robot.sh"

DEFAULT_DESTINATION_DIR=/home/admin
DEFAULT_TEST_SCP_DIR=${DEFAULT_DESTINATION_DIR}/deployedTests
DEFAULT_DESTINATION_TEST_RESULTS_DIR=${DEFAULT_DESTINATION_DIR}/testResults

# C++ test variables
DEFAULT_CPP_TEST_NAME=FRCUserProgram
DEFAULT_CPP_TEST_ARGS="--gtest_color=yes"
if [ "$GRADLE" ]; then
    DEFAULT_LOCAL_CPP_TEST_FILE=../wpilibc/build/binaries/fRCUserProgramExecutable/FRCUserProgram
else
    DEFAULT_LOCAL_CPP_TEST_FILE=../cmake/target/cmake/wpilibc/wpilibC++IntegrationTests/FRCUserProgram
fi

CPP_REPORT=cppreport.xml
DEFAULT_LOCAL_CPP_TEST_RESULT=${DEFAULT_LOCAL_TEST_RESULTS_DIR}/${CPP_REPORT}
DEFAULT_DESTINATION_CPP_TEST_RESULTS=${DEFAULT_DESTINATION_TEST_RESULTS_DIR}/${CPP_REPORT}

# Java test variables
DEFAULT_JAVA_TEST_NAME=FRCUserProgram.jar
DEFAULT_JAVA_TEST_ARGS=""

if [ "$GRADLE" ]; then
    DEFAULT_LOCAL_JAVA_TEST_FILE=../wpilibj/build/libs/wpilibJavaIntegrationTests-0.1.0-SNAPSHOT.jar
else
    DEFAULT_LOCAL_JAVA_TEST_FILE=../wpilibj/wpilibJavaIntegrationTests/target/wpilibJavaIntegrationTests-0.1.0-SNAPSHOT.jar
fi

JAVA_REPORT=javareport.xml
DEFAULT_LOCAL_JAVA_TEST_RESULT=${DEFAULT_LOCAL_TEST_RESULTS_DIR}/${JAVA_REPORT}
DEFAULT_DESTINATION_JAVA_TEST_RESULTS=${DEFAULT_DESTINATION_TEST_RESULTS_DIR}/AntReports/TEST-edu.wpi.first.wpilibj.test.TestSuite.xml



