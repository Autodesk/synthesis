#ifndef _ROBORIO_H_
#define _ROBORIO_H_

#define ASIO_STANDALONE
#define ASIO_HAS_STD_ADDRESSOF
#define ASIO_HAS_STD_ARRAY
#define ASIO_HAS_CSTDINT
#define ASIO_HAS_STD_SHARED_PTR
#define ASIO_HAS_STD_TYPE_TRAITS


/**
 * \file roborio.h
 * \brief Defines internal structure of mock RoboRIO
 * This file defines the RoboRIOs structure
 */

#include <array>
#include <memory>
#include <mutex>
#include <queue>
#include <vector>
#include <thread>
#include <atomic>

#include <asio.hpp>

#include "FRC_NetworkCommunication/FRCComm.h"

#include "HAL/ChipObject.h"
#include "athena/PortsInternal.h"
#include "athena/DigitalInternal.h"

#include "DriverStation.h"
#include "GenericHID.h"

#include "error.h"

#include "send_data.h"
#include "sync_server.h"

#include <iostream>

namespace hel{
    using namespace nFPGA;
    using namespace nRoboRIO_FPGANamespace;

    /**
     * \struct RoboRIO roborio.h
     * \brief Mock RoboRIO implementation
     *
     * This class represents the internals of the RoboRIO hardware, broken up into several sub-systems:
     * Analog Input, Analog Output, PWM, DIO, SPI, MXP, RS232, and I2C.
     */
    struct RoboRIO{
        /**
         * \struct AnalogOutputs roborio.h
         * \brief Data model for analog outputs.
         * Holds all internal data needed to model analog outputs on the RoboRIO.
         */
        struct AnalogOutputs{
    	private:
            /**
             * \var std::array<uint16_t, tAO::kNumMXPRegisters> mxp_outputs
             * \brief Analog output data
             * 
             */
            std::array<uint16_t, tAO::kNumMXPRegisters> mxp_outputs;

        public:
            /**
             * \fn uint16_t getMXPOutput(uint8_t index)const
             * \brief Get MXP output.
             * Returns the MXP output given
             * \param index an byte representing the index of the analog output.
             * \return an unsigned 16-bit integer representing the current analog output.
             */
    		uint16_t getMXPOutput(uint8_t)const;
            /**
             * \n void setMXPOutput(uint8_t index, uint16_t value)
             * \brief Set MXP output.
             * Set the MXP value of analog output with a given index to a given value
             * \param index an byte representing the index of the analog output.
             * \param value an unsigned 16-bit integer representing the value of the analog output.
             */
    		void setMXPOutput(uint8_t,uint16_t);
        };
        /**
         * \struct AnalogInputs roborio.h
         * \brief Data model for analog inputs.
         * Holds all internal data needed to model analog inputs on the RoboRIO.
         */
    	struct AnalogInputs {
            /**
             * \struct AnalogInput roborio.h
             * \brief Data model for individual analog input
             * Holds all internal data for a single analog input.
             */
            struct AnalogInput{
                /**
                 * \var uint8_t oversample_bits.
                 * \brief When storing analog value history, keep 2**(oversample_bits + average_bits) samples.
                 */
                uint8_t oversample_bits;
                /**
                 * \var uint8_t average_bits.
                 * \brief When averaging, average 2**average_bits samples.
                 */
                uint8_t average_bits;
                /**
                 * \var uint8_t scan_list.
                 * \brief Currently unknown functionality.
                 */
                uint8_t scan_list;

                /**
                 * \var int32_t value
                 * \brief The history of analog input values
                 * The most recent value is the last element.
                 */

                std::vector<int32_t> values;
            };

            /**
             * \fn void setConfig(tConfig value)
             * \brief Sets analog input configuration.
             * Sets current analog input system to \b value.
             * \param value a tConfig object containing new configuration data.
             */

            void setConfig(tAI::tConfig value);

            /**
             * \fn tConfig getConfig()
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \return tConfig representing current analog system configuration.
             */

            tAI::tConfig getConfig();

