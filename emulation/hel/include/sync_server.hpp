#ifndef _SYNC_SERVER_H_
#define _SYNC_SERVER_H_

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

#endif /* _SYNC_SERVER_H_ */
