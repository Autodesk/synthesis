#ifndef _SYNC_SERVER_HPP_
#define _SYNC_SERVER_HPP_

#include "roborio.hpp"
#include <asio.hpp>

#define SEND_PORT 11001

namespace hel {
    /**
     * \brief TCP socket transmitted used in communication with Synthesis's engine
     */

    class SyncServer {
    public:
        /**
         * Constructor for SyncServer
         */

        SyncServer(asio::io_service& io);

        /**
         * \brief Begins and runs transmitter in a background thread
         */

        void startSync(asio::io_service& io);

    private:
        asio::ip::tcp::endpoint endpoint;
    };
}

#endif /* _SYNC_SERVER_HPP_ */