            /**
             * \fn void setReadSelect(tReadSelect value)
             * \brief Sets analog input read select.
             * Sets current analog input system to \b value. This specifies which analog input to read.
             * \param value a tReadSelect object containing addressing information for the desired analog input.
             */

            void setReadSelect(tAI::tReadSelect);

            /**
             * \fn tConfig getReadSelect()
             * \brief Get current analog input read select.
             * Gets current analog system read select. This specifies which analog input to read.
             * \return tReadSelect representing current analog system read selection.
             */

            tAI::tReadSelect getReadSelect();

            /**
             * \fn void setOversampleBits(uint8_t channel, uint8_t value)
             * \brief Sets number of samples to keep beyond those needed for averaging.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a byte representing the number of samples to collect after that need for the average.
             */

            void setOversampleBits(uint8_t, uint8_t);

            /**
             * \fn void setOversampleBits(uint8_t channel, uint8_t value)
             * \brief Sets number of sample to average to 2**value.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a byte representing the number of samples to use in averaging.
             */

            void setAverageBits(uint8_t, uint8_t);

            /**
             * \fn void setOversampleBits(uint8_t channel, uint8_t value)
             * \brief Sets analog input scan lsit.
             * Sets a given analog inputs scan list to \b value.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a byte representing the scan list.
             */
            void setScanList(uint8_t, uint8_t);

            /**
             * \fn void setValue(uint8_t channel, std::vector<int32_t> values)
             * \brief Sets the history of analog input values.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \param value a vector history of 32-bit integers representing the value of the input.
             */

            void setValues(uint8_t, std::vector<int32_t>);

            /**
             * \fn uint8_t getOversampleBits(uint8_t channel)
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a byte representing the current bits to oversample.
             */

            uint8_t getOversampleBits(uint8_t);

            /**
             * \fn uint8_t getAverageBits(uint8_t channel)
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a byte representing the number of bits per sample for analog input \b channel.
             */

            uint8_t getAverageBits(uint8_t);

            /**
             * \fn uint8_t getScanList(uint8_t channel)
             * \brief Get current analog input configuration.
             * Gets current analog system configuration settings.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a byte representing the current scan list for analog input \b channel.
             */

            uint8_t getScanList(uint8_t);

            /**
             * \fn uint8_t getValue(uint8_t channel)
             * \brief Get the recent history of analog input values.
             * \param channel a byte representing the hardware channel of the desired analog input.
             * \return a vector of 32-bit integer representing the recent history of the analog input value for analog input \b channel.
             */

            std::vector<int32_t> getValues(uint8_t);

        private:

            /**
             * \var std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs
             * \brief Array of all analog inputs.
             * A holder array for all analog input objects.
             */

            std::array<AnalogInput, hal::kNumAnalogInputs> analog_inputs;

            /**
             * \var tConfig config;
             * \brief current analog input configuration.
             */

            tAI::tConfig config;

            /**
             * \var tReadSelect read_select;
             * \brief current analog input read select configuration.
             */

            tAI::tReadSelect read_select;
        };

        /**
         * \struct PWMSystem roborio.h
         * \brief Data model for PWM system.
         * Data model for all PWMS. Holds all internal data for PWMs.
         */

    	struct PWMSystem{
    	private:

            /**
             * \var tConfig config
             * \brief Current PWM system configuration.
             */

    		tPWM::tConfig config;

            /**
             * \struct PWM roborio.h
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
    		};

            /**
             * \var std::array<PWM, tPWM::kNumHdrRegisters> hdr;
             * \brief Array of all PWM Headers on the base RoboRIO board.
             * Array of all PWM headers on the base board of the RoboRIO (not MXP). Numbered 0-10 on the board.
             */

    		std::array<PWM, tPWM::kNumHdrRegisters> hdr;

            /**
             * \var std::array<PWM, tPWM::kNumMXPRegisters> mxp;
             * \brief Array of all PWM Headers on the MXP.
             * Array of all PWM headers on the MXP.
             */

