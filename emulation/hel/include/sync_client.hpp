#ifndef _SYNC_CLIENT_HPP_
#define _SYNC_CLIENT_HPP_

#include "roborio.hpp"
#include <asio.hpp>

#define RECEIVE_PORT 11000

namespace hel {
    /**
     * \brief TCP socket receiver used in communication with Synthesis's engine
     */

    class SyncClient {
    public:
        /**
         * Constructor for SyncClient
         */

        SyncClient(asio::io_service& io);

        /**
         * \brief Begins and runs receiver in a background thread
         */

        void startSync(asio::io_service& io);

    private:
        asio::ip::tcp::endpoint endpoint;
    };
}

#endif /* _SYNC_CLIENT_HPP_ */
