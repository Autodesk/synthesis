#ifndef _UDP_SOCKET_HPP_
#define _UDP_SOCKET_HPP_

#include "socket.hpp"

namespace minerva::net {
    class UDPSocket : public Socket {
    public:
        UDPSocket() noexcept(false);

        UDPSocket(unsigned short port) noexcept(false);

        UDPSocket(const std::string &local_address, unsigned short port) noexcept(false);

        void disconnect() noexcept(false);

        void sendUDP(const void *buff, int len,
                     const std::string &remote_address,
                     unsigned short remote_port) noexcept(false);
        int recvUDP(void *buff, int len) noexcept(false);
    private:
        void setBroadcast();
    };
}

#endif /* _UDP_SOCKET_HPP_ */