    		std::array<PWM, tPWM::kNumMXPRegisters> mxp;

    	public:

            /**
             * \fn tConfig getConfig()const
             * \brief Gets current PWM system configuration.
             * Gets current PWM configuration.
             * \return tConfig representing current PWM system configuration.
             */

    		tPWM::tConfig getConfig()const;

            /**
             * \fn void setConfig(tConfig config)
             * \brief Sets PWM system configuration.
             * Sets new PWM system configuration.
             * \param tConfig representing new PWM system configuration.
             */

            void setConfig(tPWM::tConfig);

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
    	};

        struct DIOSystem{
    	private:

            /**
             * \var
             */

            tDIO::tDO outputs;

            /**
             * \var
             */

            tDIO::tOutputEnable enabled_outputs;

            /**
             * \var
             */

            tDIO::tPulse pulses;

            /**
             * \var
             */

            tDIO::tDI inputs;

            /**
             * \var
             */

    		uint16_t mxp_special_functions_enabled;//TODO this may be enabled low, double check that

            /**
             * \var
             */

    		uint8_t pulse_length;

    		std::array<uint8_t, hal::kNumDigitalPWMOutputs> pwm; //TODO unclear whether these are mxp pins or elsewhere (there are only six here whereas there are ten on the mxp)

    	public:

            /**
             * \fn
             */

    		tDIO::tDO getOutputs()const;

            /**
             * \fn
             */

    		void setOutputs(tDIO::tDO);

            /**
             * \fn
             */

    		tDIO::tOutputEnable getEnabledOutputs()const;

            /**
             * \fn
             */

    		void setEnabledOutputs(tDIO::tOutputEnable);

            /**
             * \fn
             */

    		uint16_t getMXPSpecialFunctionsEnabled()const;

            /**
             * \fn
             */

    		void setMXPSpecialFunctionsEnabled(uint16_t);

            /**
             * \fn
             */

    		tDIO::tPulse getPulses()const;

            /**
             * \fn
             */

    		void setPulses(tDIO::tPulse);

            /**
             * \fn
             */

    		tDIO::tDI getInputs()const;

            /**
             * \fn
             */

    		void setInputs(tDIO::tDI);

            /**
             * \fn
             */

    		uint8_t getPulseLength()const;

            /**
             * \fn
             */

    		void setPulseLength(uint8_t);

            /**
             * \fn
             */

    		uint8_t getPWMDutyCycle(uint8_t)const;

            /**
             * \fn
             */

    		void setPWMDutyCycle(uint8_t, uint8_t);
    	};

    	/**
    	 * \struct CANBus roborio.h
    	 * \brief Models CAN bus input and output.
    	 * Holds internal queues of CAN messages for input and output.
    	 */

    	struct CANBus{
    		
    		/**
    		 * \struct Message roborio.h
    		 * \brief Holds internally all parts of a CAN bus message
    		 */
    		struct Message{
    			
    			/**
    			 * \var uint32_t id
    			 * \brief the message identifier which also communicates priority 
    			 * The message ID can be configured to the 11-bit base or 29-bit extended format.
    			 */

    			uint32_t id;

    			/**
    			 * \var std::array<uint8_t, 8> data
    			 * \brief the data transmitted with the message in byte array form
    			 * The data can array can vary from 0-8 bytes in size.
    			 */

    			std::array<uint8_t, 8> data;

    			/**
    			 * \var uint8_t data_size
    			 * \brief four bits representing the number of bytes of data in the message 
    			 * There can be between 0-8 bytes of data.
    			 */

    			uint8_t data_size: 4;

    			/**
    			 * \var uint32_t time_stamp
    			 * \brief time stamp of message send/receive in milliseconds
    			 */

    			uint32_t time_stamp;

    			/**
    			 * \var static constexpr int32_t CAN_SEND_PERIOD_NO_REPEAT
    			 * \brief a send period communicating the message should not be repeated
    			 */

    			static constexpr int32_t CAN_SEND_PERIOD_NO_REPEAT = 0;

