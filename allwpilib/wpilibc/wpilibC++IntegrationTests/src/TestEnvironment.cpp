/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2014. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "gtest/gtest.h"
#include "WPILib.h"

class TestEnvironment : public testing::Environment {
	bool m_alreadySetUp;

public:
	TestEnvironment(): m_alreadySetUp(false) {}

	virtual void SetUp() {
		/* Only set up once.  This allows gtest_repeat to be used to
			automatically repeat tests. */
		if(m_alreadySetUp) return;
		m_alreadySetUp = true;

		if(!HALInitialize()) {
			std::cerr << "FATAL ERROR: HAL could not be initialized" << std::endl;
			exit(-1);
		}

		/* This sets up the network communications library to enable the driver
			station. After starting network coms, it will loop until the driver
			station returns that the robot is enabled, to ensure that tests
			will be able to run on the hardware. */
		HALNetworkCommunicationObserveUserProgramStarting();
		LiveWindow::GetInstance()->SetEnabled(false);

		std::cout << "Waiting for enable" << std::endl;

		while(!DriverStation::GetInstance()->IsEnabled()) {
			Wait(0.1);
		}
	}

	virtual void TearDown() {
	}
};

testing::Environment *const environment = testing::AddGlobalTestEnvironment(new TestEnvironment);
