#ifndef _PWM_SYSTEM_HPP_
#define _PWM_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPWM.h"

#include "bounds_checked_array.hpp"

namespace hel{
    /**
     * \brief Data model for PWM system.
     * Data model for all PWMS. Holds all internal data for PWMs.
     */

    struct PWMSystem{
        /**
         * \brief The expected loop timing by HAL
         *
         * When loop timing is requested, this should be used to prevent errors
         */

        static constexpr const int32_t EXPECTED_LOOP_TIMING = 40;

        /**
         * \brief The number of PWM headers
         */

        static constexpr const int32_t NUM_HDRS = nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters;
    private:

        /**
         * \brief Current PWM system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig config;

        /**
         * \brief Data model for individual PWM
         * Data model used for storing data about an individual PWM.
         */

        struct PWM{

            bool zero_latch;

            /**
             * \brief 2 bit PWM signal mask.
             * 2-bit mask for signal masking frequency, effectively scaling the pulse width (0 = 1x 1, = 2x, 3 = 4x)
             */

            uint32_t period_scale;

            /**
             * \brief PWM pulse width in microseconds
             */

            uint16_t pulse_width;

            /**
             * Constructor for PWM
             */

            PWM()noexcept;

            /**
             * Constructor for PWM
             * \param source A PWM object to copy
             */

            PWM(const PWM&)noexcept;
        };

        /**
         * \brief Array of all PWM Headers on the base RoboRIO board.
         * Array of all PWM headers on the base board of the RoboRIO (not MXP). Numbered 0-10 on the board.
         */

        BoundsCheckedArray<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters> hdr;

        /**
         * \brief Array of all PWM Headers on the MXP.
         * Array of all PWM headers on the MXP.
         */

        BoundsCheckedArray<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumMXPRegisters> mxp;

    public:

        /**
         * \fn nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig getConfig()const noexcept
         * \brief Get the current PWM system configuration.
         * \return tConfig representing current PWM system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig getConfig()const noexcept;

        /**
         * \fn void setConfig(nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig value)noexcept
         * \brief Set the PWM system configuration.
         * \param value The new PWM system configuration to use.
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig)noexcept;

        bool getHdrZeroLatch(uint8_t)const;

        void setHdrZeroLatch(uint8_t, bool);

        bool getMXPZeroLatch(uint8_t)const;

        void setMXPZeroLatch(uint8_t, uint32_t);

        /**
         * \brief Get current pwm scale for a header based PWM.
         * \param index The index of the pwm.
         * \return An unsigned 32-bit integer representing the PWM period scale.
         */

        uint32_t getHdrPeriodScale(uint8_t)const ;

        /**
         * \brief Set PWM scale for a header based pwm.
         * \param index The index of the PWM.
         * \param value The period scale you wish to set
         */

        void setHdrPeriodScale(uint8_t, uint32_t);

        /**
         * \brief Get current pwm scale for a header based pwm.
         * \param index The index of the pwm.
         * \return An unsigned 32-bit integer representing the pwm period scale.
         */

        uint32_t getMXPPeriodScale(uint8_t)const ;

        /**
         * \brief Set PWM scale for a MXP PWM.
         * \param index the index of the PWM.
         * \param value the period scale you wish to set.
         */

        void setMXPPeriodScale(uint8_t, uint32_t);

        /**
         * \brief Get current PWM pulse width for RoboRIO headers.
         * \param index the index of the PWM.
         * \return Unsigned 32-bit integer representing the PWM pulse width.
         */

        uint32_t getHdrPulseWidth(uint8_t)const ;

        /**
         * \brief Sets PWM pulse width for PWMs on the base board.
         * \param index the index of the PWM.
         * \param value the new pulse width to write to the PWM.
         */

        void setHdrPulseWidth(uint8_t, uint32_t);

        /**
         * \brief Get current PWM pulse width on RoboRIO MXP.
         * \param index the index of the PWM.
         * \return Unsigned 32-bit integer representing the PWM pulse width.
         */

        uint32_t getMXPPulseWidth(uint8_t)const ;

        /**
         * \brief Sets PWM pulse width for PWMs on the MXP.
         * \param index the index of the PWM.
         * \param value the new pulse width to write to the PWM.
         */

        void setMXPPulseWidth(uint8_t, uint32_t);

        /**
         * \brief Convert the pulse width to a percent output
         * \param pulse_width The pulse width to convert
         * \return The scaled, percent output that the pulse width represents
         */

        static double getPercentOutput(uint32_t)noexcept; //TODO use period scale and config?

        /**
         * Constructor for PWMSystem
         */

        PWMSystem()noexcept;

        /**
         * Constructor for PWMSystem
         * \param source A PWMSystem object to copy
         */

        PWMSystem(const PWMSystem&)noexcept;
    };

    namespace pwm_pulse_width{
        // All of these values were calculated based off of the WPILib defaults and the math used to calculate their respective fields
        constexpr int32_t MAX = 1499;
        constexpr int32_t CENTER = 999;
        constexpr int32_t MIN = 499;

        constexpr int32_t DEADBAND_MAX = CENTER + 1;
        constexpr int32_t DEADBAND_MIN = CENTER - 1;

        constexpr int32_t POSITIVE_SCALE_FACTOR = MAX - DEADBAND_MAX;
        constexpr int32_t NEGATIVE_SCALE_FACTOR = DEADBAND_MIN - MIN;
    }
}
#endif
