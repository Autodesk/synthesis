#ifndef _SYNC_CLIENT_HPP_
#define _SYNC_CLIENT_HPP_

#include "roborio.hpp"
#include <asio.hpp>

namespace hel {
    class SyncClient {
    public:
        SyncClient(asio::io_service& io);
        void startSync(asio::io_service& io);

    private:
        asio::ip::tcp::endpoint endpoint;
    };
}

#endif /* _SYNC_CLIENT_HPP_ */
