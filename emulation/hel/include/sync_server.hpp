#ifndef _SYNC_SERVER_HPP_
#define _SYNC_SERVER_HPP_

#include "roborio.hpp"
#include <asio.hpp>

namespace hel {
    class SyncServer {
    public:
        SyncServer(asio::io_service& io_service) : socket(io_service) {}
        void startSync();

    private:
        asio::ip::udp::socket socket;

    };
}

#endif /* _SYNC_SERVER_HPP_ */
