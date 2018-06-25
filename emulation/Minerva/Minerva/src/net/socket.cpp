#include <fcntl.h>
#include "net/socket.hpp"

namespace minerva::net {

    Socket::~Socket() {
        ::close(socket_descriptor_);
        socket_descriptor_ = -1;
    }

    Socket::Socket(SocketType type, NetworkLayerProtocol protocol) {
        socket_descriptor_ = socket(protocol, type, 0);
        if (socket_descriptor_ < 0) {
            throw SocketException("Socket creation failed (socket()).\n");
        }
    }

    Socket::Socket(int socket_descriptor) {
        socket_descriptor_ = socket_descriptor;
    }

    Socket::Socket(const Socket &socket) {}

    void Socket::operator=(const Socket &socket) {}

    std::string Socket::getLocalAddress() noexcept(false) {
        sockaddr_in address;
        unsigned int address_length = sizeof(address);

        if (getsockname(socket_descriptor_, (sockaddr *) &address, (socklen_t *) &address_length) < 0) {
            throw SocketException("Failed to retrieve local address (getsockname())");
        }
        return inet_ntoa(address.sin_addr);
    }

    unsigned short Socket::getLocalPort() noexcept(false) {
        sockaddr_in address;
        unsigned int address_length = sizeof(address);

        if (getsockname(socket_descriptor_, (sockaddr *) &address, (socklen_t *) &address_length) < 0) {
            throw SocketException("Failed to retrieve local address (getsockname())");
        }
        return ntohs(address.sin_port);
     }

    void Socket::bindLocalPort(unsigned short local_port) {

        sockaddr_in local_address;
        memset(&local_address, 0, sizeof(local_address));

        local_address.sin_family = AF_INET;
        local_address.sin_addr.s_addr = htonl(INADDR_ANY);
        local_address.sin_port = htons(local_port);

        if (bind(socket_descriptor_, (sockaddr *) &local_address, sizeof (local_address)) < 0) {
            throw SocketException("Failed to bind local port (bind())");
        }

    }

    void Socket::bindLocalAddressAndPort(const std::string &local_address,
                                         unsigned short local_port) noexcept(false) {
        sockaddr_in local_addr;
        fillAddr(local_address, local_port, local_addr);

        if (bind(socket_descriptor_, (sockaddr *) &local_addr, sizeof(local_addr)) < 0) {
            throw SocketException("Failed to bind local port and address (bind())");
        }
    }

    void Socket::fillAddr(const std::string &local_address, unsigned short local_port, sockaddr_in& local_addr) {
        memset(&local_addr, 0, sizeof(local_addr));
        local_addr.sin_family = AF_INET;

        hostent *host;
        if ((host = gethostbyname(local_address.c_str())) == NULL) {
            throw SocketException("Failed to resolve hostname (gethostbyname())");
        }
        local_addr.sin_addr.s_addr = *((unsigned long *) host->h_addr_list[0]);
        local_addr.sin_port = htons(local_port);
    }

    unsigned long int Socket::getReadBufferSize() {
        unsigned long int size;
        socklen_t n = sizeof(size);
        getsockopt(socket_descriptor_, SOL_SOCKET, SO_RCVBUF, (void*) &size, (&n));
        return size;
    }

    void Socket::setReadBufferSize(unsigned int size) {
        if (setsockopt(socket_descriptor_, SOL_SOCKET, SO_RCVBUF, &size, sizeof(size)) == -1) {
            throw SocketException("Failed to set buffer size");
        }
    }

    void Socket::setBlockingMode(bool mode) {
        int opts;
        opts = fcntl (socket_descriptor_, F_GETFL);
        if (opts < 0)
            return;
        if (mode)
            opts = (opts|O_NONBLOCK);
        else
            opts = (opts & ~O_NONBLOCK);
        fcntl(socket_descriptor_, F_SETFL,opts);

    }

    void Socket::connect(const std::string &address, unsigned short remote_port) {
        sockaddr_in remote_address;
        fillAddr(address, remote_port, remote_address);
        if (::connect(socket_descriptor_, (sockaddr*) &remote_address, sizeof(remote_address)) <0) {
            throw SocketException("Failed to connect (connect())");
        }
    }

    void Socket::send(const void *buff, int len) {
        if (::send(socket_descriptor_, (void *) buff, len, 0) < 0) {
            throw SocketException("Failed to send (send())");
        }
    }

    int Socket::recv(void * buff, int len)  {
        int bytes;
        if ((bytes = ::recv(socket_descriptor_, (void *) buff, len, 0)) <0) {
            throw SocketException("Failed to recieve data (recv())");
        }
        char* data = static_cast<char *>(buff);
        data[bytes] = '\0';
        return bytes;
    }

    std::string Socket::getRemoteAddress() {
        sockaddr_in address;
        unsigned int address_length = sizeof(address);

        if (getpeername(socket_descriptor_, (sockaddr *) &address, (socklen_t*) &address_length) < 0) {
            throw SocketException("Failed to retrieve remote addres (getpeername())");
        }
        return inet_ntoa(address.sin_addr);
    }

    unsigned short Socket::getRemotePort() {
        sockaddr_in address;
        unsigned int address_length = sizeof(address);

        if (getpeername(socket_descriptor_, (sockaddr *) &address, (socklen_t*) &address_length) < 0) {
            throw SocketException("Failed to retrieve remote addres (getpeername())");
        }
        return htons(address.sin_port);
    }

    Socket& Socket::operator<<(const std::string& string) {
        send(string.c_str(), string.length());
        return *this;
    }

    Socket& Socket::operator>>( std::string& string) {
        char *buff = new char[getReadBufferSize()];
        recv(buff, getReadBufferSize());
        string.append(buff);
        delete [] buff;
        return *this;
    }
}
