#ifndef _SYNC_SERVER_H_
#define _SYNC_SERVER_H_

#include "roborio.h"
#include <asio.hpp>

namespace hel {
    class SyncServer {
        SyncServer(asio::io_service& io_service) : socket(io_service, asio::ip::udp::endpoint(asio::ip::udp::v4(), 11000)) {
            startSync();
        }

    private:
        asio::ip::udp::socket socket;

        void startSync();
    };
}

#endif /* _SYNC_SERVER_H_ */
