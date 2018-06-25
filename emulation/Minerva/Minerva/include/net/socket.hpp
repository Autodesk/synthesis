#ifndef _SOCKET_HPP_
#define _SOCKET_HPP_

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <sys/wait.h>
#include <signal.h>

#include <string>

#include "socket_exception.hpp"

namespace minerva::net {

    class Socket {
    public:

        ~Socket();

        enum SocketType {
            TcpSocket = SOCK_STREAM,
            UdpSocket = SOCK_DGRAM,
            Unknown = -1,
        };

        enum NetworkLayerProtocol {
            IPv4Protocol = AF_INET,
            IPv6Protocol = AF_INET6,
            UnknownNetworkLayerProtocol = -1
        };

        enum Result {
            Success = 0,
            TimedOut = ETIMEDOUT,
            Exception = 255
        };

        std::string getLocalAddress() noexcept(false);

        unsigned short getLocalPort() noexcept(false);

        void bindLocalPort(unsigned short local_port) noexcept(false);

        void bindLocalAddressAndPort(const std::string &local_address, unsigned short port);

        unsigned long int getReadBufferSize();

        void setReadBufferSize(unsigned int size) noexcept(false);

        void setBlockingMode(bool mode) noexcept(false);

        void connect(const std::string &address, unsigned short remote_port) noexcept(false);

        void send(const void *buff, int len);

        int recv(void *buff, int len);

        std::string getRemoteAddress() noexcept(false);

        unsigned short getRemotePort() noexcept(false);

        Socket& operator<<(const std::string& string);

        Socket& operator>>(std::string& str);

        virtual int blockingRecv(unsigned long timeout);

    protected:
        int socket_descriptor_;

        Socket(SocketType type, NetworkLayerProtocol proto) noexcept(false);
        Socket(int socket_descriptor);
        static void fillAddr(const std::string &local_address, unsigned short port, sockaddr_in& local_addr);
    private:
        Socket(const Socket & socket);
        void operator=(const Socket &socket);
    };
}

#endif /* _SOCKET_HPP_ */
