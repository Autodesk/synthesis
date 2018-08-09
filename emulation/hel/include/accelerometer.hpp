#ifndef _ACCELEROMETER_HPP_
#define _ACCELEROMETER_HPP_

#include <cstdint>
#include <utility>

namespace hel{

    /**
     * \struct Accelerometer
     * \brief Data model for on-board RoboRIO accelerometer
     */

    struct Accelerometer{

        /**
         * \enum Register
         * \brief Copy of HAL emuneration specifying target for Accelerometer function call
         */

        enum Register: uint8_t {
            kReg_Status = 0x00,
            kReg_OutXMSB = 0x01,
            kReg_OutXLSB = 0x02,
            kReg_OutYMSB = 0x03,
            kReg_OutYLSB = 0x04,
            kReg_OutZMSB = 0x05,
            kReg_OutZLSB = 0x06,
            kReg_Sysmod = 0x0B,
            kReg_IntSource = 0x0C,
            kReg_WhoAmI = 0x0D,
            kReg_XYZDataCfg = 0x0E,
            kReg_HPFilterCutoff = 0x0F,
            kReg_PLStatus = 0x10,
            kReg_PLCfg = 0x11,
            kReg_PLCount = 0x12,
            kReg_PLBfZcomp = 0x13,
            kReg_PLThsReg = 0x14,
            kReg_FFMtCfg = 0x15,
            kReg_FFMtSrc = 0x16,
            kReg_FFMtThs = 0x17,
            kReg_FFMtCount = 0x18,
            kReg_TransientCfg = 0x1D,
            kReg_TransientSrc = 0x1E,
            kReg_TransientThs = 0x1F,
            kReg_TransientCount = 0x20,
            kReg_PulseCfg = 0x21,
            kReg_PulseSrc = 0x22,
            kReg_PulseThsx = 0x23,
            kReg_PulseThsy = 0x24,
            kReg_PulseThsz = 0x25,
            kReg_PulseTmlt = 0x26,
            kReg_PulseLtcy = 0x27,
            kReg_PulseWind = 0x28,
            kReg_ASlpCount = 0x29,
            kReg_CtrlReg1 = 0x2A,
            kReg_CtrlReg2 = 0x2B,
            kReg_CtrlReg3 = 0x2C,
            kReg_CtrlReg4 = 0x2D,
            kReg_CtrlReg5 = 0x2E,
            kReg_OffX = 0x2F,
            kReg_OffY = 0x30,
            kReg_OffZ = 0x31
        };

        /**
         * \enum ControlMode
         * \brief Represents which data the accelerometer model will modify
         */

        enum class ControlMode{SET_COMM_TARGET,SET_DATA};
    private:

        /**
         * \var ControlMode control_mode
         * \brief Changes what value Ni FPGA accelerometer writes data to
         */

        ControlMode control_mode;

        /**
         * \var uint8_t comm_target_reg
         * \brief The type of target to open communication with
         */

        uint8_t comm_target_reg;

        /**
         * \var bool active
         * \brief Whether the accelerometer is active or not
         */

        bool active;

        /**
         * \var uin8_t range
         * \brief The range of the accelerometer
         * The ranges map as such: 0 is 2G, 1 is 4G, and 3 is 8G
         */

        uint8_t range;

        /**
         * \var float x_accel
         * \brief The x component of acceleration in g's
         */

        float x_accel;

        /**
         * \var float y_accel
         * \brief The y component of acceleration in g's
         */

        float y_accel;

        /**
         * \var float z_accel
         * \brief The z component of acceleration in g's
         */

        float z_accel;

    public:

        /**
         * \fn ControlMode getControlMode()const noexcept
         * \brief Fetches the active control mode
         * \return The active control mode specifying the data for modification
         */

        ControlMode getControlMode()const noexcept;

        /**
         * \fn void setControlMode(ControlMode control_mode)noexcept
         * \brief Sets the active control mode
         * \param control_mode The data to specify for modification
         */

        void setControlMode(ControlMode)noexcept;

        /**
         * \fn uint8_t getCommTargetReg()const noexcept
         * \brief Get the current data target for communication
         * \return The data target for communication
         */

        uint8_t getCommTargetReg()const noexcept;

        /**
         * \fn void setCommTargetReg(uint8_t comm_target_reg)noexcept
         * \brief Set the current data target for communication
         * \param comm_target_reg The data target for communication
         */

        void setCommTargetReg(uint8_t)noexcept;

        /**
         * \fn bool getActive()const noexcept
         * \brief Get if the accelerometer is active
         * \return True if the accelerometer is set to active
         */

        bool getActive()const noexcept;

        /**
         * \fn void setActive(bool active)noexcept
         * \brief Enable the accelerometer
         * \param active True to enable the accelerometer
         */

        void setActive(bool)noexcept;

        /**
         * \fn uint8_t getRange()const noexcept
         * \brief Get the operating range of the accelerometer
         * \return A byte representing the operating range of the accelerometer
         */

        uint8_t getRange()const noexcept;

        /**
         * \fn void setRange(uint_t range)noexcept
         * \brief Set the operating range for accelerometer
         * \param Range the operating range of the accelerometer
         */

        void setRange(uint8_t)noexcept;

        /**
         * \fn float getXAccel()const noexcept
         * \brief Get the acceleration in the x direction
         * \return A float representing the acceleration in the x direction
         */

        float getXAccel()const noexcept;

        /**
         * \fn void setXAccel(float x_accel)noexcept
         * \brief Set the acceleration in the x direction
         * \param x_accel A float to set the acceleration in the x direction
         */

        void setXAccel(float)noexcept;

        /**
         * \fn float getYAccel()const noexcept
         * \brief Get the acceleration in the y direction
         * \return A float representing the acceleration in the y direction
         */

        float getYAccel()const noexcept;

        /**
         * \fn void setYAccel(float y_accel)noexcept
         * \brief Set the acceleration in the y direction
         * \param y_accel A float to set the acceleration in the y direction
         */

        void setYAccel(float)noexcept;

        /**
         * \fn float getZAccel()const noexcept
         * \brief Get the acceleration in the z direction
         * \return A float representing the acceleration in the z direction
         */

        float getZAccel()const noexcept;

        /**
         * \fn void setZAccel(float z_accel)noexcept
         * \brief Set the acceleration in the z direction
         * \param z_accel A float to set the acceleration in the z direction
         */

        void setZAccel(float)noexcept;

        /**
         * \fn float convertAccel(std::pair<uint8_t,uint8_t> accel)noexcept
         * \brief Converts the format of acceleration the Ni FPGA would generate to acceleration in g's
         * \param accel A pair of bytes that when concatenated represend the value of acceleration without scaling from range configuration
         * \return A float representing acceleration in g's
         */

        float convertAccel(std::pair<uint8_t,uint8_t>)noexcept;

        /**
         * \fn std::pair<uint8_t, uint8_t> convertAccel(float accel)noexcept
         * \brief Converts acceleration in g's to the format of acceleration the Ni FPGA would generate
         * \param accel A float representing acceleration in g's
         * \return A pair of bytes that when concatenated represend the value of acceleration without scaling from range configuration
         */

        std::pair<uint8_t, uint8_t> convertAccel(float)noexcept;

        /**
         * Constructor for an Accelerometer
         */

        Accelerometer()noexcept;

        /**
         * Constructor for an Accelerometer
         *
         * \param source An accelerometer object to copy
         */

        Accelerometer(const Accelerometer&)noexcept;
    };
}

#endif
