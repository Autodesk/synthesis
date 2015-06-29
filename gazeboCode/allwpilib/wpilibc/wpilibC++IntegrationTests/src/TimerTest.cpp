/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2014. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "WPILib.h"
#include "gtest/gtest.h"
#include "TestBench.h"

static const double kWaitTime = 0.5;

class TimerTest : public testing::Test {
protected:
	Timer *m_timer;
	
	virtual void SetUp() {
		m_timer = new Timer;
	}
	
	virtual void TearDown() {
		delete m_timer;
	}
	
	void Reset() {
		m_timer->Reset();
	}
};

/**
 * Test if the Wait function works
 */
TEST_F(TimerTest, Wait) {
	Reset();
	
	double initialTime = m_timer->GetFPGATimestamp();
	
	Wait(kWaitTime);
	
	double finalTime = m_timer->GetFPGATimestamp();
	
	EXPECT_NEAR(kWaitTime, finalTime - initialTime, 0.001);
}

