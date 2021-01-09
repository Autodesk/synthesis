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
         * \brief The maximum number of joysticks supported by WPILib
         */

        static constexpr uint8_t MAX_JOYSTICK_COUNT = 6; //kJoystickPorts from frc::DriverStation

        /**
         * \brief The maximum number of joystick axes supported by HAL
         */

        static constexpr uint8_t MAX_AXIS_COUNT = 12; //HAL_kMaxJoystickAxes;

        /**
         * \brief The maximum number of joystick POVs supported by HAL
         */

        static constexpr uint8_t MAX_POV_COUNT = 12; //HAL_kMaxJoystickPOVs;

        /**
         * \brief The Maximum size of the joystick name string in characters
         */

        static constexpr unsigned MAX_JOYSTICK_NAME_SIZE = 256;
    private:

        /**
         * \brief Whether the joystick is an XBox controller or not
         */

        bool is_xbox;

        /**
         * \brief The joystick type
         */

        uint8_t type;

        /**
         * \brief The name of the joystick
         */

        std::string name;

        /**
         * \brief A bit mask of joystick button states
         */

        uint32_t buttons;

        /**
         * \brief The number of buttons on the joystick
         */

        uint8_t button_count;

        /**
         * \brief Array containing joystick axis states
         * The states of each axis stored as a byte representing percent offset from rest in either direction
         */

        BoundsCheckedArray<int8_t, MAX_AXIS_COUNT> axes;

        /**
         * \brief The number of axes on the joystick
         */

        uint8_t axis_count;

        /**
         * \brief Array containing joystick axis types
         */

        BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT> axis_types; //TODO It is unclear how to interpret the bytes representing axis type

        /**
         * \brief Array containing joystick POV (aka D-pad) states
         * The states of each POV stored as 16-bit integers representing the angle in degrees that is pressed, -1 if none are pressed
         */

        BoundsCheckedArray<int16_t, MAX_POV_COUNT> povs;

        /**
         * \brief The number of POVs on the joystick
         */

        uint8_t pov_count;

        /**
         * \brief A 32-bit mask representing HID outputs
         */

        uint32_t outputs;

        /**
         * \brief A 16-bit mapped percent of output to the left rumble
         */

        uint16_t left_rumble;

        /**
         * \brief A 16-bit mapped percent of output to the right rumble
         */

        uint16_t right_rumble;

    public:
        /**
         * \brief Get if the joystick is an XBox controller
         * \return True if the joystick is an XBox controller
         */

        bool getIsXBox()const noexcept;

        /**
         * \brief Set if the joystick is an XBox controller
         * \param xbox True to set the joystick as an Xbox controller
         */

        void setIsXBox(bool)noexcept;

        /**
         * \brief Get the type of joystick
         * \return An integer representing the type of joystick
         */

        uint8_t getType()const noexcept;

        /**
         * \brief Set the type of joystick
         * \param t The type to set the joystick type to
         */

        void setType(uint8_t)noexcept;

        /**
         * \brief Get the name of the joystick
         * \return The name of the joystick
         */

        std::string getName()const noexcept;

        /**
         * \brief Set the name of the joystick
         * \param n The name to set for the joystick
         */

        void setName(std::string)noexcept;

        /**
         * \brief Get the button states of the joystick
         * \return An integer bitmask representing the states of the joystick buttons
         */

        uint32_t getButtons()const noexcept;

        /**
         * \brief Set the button states of the joystick
         * \param b The integer bitmask of button states to set for the joystick
         */

        void setButtons(uint32_t)noexcept;

        /**
         * \brief Get the number of buttons on the joystick
         * \return The number of buttons on the joystick
         */

        uint8_t getButtonCount()const noexcept;

        /**
         * \brief Set the number of buttons on the joystick
         * \param b_count The number of buttons to set for the joystick
         */

        void setButtonCount(uint8_t)noexcept;

        /**
         * \brief Get the states of the joystick axes
         * \return A BoundsCheckedArray of joystick axes states
         */

        BoundsCheckedArray<int8_t, MAX_AXIS_COUNT> getAxes()const;

        /**
         * \brief Set the states of the joystick axes
         * \param a The states of axes to set for the joystick
         */

        void setAxes(BoundsCheckedArray<int8_t, MAX_AXIS_COUNT>);

        /**
         * \brief Get the number of axes on the joystick
         * \return The number of axes on the joystick
         */

        uint8_t getAxisCount()const noexcept;

        /**
         * \brief Set the number of axes on the joystick
         * \param a_count The number of axes to set for the joystick
         */

        void setAxisCount(uint8_t)noexcept;

        /**
         * \brief Get the axis types of the axes on the joystick
         * \return A BoundsCheckedArray of integers representing the axis types on the joystick
         */

        BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT> getAxisTypes()const;

        /**
         * \brief Set the axis types of the axes on the joystick
         * \param a_types The axis types to set for the joystick
         */

        void setAxisTypes(BoundsCheckedArray<uint8_t, MAX_AXIS_COUNT>);

        /**
         * \brief Get the states of the POVs on the joystick
         * \return The states of the POVs on the joystick
         */

        BoundsCheckedArray<int16_t, MAX_POV_COUNT> getPOVs()const;

        /**
         * \brief Set the states of the POVs on the joystick
         * \param p The states of the POVs to set for the joystick
         */

        void setPOVs(BoundsCheckedArray<int16_t, MAX_POV_COUNT>);

        /**
         * \brief Get the number of POVs on the joystick
         * \return The number of POVs on the joystick
         */

        uint8_t getPOVCount()const noexcept;

        /**
         * \brief Set the number of POVs on the joystick
         * \param p_count The number of POVs to set for the joystick
         */

        void setPOVCount(uint8_t)noexcept;

        /**
         * \brief Get the states of the joystick outputs
         * \return An integer bitmask of the joystick outputs
         */

        uint32_t getOutputs()const noexcept;

        /**
         * \brief Set the states of the joystick outputs
         * \param out An integer bitmask of outputs to set for the joystick
         */

        void setOutputs(uint32_t)noexcept;

        /**
         * \brief Get the joystick's left rumble state
         * \return The joystick's left rumble state
         */

        uint16_t getLeftRumble()const noexcept;

        /**
         * \brief Set the joystick's left rumble state
         * \param rumble The state of left rumble to set for the joystick
         */

        void setLeftRumble(uint16_t)noexcept;

        /**
         * \brief Get the joystick's right rumble state
         * \return The joystick's right rumble state
         */

        uint16_t getRightRumble()const noexcept;

        /**
         * \brief Set the joystick's right rumble state
         * \param rumble The state of right rumble to set for the joystick
         */

        void setRightRumble(uint16_t)noexcept;

        /**
         * \brief Format the Joystick data as a string
         * \return The Joystick data in string format
         */

        std::string toString()const;

        /**
         * Constructor for a Joystick
         */

        Joystick()noexcept;

        /**
         * Constructor for Joystick
         * \param source A Joystick object to copy
         */

        Joystick(const Joystick&)noexcept;
    };
}

#endif
