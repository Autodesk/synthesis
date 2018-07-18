#ifndef _JOYSTICK_HPP_
#define _JOYSTICK_HPP_

#include <cstdint>
#include <string>
#include <array>

namespace hel{

    /**
     * \struct Joystick
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
         * \var std::array<int8_t> MAX_AXIS_COUNT> axes
         * \brief Array containing joystick axis states
         * The states of each axis stored as a byte representing percent offset from rest in either diretion
         */

        std::array<int8_t, MAX_AXIS_COUNT> axes;

        /**
         * \var uint8_t axis count
         * \brief The number of axes on the joystick
         */

        uint8_t axis_count;

        /**
         * \var std::array<uint8_t, MAX_AXIS_COUNT> axis_types
         * \brief Array containing joystick axis types
         */

        std::array<uint8_t, MAX_AXIS_COUNT> axis_types; //TODO It is unclear how to interpret the bytes representing axis type

        /**
         * \var std::array<int16_t, MAX_POV_COUNT> povs
         * \brief Array containing joystick POV states
         * The states of each POV stored as 16-bit integers representing the angle in degrees that is pressed, -1 if none are pressed
         */

        std::array<int16_t, MAX_POV_COUNT> povs;

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
        bool getIsXBox()const;

        void setIsXBox(bool);

        uint8_t getType()const;

        void setType(uint8_t);

        std::string getName()const;

        void setName(std::string);

        uint32_t getButtons()const;

        void setButtons(uint32_t);

        uint8_t getButtonCount()const;

        void setButtonCount(uint8_t);

        std::array<int8_t, MAX_AXIS_COUNT> getAxes()const;

        void setAxes(std::array<int8_t, MAX_AXIS_COUNT>);

        uint8_t getAxisCount()const;

        void setAxisCount(uint8_t);

        std::array<uint8_t, MAX_AXIS_COUNT> getAxisTypes()const;

        void setAxisTypes(std::array<uint8_t, MAX_AXIS_COUNT>);

        std::array<int16_t, MAX_POV_COUNT> getPOVs()const;

        void setPOVs(std::array<int16_t, MAX_POV_COUNT>);

        uint8_t getPOVCount()const;

        void setPOVCount(uint8_t);

        uint32_t getOutputs()const;

        void setOutputs(uint32_t);

        uint16_t getLeftRumble()const;

        void setLeftRumble(uint16_t);

        uint16_t getRightRumble()const;

        void setRightRumble(uint16_t);

        std::string serialize()const;

        static Joystick deserialize(std::string);

        std::string toString()const;

        Joystick();
    };
}

#endif
