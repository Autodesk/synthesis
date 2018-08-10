#ifndef _JOYSTICK_HPP_
#define _JOYSTICK_HPP_

#include <cstdint>
#include <string>
#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \brief A data container for joystick data
     * Holds data surrounding joystick inputs and outputs and a description
     */

    struct Joystick{

        /**
         * \var static constexpr uint8_t MAX_JOYSTICK_COUNT
         * \brief The maximum number of joysticks supported by WPILib
         */

        static constexpr uint8_t MAX_JOYSTICK_COUNT = 6; //kJoystickPorts from frc::DriverStation

        /**
         * \var static constexpr uint8_t MAX_AXIS_COUNT
         * \brief The maximum number of joystick axes supported by HAL
         */

        static constexpr uint8_t MAX_AXIS_COUNT = 12; //HAL_kMaxJoystickAxes;

        /**
         * \var static constexpr uint8_t MAX_POV_COUNT
         * \brief The maximum number of joystick POVs supported by HAL
         */

        static constexpr uint8_t MAX_POV_COUNT = 12; //HAL_kMaxJoystickPOVs;

    private:

        /**
         * \var bool is_xbox
         * \brief Whether the joystick is an XBox controller or not
         */

        bool is_xbox;

        /**
         * \var uint8_t type
         * \brief The joystick type
         */

        uint8_t type;

        /**
         * \var std::string name
         * \brief The name of the joystick
         */

        std::string name;

        /**
         * \var uint32_t buttons
         * \brief A bit mask of joystick button states
         */

        uint32_t buttons;

        /**
         * \var uint8_t button_count
         * \brief The number of buttons on the joystick
         */

        uint8_t button_count;

        /**
         * \var BoundsCheckedArray<int8_t> MAX_AXIS_COUNT> axes
         * \brief Array containing joystick axis states
         * The states of each axis stored as a byte representing percent offset from rest in either diretion
         */

        BoundsCheckedArray<int8_t, MAX_AXIS_COUNT> axes;

        /**
         * \var uint8_t axis count
         * \brief The number of axes on the joystick
         */

        uint8_t axis_count;

        /**
         * \var BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT> axis_types
         * \brief Array containing joystick axis types
         */

        BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT> axis_types; //TODO It is unclear how to interpret the bytes representing axis type

        /**
         * \var BoundsCheckedArray<int16_t, MAX_POV_COUNT> povs
         * \brief Array containing joystick POV states
         * The states of each POV stored as 16-bit integers representing the angle in degrees that is pressed, -1 if none are pressed
         */

        BoundsCheckedArray<int16_t, MAX_POV_COUNT> povs;

        /**
         * \var uint8_t pov_count
         * \brief The number of POVs on the joystick
         */

        uint8_t pov_count;

        /**
         * \var uint32_t outputs
         * \brief A 32-bit mask representing HID outputs
         */

        uint32_t outputs;

        /**
         * \var uint16_t left_rumble
         * \brief A 16-bit mapped percent of output to the left rumble
         */

        uint16_t left_rumble;

        /**
         * \var uint16_t right_rumble
         * \brief A 16-bit mapped percent of output to the right rumble
         */

        uint16_t right_rumble;

    public:
        bool getIsXBox()const noexcept;

        void setIsXBox(bool)noexcept;

        uint8_t getType()const noexcept;

        void setType(uint8_t)noexcept;

        std::string getName()const noexcept;

        void setName(std::string)noexcept;

        uint32_t getButtons()const noexcept;

        void setButtons(uint32_t)noexcept;

        uint8_t getButtonCount()const noexcept;

        void setButtonCount(uint8_t)noexcept;

        BoundsCheckedArray<int8_t, MAX_AXIS_COUNT> getAxes()const;

        void setAxes(BoundsCheckedArray<int8_t, MAX_AXIS_COUNT>);

        uint8_t getAxisCount()const noexcept;

        void setAxisCount(uint8_t)noexcept;

        BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT> getAxisTypes()const;

        void setAxisTypes(BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT>);

        BoundsCheckedArray<int16_t, MAX_POV_COUNT> getPOVs()const;

        void setPOVs(BoundsCheckedArray<int16_t, MAX_POV_COUNT>);

        uint8_t getPOVCount()const noexcept;

        void setPOVCount(uint8_t)noexcept;

        uint32_t getOutputs()const noexcept;

        void setOutputs(uint32_t)noexcept;

        uint16_t getLeftRumble()const noexcept;

        void setLeftRumble(uint16_t)noexcept;

        uint16_t getRightRumble()const noexcept;

        void setRightRumble(uint16_t)noexcept;

        std::string serialize()const;

        static Joystick deserialize(std::string);

        std::string toString()const;

        Joystick()noexcept;
        Joystick(const Joystick&)noexcept;
    };
}

#endif
