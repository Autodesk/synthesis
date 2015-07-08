/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2014. All Rights Reserved.                             */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in the root directory of */
/* the project.                                                               */
/*----------------------------------------------------------------------------*/

#include "WPILib.h"
#include "gtest/gtest.h"
#include "TestBench.h"

static const double kMotorDelay = 2.5;

static const double kMaxPeriod = 2.0;

class CounterTest : public testing::Test {
protected:
  Counter *m_talonCounter;
  Counter *m_victorCounter;
  Counter *m_jaguarCounter;
  Talon *m_talon;
  Victor *m_victor;
  Jaguar *m_jaguar;

  virtual void SetUp() {
    m_talonCounter = new Counter(TestBench::kTalonEncoderChannelA);
    m_victorCounter = new Counter(TestBench::kVictorEncoderChannelA);
    m_jaguarCounter = new Counter(TestBench::kJaguarEncoderChannelA);
    m_victor = new Victor(TestBench::kVictorChannel);
    m_talon = new Talon(TestBench::kTalonChannel);
    m_jaguar = new Jaguar(TestBench::kJaguarChannel);
  }

  virtual void TearDown() {
    delete m_talonCounter;
    delete m_victorCounter;
    delete m_jaguarCounter;
    delete m_victor;
    delete m_talon;
    delete m_jaguar;
    }

    void Reset() {
      m_talonCounter->Reset();
      m_victorCounter->Reset();
      m_jaguarCounter->Reset();
      m_talon->Set(0.0f);
      m_victor->Set(0.0f);
      m_jaguar->Set(0.0f);
    }
};

/**
 * Tests the counter by moving the motor and determining if the
 * counter is counting.
 */
TEST_F(CounterTest, CountTalon) {
  Reset();
  /* Run the motor forward and determine if the counter is counting. */
  m_talon->Set(1.0f);
  Wait(0.5);
  EXPECT_NE(0.0f, m_talonCounter->Get())
    << "The counter did not count (talon)";
  /* Set the motor to 0 and determine if the counter resets to 0. */
  m_talon->Set(0.0f);
  Wait(0.5);
  m_talonCounter->Reset();
  EXPECT_FLOAT_EQ(0.0f, m_talonCounter->Get())
    << "The counter did not restart to 0 (talon)";
}

TEST_F(CounterTest, CountVictor) {
  Reset();
  /* Run the motor forward and determine if the counter is counting. */
  m_victor->Set(1.0f);
  Wait(0.5);
  EXPECT_NE(0.0f, m_victorCounter->Get())
    << "The counter did not count (victor)";
  /* Set the motor to 0 and determine if the counter resets to 0. */
  m_victor->Set(0.0f);
  Wait(0.5);
  m_victorCounter->Reset();
  EXPECT_FLOAT_EQ(0.0f, m_victorCounter->Get())
    << "The counter did not restart to 0 (jaguar)";
}

TEST_F(CounterTest, CountJaguar) {
  Reset();
  /* Run the motor forward and determine if the counter is counting. */
  m_jaguar->Set(1.0f);
  Wait(0.5);
  EXPECT_NE(0.0f, m_jaguarCounter->Get())
    << "The counter did not count (jaguar)";
  /* Set the motor to 0 and determine if the counter resets to 0. */
  m_jaguar->Set(0.0f);
  Wait(0.5);
  m_jaguarCounter->Reset();
  EXPECT_FLOAT_EQ(0.0f, m_jaguarCounter->Get())
    << "The counter did not restart to 0 (jaguar)";
}

/**
 * Tests the GetStopped and SetMaxPeriod methods by setting the Max Period and
 * getting the value after a period of time.
 */
TEST_F(CounterTest, TalonGetStopped) {
  Reset();
  /* Set the Max Period of the counter and run the motor */
  m_talonCounter->SetMaxPeriod(kMaxPeriod);
  m_talon->Set(1.0f);
  Wait(0.5);
  EXPECT_FALSE(m_talonCounter->GetStopped())
    << "The counter did not count.";
  /* Stop the motor and wait until the Max Period is exceeded */
  m_talon->Set(0.0f);
  Wait(kMotorDelay);
  EXPECT_TRUE(m_talonCounter->GetStopped())
    << "The counter did not stop counting.";
}

TEST_F(CounterTest, VictorGetStopped) {
  Reset();
  /* Set the Max Period of the counter and run the motor */
  m_victorCounter->SetMaxPeriod(kMaxPeriod);
  m_victor->Set(1.0f);
  Wait(0.5);
  EXPECT_FALSE(m_victorCounter->GetStopped())
    << "The counter did not count.";
  /* Stop the motor and wait until the Max Period is exceeded */
  m_victor->Set(0.0f);
  Wait(kMotorDelay);
  EXPECT_TRUE(m_victorCounter->GetStopped())
    << "The counter did not stop counting.";
}

TEST_F(CounterTest, JaguarGetStopped) {
  Reset();
  /* Set the Max Period of the counter and run the motor */
  m_jaguarCounter->SetMaxPeriod(kMaxPeriod);
  m_jaguar->Set(1.0f);
  Wait(0.5);
  EXPECT_FALSE(m_jaguarCounter->GetStopped())
    << "The counter did not count.";
  /* Stop the motor and wait until the Max Period is exceeded */
  m_jaguar->Set(0.0f);
  Wait(kMotorDelay);
  EXPECT_TRUE(m_jaguarCounter->GetStopped())
    << "The counter did not stop counting.";
}