    			/**
    			 * \var static constexpr int32_t CAN_SEND_PERIOD_STOP_REPEATING
    			 * \brief a send period communicating the message with the associated ID should stop repeating
    			 */

    			static constexpr int32_t CAN_SEND_PERIOD_STOP_REPEATING = -1;

    			/**
    			 * \var static constexpr uint32_t CAN_IS_FRAME_REMOTE
    			 * \brief used to identify a message ID as that of a remote frame
    			 * Remote CAN frames are requests for data from a different source.
    			 */

    			static constexpr uint32_t CAN_IS_FRAME_REMOTE = 0x80000000;

    			/**
    			 * \var static constexpr uint32_t CAN_IS_FRAME_11BIT
    			 * \brief used to identify a message ID as using 11-bit, base formatting
    			 */

    			static constexpr uint32_t CAN_IS_FRAME_11BIT = 0x40000000;

    			/**
    			 * \var static constexpr uint32_t CAN_29BIT_MESSAGE_ID_MASK
    			 * \brief used as a message ID mask to communicate the message ID is in 29-bit, extended formatting
    			 */

    			static constexpr uint32_t CAN_29BIT_MESSAGE_ID_MASK = 0x1FFFFFFF;

    			/**
    			 * \var static constexpr uint32_t CAN_11BIT_MESSAGE_ID_MASK
    			 * \brief used as a message ID mask to communicate the message ID is in 11-bit, base formatting
    			 */

    			static constexpr uint32_t CAN_11BIT_MESSAGE_ID_MASK = 0x000007FF;
    		};
    	private:

    		/**
    		 * \var std::queue<Message> in_message_queue
    		 * \brief a queue of CAN messages to send
    		 */

    		std::queue<Message> in_message_queue;

    		/**
    		 * \var std::queue<Message> out_message_queue
    		 * \brief a queue of CAN messages that have been receieved
    		 */

    		std::queue<Message> out_message_queue;
    
    	public:

    		/**
    		 * \fn void enqueueMessage(Message m)
    		 * \brief Add a CAN message to the output queue.
    		 * \param value a Message object to add to the message output queue.
    		 */
    
    		void enqueueMessage(Message);
    		
    		/**
    		 * \fn Message getNextMessage()const
    		 * \brief Get the oldest received message (i.e. the next in queue)
    		 * \return the next received CAN message in queue.
    		 */

    		Message getNextMessage()const;

    		/**
    		 * \fn void popNextMessage()
    		 * \brief removes the oldest received message from the input queue
    		 */

    		void popNextMessage();
    	};

    	/**
    	 * \struct RelaySystem roborio.h
    	 * \brief Data model for Relay system.
    	 * Holds all internal data to model relay outputs.
    	 */

    	struct RelaySystem{
    	private:
    		
    		/**
    		 * \var tRelay::tValue value
    		 * \brief Relay output data
    		 */
            
    		tRelay::tValue value;

    	public:

    		/**
    		 * \fn tRelay::tValue getValue()const
    		 * \brief Get relay output.
    		 * Returns the relay output
    		 * \return a tRelay::tValue object representing the reverse and forward channel outputs.
    		 */
    		
    		tRelay::tValue getValue()const;

    		/**
    		 * \fn void setValue(tRelay::tValue value)
    		 * \brief Set relay output.
    		 * Sets the relay output to \b value
    		 * \param value a tRelay::tValue object representing the reverse and forward channel outputs.
    		 */
    		
    		void setValue(tRelay::tValue);
    	};

        /**
         * \struct RobotState roborio.h
         * \brief Represents match phase and robot enabled state
         */

    	struct RobotState{

            /**
             * \enum State
             * \brief Represents robot running state
             * Represents whether robot is in autonomous, teleoperated, or test/practice mode.
             */

    		enum class State{AUTONOMOUS,TELEOPERATED,TEST};

    	private:

            /**
             * \var State state
             * \brief The robot running state
             */

            State state;

            /**
             * \var bool enabled
             * \brief Robot enabled state
             */

    		bool enabled;

