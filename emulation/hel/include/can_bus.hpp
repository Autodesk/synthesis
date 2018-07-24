#ifndef _CAN_BUS_HPP_
#define _CAN_BUS_HPP_

#include <queue>
#include <cstdint>
#include "bounds_checked_array.hpp"

namespace hel{

    /**
     * \struct CANBus
     * \brief Models CAN bus input and output.
     * Holds internal queues of CAN messages for input and output.
     */

    struct CANBus{

        /**
         * \struct Message
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
             * \var BoundsCheckedArray<uint8_t, 8> data
             * \brief the data transmitted with the message in byte array form
             * The data can array can vary from 0-8 bytes in size.
             */

            BoundsCheckedArray<uint8_t, 8> data;

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

            static constexpr const int32_t CAN_SEND_PERIOD_NO_REPEAT = 0;

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

            Message();
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

        CANBus();
    };
}

#endif
