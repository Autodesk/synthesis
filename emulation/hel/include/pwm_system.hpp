#ifndef _PWM_SYSTEM_HPP_
#define _PWM_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPWM.h"

#include <array>

namespace hel{
    /**
     * \struct PWMSystem
     * \brief Data model for PWM system.
     * Data model for all PWMS. Holds all internal data for PWMs.
     */

    struct PWMSystem{
        static constexpr const int32_t EXPECTED_LOOP_TIMING = 40;
    private:

        /**
         * \var tConfig config
         * \brief Current PWM system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig config;

        /**
         * \struct PWM
         * \brief Data model for individual PWM
         * Data model used for storing data about an individual PWM.
         */

        struct PWM{

            /**
             * \var uint32_t period_scale
             * \brief 2 bit PWM signal mask.
             * 2-bit mask for signal masking frequency, effectively scaling the PWM value (0 = 1x 1, = 2x, 3 = 4x)
             */

            uint32_t period_scale;

            /**
             * \var uint16_t duty_cycle
             * \brief PWM Duty cycle
             * The percentage (0-65535)
             */

            uint16_t duty_cycle;

            PWM();
        };

        /**
         * \var std::array<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters> hdr;
         * \brief Array of all PWM Headers on the base RoboRIO board.
         * Array of all PWM headers on the base board of the RoboRIO (not MXP). Numbered 0-10 on the board.
         */

        std::array<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters> hdr;

        /**
         * \var std::array<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumMXPRegisters> mxp;
         * \brief Array of all PWM Headers on the MXP.
         * Array of all PWM headers on the MXP.
         */

        std::array<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumMXPRegisters> mxp;

    public:

        /**
         * \fn tConfig getConfig()const
         * \brief Gets current PWM system configuration.
         * Gets current PWM configuration.
         * \return tConfig representing current PWM system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig getConfig()const;

        /**
         * \fn void setConfig(tConfig config)
         * \brief Sets PWM system configuration.
         * Sets new PWM system configuration.
         * \param tConfig representing new PWM system configuration.
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig);

        /**
         * \fn uint32_t getHdrPeriodScale(uint8_t index)
         * \brief get current pwm scale for a header based PWM.
         * Get current PWM scale for a pwm on the base RoboRIO board.
         * \param index the index of the pwm.
         * \return Unsigned 32-bit integer representing the PWM period scale.
         */

        uint32_t getHdrPeriodScale(uint8_t)const;

        /**
         * \fn void setHdrPeriodScale(uint8_t index)
         * \brief Set PWM scale for a header based pwm.
         * Set PWM scale for a PWM on the base RoboRIO board.
         * \param index the index of the PWM.
         * \param value the period scale you wish to set
         */


        void setHdrPeriodScale(uint8_t, uint32_t);

        /**
         * \fn uint32_t getMXPPeriodScale(uint8_t index)
         * \brief get current pwm scale for a header based pwm.
         * get current pwm scale for a pwm on the base roborio board.
         * \param index the index of the pwm.
         * \return unsigned 32-bit integer representing the pwm period scale.
         */


        uint32_t getMXPPeriodScale(uint8_t)const;

        /**
         * \fn void setMXPPeriodScale(uint8_t index, uint32_t value)
         * \brief Set PWM scale for a MXP PWM.
         * Set PWM scale for a PWM on the base RoboRIO MXP.
         * \param index the index of the PWM.
         * \param value the period scale you wish to set.
         */


        void setMXPPeriodScale(uint8_t, uint32_t);

        /**
         * \fn uint32_t getHdrDutyCycle(uint8_t index)
         * \brief Get current PWM duty cycle.
         * Get current PWM duty cycle for header PWMs.
         * \param index the index of the PWM.
         * \return Unsigned 32-bit integer representing the PWM duty cycle.
         */


        uint32_t getHdrDutyCycle(uint8_t)const;

        /**
         * \fn void setHdrDutyCycle(uint8_t index, uint32_t value)
         * \brief Sets PWM Duty cycle for PWMs on the base board.
         * Sets PWM Duty cycle for PWMs on the base board.
         * \param index the index of the PWM.
         * \param value the new duty cycle to write to the PWM.
         */

        void setHdrDutyCycle(uint8_t, uint32_t);

        /**
         * \fn uint32_t getMXPDutyCycle(uint8_t index)
         * \brief Get current PWM duty cycle.
         * Get current PWM duty cycle for MXP PWMs.
         * \param index the index of the PWM.
         * \return Unsigned 32-bit integer representing the PWM duty cycle.
         */

        uint32_t getMXPDutyCycle(uint8_t)const;

        /**
         * \fn void setMXPDutyCycle(uint8_t index, uint32_t value)
         * \brief Sets PWM Duty cycle for PWMs on the MXP.
         * Sets PWM Duty cycle for PWMs on the MXP.
         * \param index the index of the PWM.
         * \param value the new duty cycle to write to the PWM.
         */
        void setMXPDutyCycle(uint8_t, uint32_t);

        PWMSystem();
    };

}

#endif