            /**
             * \var bool emergency_stopped
             * \brief Robot emergency stopped state
             */

            bool emergency_stopped;

            /**
             * \var bool fms_attached
             * \brief Whether the FMS is attached or not
             */

            bool fms_attached;

            /**
             * \var bool ds_attached
             * \brief Whether the driver station is attached or not
             */

            bool ds_attached;

    	public:

            /**
             * \fn State getState()const
             * \brief Get robot running state
             * \return a State object representing the robot running state
             */

    		State getState()const;

            /**
             * \fn void setState(State state)
             * \brief Set robot running state
             * \param state a State object representing the robot running state
             */

    		void setState(State);

            /**
             * \fn bool getEnabled()const
             * \brief Get robot enabled state
             * \return true if the robot is enabled
             */

    		bool getEnabled()const;

            /**
             * \fn void setEnabled(bool enabled)
             * \brief Set the robot enabled state
             * \param enabled a bool representing the robot enabled state
             */

        	void setEnabled(bool);

            /**
             * \fn bool getEmergencyStopped()const
             * \brief Get robot emergency stopped state
             * \return true if the robot is emergency stopped
             */

            bool getEmergencyStopped()const;
            
            /**
             * \fn void setEmergencyStopped(bool emergency_stopped)
             * \brief Set the robot emergency stopped state
             * \param emergency_stopped a bool representing the robot emergency stopped state
             */

            void setEmergencyStopped(bool);
            
            /**
             * \fn bool getFMSAttached()const
             * \brief Get robot FMS connection state
             * \return true if the robot is connected to the FMS
             */

            bool getFMSAttached()const;
            
            /**
             * \fn void setFMSAttached(bool fms_attached)
             * \brief Set robot FMS connection state
             * \param fms_attached a bool representing the robot FMS connection state
             */

            void setFMSAttached(bool);

            /**
             * \fn bool getDSAttached()const
             * \brief Get robot driver station connection state
             * \return true if the robot is connected to the driver station
             */

            bool getDSAttached()const;
            
            /**
             * \fn void setDSAttached(bool ds_attached)
             * \brief Set robot driver station connection state
             * \param ds_attached a bool representing the robot driver station connection state
             */

            void setDSAttached(bool);

            /**
             * \fn ControlWord_t toControlWord()const
             * \brief Populate a new ControlWord_t object from this RobotState object
             * \return a new ControlWord_t object populated from this RobotState object
             */

            ControlWord_t toControlWord()const;
    	};
    	
        /**
         * \struct DriverStationInfo roborio.h
         * \brief A data container for match/driver station information
         * Holds all of the match data communicated to the robot via the driver station
    	 */

        struct DriverStationInfo{
    	private:

            /**
             * \var std::string event_name
             * \brief A string representing the name of the event
             */

    		std::string event_name;

            /**
             * \var std::string game_specific_message
             * \brief Represents any game-specific information
             * The FMS will generate any game-specific information and communicate it to the robots.
             */

    	    std::string game_specific_message; 

            /**
             * \var MatchType_t match_type
             * \brief Represents which type of match is running
             */

            MatchType_t match_type;

    		/**
             * \var uint16_t match_number
             * \brief Represents the match number at the event
             */

            uint16_t match_number;

            /**
             * \var uint8_t replay_number
             * \brief An byte representing if the match is a replay and which it is
             */

        	uint8_t replay_number;

            /**
             * \var AllianceStationID_t alliance_station_id
             * \brief Represents which driver station position the robot is running from
             */

        	AllianceStationID_t alliance_station_id;
           
            /**
             * \var double match_time
             * \brief Represents match time in seconds
             */

        	double match_time; 

    	public:

            /**
             * \fn std::string getEventName()const
             * \brief Fetch a string representing the event name 
             * return a standard string representing the event name
             */

    		std::string getEventName()const;

            /**
             * \fn void setEventName(std::string event_name)
             * \brief Set the name of the event
             * \param event_name a standard string representing the name of the event
             */

    		void setEventName(std::string);

