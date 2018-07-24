#ifndef _SYNC_SERVER_HPP_
#define _SYNC_SERVER_HPP_

#include "roborio.hpp"
#include <asio.hpp>

namespace hel {
    class SyncServer {
    public:
        SyncServer(asio::io_service& io);
        void startSync(asio::io_service& io);

    private:
        asio::ip::tcp::endpoint endpoint;
        int packet_number = 0;
    };
}

#endif /* _SYNC_SERVER_HPP_ */
