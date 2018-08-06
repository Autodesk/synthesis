#ifndef _PWM_SYSTEM_HPP_
#define _PWM_SYSTEM_HPP_

#include "FRC_FPGA_ChipObject/RoboRIO_FRC_ChipObject_Aliases.h"
#include "FRC_FPGA_ChipObject/nRoboRIO_FPGANamespace/tPWM.h"

#include "bounds_checked_array.hpp"

namespace hel{
    /**
     * \struct PWMSystem
     * \brief Data model for PWM system.
     * Data model for all PWMS. Holds all internal data for PWMs.
     */

    struct PWMSystem{
        static constexpr const int32_t EXPECTED_LOOP_TIMING = 40;
        static constexpr const int32_t NUM_HDRS = nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters;
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
             * 2-bit mask for signal masking frequency, effectively scaling the pulse width (0 = 1x 1, = 2x, 3 = 4x)
             */

            uint32_t period_scale;

            /**
             * \var uint16_t pulse_width
             * \brief PWM pulse width in microseconds
             */

            uint16_t pulse_width;

            PWM()noexcept;
            PWM(const PWM&)noexcept;
        };

        /**
         * \var BoundsCheckedArray<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters> hdr;
         * \brief Array of all PWM Headers on the base RoboRIO board.
         * Array of all PWM headers on the base board of the RoboRIO (not MXP). Numbered 0-10 on the board.
         */

        BoundsCheckedArray<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters> hdr;

        /**
         * \var BoundsCheckedArray<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumMXPRegisters> mxp;
         * \brief Array of all PWM Headers on the MXP.
         * Array of all PWM headers on the MXP.
         */

        BoundsCheckedArray<PWM, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumMXPRegisters> mxp;

    public:

        /**
         * \fn tConfig getConfig()const
         * \brief Gets current PWM system configuration.
         * Gets current PWM configuration.
         * \return tConfig representing current PWM system configuration.
         */

        nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig getConfig()const noexcept;

        /**
         * \fn void setConfig(tConfig config)
         * \brief Sets PWM system configuration.
         * Sets new PWM system configuration.
         * \param tConfig representing new PWM system configuration.
         */

        void setConfig(nFPGA::nRoboRIO_FPGANamespace::tPWM::tConfig)noexcept;

        /**
         * \fn uint32_t getHdrPeriodScale(uint8_t index)
         * \brief get current pwm scale for a header based PWM.
         * Get current PWM scale for a pwm on the base RoboRIO board.
         * \param index the index of the pwm.
         * \return Unsigned 32-bit integer representing the PWM period scale.
         */

        uint32_t getHdrPeriodScale(uint8_t)const ;

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


        uint32_t getMXPPeriodScale(uint8_t)const ;

        /**
         * \fn void setMXPPeriodScale(uint8_t index, uint32_t value)
         * \brief Set PWM scale for a MXP PWM.
         * Set PWM scale for a PWM on the base RoboRIO MXP.
         * \param index the index of the PWM.
         * \param value the period scale you wish to set.
         */


        void setMXPPeriodScale(uint8_t, uint32_t);

        /**
         * \fn uint32_t getHdrPulseWidth(uint8_t index)
         * \brief Get current PWM pulse width.
         * Get current PWM pulse width for header PWMs.
         * \param index the index of the PWM.
         * \return Unsigned 32-bit integer representing the PWM pulse width.
         */

        uint32_t getHdrPulseWidth(uint8_t)const ;

        /**
         * \fn void setHdrPulseWidth(uint8_t index, uint32_t value)
         * \brief Sets PWM pulse width for PWMs on the base board.
         * Sets PWM pulse width for PWMs on the base board.
         * \param index the index of the PWM.
         * \param value the new pulse width to write to the PWM.
         */

        void setHdrPulseWidth(uint8_t, uint32_t);

        /**
         * \fn uint32_t getMXPPulseWidth(uint8_t index)
         * \brief Get current PWM pulse width.
         * Get current PWM pulse width for MXP PWMs.
         * \param index the index of the PWM.
         * \return Unsigned 32-bit integer representing the PWM pulse width.
         */

        uint32_t getMXPPulseWidth(uint8_t)const ;

        /**
         * \fn void setMXPPulseWidth(uint8_t index, uint32_t value)
         * \brief Sets PWM pulse width for PWMs on the MXP.
         * Sets PWM pulse width for PWMs on the MXP.
         * \param index the index of the PWM.
         * \param value the new pulse width to write to the PWM.
         */

        void setMXPPulseWidth(uint8_t, uint32_t);

        /**
         *
         */

        static double getPercentOutput(uint32_t)noexcept; //TODO use period scale and config?

        PWMSystem()noexcept;
        PWMSystem(const PWMSystem&)noexcept;
    };

    namespace pwm_pulse_width{
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