            /**
             * \fn std::string getGameSpecificMessage()const
             * \brief Fetch any game specific message for the match
             * \return a standard string representing any game specific message
             */

    		std::string getGameSpecificMessage()const;

            /**
             * \fn void setGameSpecificMessage(std::string game_specific_message)
             * \brief Set the game specific message for the match
             * \param game_specific_message the game specific message for the match
             */

    		void setGameSpecificMessage(std::string);

            /**
             * \fn MatchType_t getMatchType()const
             * \brief Fetch the type of match
             * \return a MatchType_t object representing the type of match
             */

    		MatchType_t getMatchType()const;

            /**
             * \fn void setMatchType(MatchType_t match_type)
             * \brief Set the tye of match
             * \param match_type the type of match running
             */

    		void setMatchType(MatchType_t);

            /**
             * \fn uint16_t getMatchNumber()const
             * \brief Fetch the match number at the event
             * \return a 16-bit integer representing the match number
             */

    		uint16_t getMatchNumber()const;

            /**
             * \fn void setMatchNumber(uint16_t match_number)
             * \brief Set the match number at the event
             * \param match_number the running match number
             */

    		void setMatchNumber(uint16_t);

            /**
             * \fn uint8_t getReplayNumber()const
             * \brief Get the replay number for the running match
             * \return a byte representing the replay number of the running match (0 if not a replay)
             */

    		uint8_t getReplayNumber()const;

            /**
             * \fn void setReplayNumber(uint8_t replay_number)
             * \brief Set the replay number for the running match
             * \param replay_number a byte representing the replay number for the running match (0 if not a replay)
             */

    		void setReplayNumber(uint8_t);

            /**
             * \fn AllianceStationID_t getAllianceStationID()const
             * \brief Fetch the driver station position controlling the robot
             * \return an AllianceStationID_t object representing the robot's driver station ID
             */

    		AllianceStationID_t getAllianceStationID()const;
    		
            /**
             * \fn void setAllianceStationID(AllianceStationID_t alliance_station_id)
             * \brief Set the driver station position for the robot's drivers
             * \param alliance_station_id the driver station position for the robot
             */

            void setAllianceStationID(AllianceStationID_t);

            /**
             * \fn double getMatchTime()const
             * \brief Get the match time in seconds
             * \return a double representing the match time in seconds
             */

    		double getMatchTime()const;
    		
            /**
             * \fn void SetMatchTime(double match_time)
             * \brief Set the match time
             * \param match_time a double representing the match time in seconds
             */

            void setMatchTime(double);
    	};

        /**
         * \struct Joystick roborio.h
         * \brief A data container for joystick data
         * Holds data surrounding joystick inputs and outputs and a description 
         */

        struct Joystick{

            /**
             * \var static constexpr uint8_t MAX_JOYSTICK_COUNT
             * \brief The maximum number of joysticks supported by WPILib
             */

            static constexpr uint8_t MAX_JOYSTICK_COUNT = frc::DriverStation::kJoystickPorts;
            
            /**
             * \var static constexpr uint8_t MAX_AXIS_COUNT
             * \brief The maximum number of joystick axes supported by HAL
             */

            static constexpr uint8_t MAX_AXIS_COUNT = HAL_kMaxJoystickAxes;
            
            /**
             * \var static constexpr uint8_t MAX_POV_COUNT
             * \brief The maximum number of joystick POVs supported by HAL
             */

            static constexpr uint8_t MAX_POV_COUNT = HAL_kMaxJoystickPOVs;

        private:
            
            /**
             * \var bool is_xbox
             * \brief Whether the joystick is an XBox controller or not
             */

            bool is_xbox;
             
            /**
             * \var frc::GenericHID::HIDType type
             * \brief The joystick type
             */

            frc::GenericHID::HIDType type;

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

            frc::GenericHID::HIDType getType()const;

            void setType(frc::GenericHID::HIDType);

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
        };

        struct Counter{
            static constexpr uint8_t MAX_COUNTER_COUNT = tCounter::kNumSystems;
        private:
            
