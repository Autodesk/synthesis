#ifndef _SYNC_SERVER_HPP_
#define _SYNC_SERVER_HPP_

#include "roborio.hpp"
#include <asio.hpp>

#define SEND_PORT 11001

namespace hel {
    class SyncServer {
    public:
        SyncServer(asio::io_service& io);
        void startSync(asio::io_service& io);

    private:
        asio::ip::tcp::endpoint endpoint;
    };
}

#endif /* _SYNC_SERVER_HPP_ */
