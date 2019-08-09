#include "testing.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

TEST(PWMTest, SetSpeed){
    constexpr unsigned SIZE = nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters +
        nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumMXPRegisters;

    for(unsigned i = 0; i < SIZE; i++){
        double power = 2.0 * i / SIZE - 1.0;
        frc::PWM pwm = frc::PWM(i);
        pwm.SetSpeed(power);

        EXPECT_NEAR(power, pwm.GetSpeed(), EPSILON);

        auto instance = hel::RoboRIOManager::getInstance();
        EXPECT_NEAR(power, hel::PWMSystem::getPercentOutput(pwm.GetRaw()), EPSILON);
        instance.second.unlock();
    }
}