            /** 
             * \var tCounter::tOutput output
             * \brief The counter's count
             */

            tCounter::tOutput output;
            
            /**
             * \var tCounter::tConfig config
             * \brief Configuration for the counter
             */

            tCounter::tConfig config;

            /**
             * \var tCounter::tTimerOutput timer_output
             * \brief The time count (period)
             */

           tCounter::tTimerOutput timer_output;

            /**
             * \var tCounter::tTimerConfig timer_config
             * \brief Configuration for the time counter
             */

            tCounter::tTimerConfig timer_config;

        public:
            tCounter::tOutput getOutput()const;
            void setOutput(tCounter::tOutput);

            tCounter::tConfig getConfig()const;
            void setConfig(tCounter::tConfig);
 
            tCounter::tTimerOutput getTimerOutput()const;
            void setTimerOutput(tCounter::tTimerOutput);
 
            tCounter::tTimerConfig getTimerConfig()const;
            void setTimerConfig(tCounter::tTimerConfig);
        };

        struct Accelerometer{
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

            enum class ControlMode{SET_COMM_TARGET,SET_DATA};
        private:

             /**
             * \var ControlMode control_mode
             * \brief Changes what value NI FPGA accelerometer writes data to
             */

            ControlMode control_mode;
            
            /**
             * \var uint8_t comm_target_reg
             * \brief The type of target to open communication with 
             */

            uint8_t comm_target_reg;

             /**
             * \var bool active
             * \brief Whether the accelerometer is active
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
            ControlMode getControlMode()const;
            void setControlMode(ControlMode);
            uint8_t getCommTargetReg()const;
            void setCommTargetReg(uint8_t);
            bool getActive()const;
            void setActive(bool);
            uint8_t getRange()const;
            void setRange(uint8_t);
            float getXAccel()const;
            void setXAccel(bool);
            float getYAccel()const;
            void setYAccel(bool);
            float getZAccel()const;
            void setZAccel(bool);
            float convertAccel(std::pair<uint8_t,uint8_t>);
            std::pair<uint8_t, uint8_t> convertAccel(float);
        };

        /**
         * \struct Accumulator roborio.h
         * \brief Data model for an analog accumulator
         * Accumulates analog values in a total over time while tracking count
         */

        struct Accumulator{
        private:

            /**
             * \var tAccumulator::tOutput output
             * \brief Stores the accumulated value of the accumulator
             */

            tAccumulator::tOutput output;
            
            /**
             * \var int32_t center
             * \brief The center value for the accumulator
             * This is used to handle device offsets
             */

            int32_t center;
            
            /**
             * \var int32_t deadband
             * \brief The deadband of the accumulator
             */

            int32_t deadband;

        public:
            tAccumulator::tOutput getOutput()const;
            void setOutput(tAccumulator::tOutput);
            int32_t getCenter()const;
            void setCenter(int32_t);
            int32_t getDeadband()const;
            void setDeadband(int32_t);
        };

        /**
         * \struct Encoder roborio.h
         * \brief Data model for encoder input data
         * Holds all the data for encoder inputs
         */

        struct Encoder{
        private:

            /**
             * \var tEnoder::tOutput output
             * \brief
             */

            tEncoder::tOutput output;
 
            /**
             * \var tEnoder::tConfig config
             * \brief Configuration for count
             */

            tEncoder::tConfig config;
 
            /**
             * \var tEnoder::tTimerOutput timer_output
             * \brief Time-based count
             */

            tEncoder::tTimerOutput timer_output;
 
            /**
             * \var tEnoder::tTimerConfig timer_config
             * \brief Configuration for time-based count
             */

            tEncoder::tTimerConfig timer_config;

        public:
            tEncoder::tOutput getOutput()const;
            void setOutput(tEncoder::tOutput);
            tEncoder::tConfig getConfig()const;
            void setConfig(tEncoder::tConfig);
            tEncoder::tTimerOutput getTimerOutput()const;
            void setTimerOutput(tEncoder::tTimerOutput);
            tEncoder::tTimerConfig getTimerConfig()const;
            void setTimerConfig(tEncoder::tTimerConfig);
        };

        /**
         * \struct Power roborio.h
         * \brief Data model for RoboRIO voltmeter and power manager
         */

        struct Power{
        private:

            /**
             * \var tPower::tStatus status
             * \brief The active state of the power supply rails
             */

            tPower::tStatus status;

            /**
             * \var tPower::tFaultCounts fault_counts
             * \brief A running count of faults for each rail
             */

            tPower::tFaultCounts fault_counts;
            
            /**
             * \var tPower::tDisable disabled
             * \brief Which power rails have been disabled
             */

            tPower::tDisable disabled;

        public:
            tPower::tStatus getStatus()const;
            void setStatus(tPower::tStatus);
            tPower::tFaultCounts getFaultCounts()const;
            void setFaultCounts(tPower::tFaultCounts);
            tPower::tDisable getDisabled()const;
            void setDisabled(tPower::tDisable);
        };

        struct NetComm{
            uint32_t ref_num;
            std::function<void(uint32_t)> occurFunction;
        };

        struct SysWatchdog{
        private:
            tSysWatchdog::tStatus status;
        
        public:
            tSysWatchdog::tStatus getStatus()const;
            void setStatus(tSysWatchdog::tStatus);
        };
    	
        struct Global{
        private:
            uint64_t fpga_start_time;

        public:
            uint64_t getFPGAStartTime()const;
            static uint64_t getCurrentTime();
            Global();
        };

        /**
    	 * \var bool user_button
    	 * \represents the state of the user button on the roborio
    	 */

    	bool user_button;

        Accelerometer accelerometer;
        std::array<Accumulator, hal::kNumAnalogInputs> accumulators; 
        AnalogInputs analog_inputs;
        AnalogOutputs analog_outputs;
    	CANBus can_bus;
        std::array<Counter, Counter::MAX_COUNTER_COUNT> counters;
        DIOSystem digital_system;
        std::vector<DSError> ds_errors;
        DriverStationInfo driver_station_info;
        std::array<Encoder, hal::kNumEncoders> encoders;
        Global global;
        std::array<Joystick, Joystick::MAX_JOYSTICK_COUNT> joysticks;
        NetComm net_comm;
        Power power;
        PWMSystem pwm_system;
    	RelaySystem relay_system;
    	RobotState robot_state;
        SysWatchdog watchdog;

        explicit RoboRIO() = default;

        friend class RoboRIOManager;
    private:
        RoboRIO(RoboRIO const&) = default;
        RoboRIO& operator=(const RoboRIO& r) = default;
    };
      /**
     * 
     */

    class RoboRIOManager {


    public:

        // This is the only method exposed to the outside.
        // All other instance getters should be private, accessible through friend classes

        static std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> getInstance() {
            static int counter = 0;
            std::unique_lock<std::recursive_mutex> lock(m);
            if (instance == nullptr) {
                instance = std::make_shared<RoboRIO>();
            }
            if (counter > 2000) {
                auto send_instance = SendDataManager::getInstance();
                //send_instance.first->update();
                send_instance.second.unlock();
            }
            counter++;
            return std::make_pair(instance, std::move(lock));
        }

        static std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> getInstance(void*) {
            std::unique_lock<std::recursive_mutex> lock(m);
            if (instance == nullptr) {
                instance = std::make_shared<RoboRIO>();
            }
            return std::make_pair(instance, std::move(lock));
        }

        static RoboRIO getCopy() {
            return RoboRIO(*(RoboRIOManager::getInstance().first));
        }

        enum class Buffer {
            Recieve,
            Execute,
            Send,
        };


    private:
        RoboRIOManager() {}
        static std::shared_ptr<RoboRIO> instance;

        static std::recursive_mutex m;
    public:

        RoboRIOManager(RoboRIOManager const&) = delete;
        void operator=(RoboRIOManager const&) = delete;

        friend class SyncServer;
    };
    
}
#endif
