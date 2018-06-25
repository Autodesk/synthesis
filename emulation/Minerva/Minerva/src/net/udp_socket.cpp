#include <net/udp_socket.hpp>

namespace minerva::net {
    UDPSocket::UDPSocket() : Socket(UdpSocket, IPv4Protocol) {
        setBroadcast();
    }

    UDPSocket::UDPSocket(unsigned short local_port)
        : Socket(UdpSocket, IPv4Protocol) {
        bindLocalPort(local_port);
        setBroadcast();
    }

    UDPSocket::UDPSocket(const std::string &local_address, unsigned short local_port)
        : Socket(UdpSocket, IPv4Protocol) {
        bindLocalAddressAndPort(local_address, local_port);
        setBroadcast();
    }

    void UDPSocket::disconnect() {
        sockaddr_in null_address;
        memset(&null_address, 0, sizeof(null_address));
        null_address.sin_family = AF_UNSPEC;

        if(::connect(socket_descriptor_, (sockaddr *) &null_address, sizeof(null_address)) < 0) {
            if (errno != EAFNOSUPPORT) {
                throw SocketException("Disconnection failed (connect())");
            }
        }

    }

    void UDPSocket::sendUDP(const void *buff, int len, const std::string &remote_address, unsigned short remote_port) {
        sockaddr_in remote;
        fillAddr(remote_address, remote_port, remote);

        if(sendto(socket_descriptor_, (void*) buff, len, 0, (sockaddr *) &remote, sizeof(remote)) < 0) {
            throw SocketException("Failed to send (sendto())");
        }
    }

    int UDPSocket::recvUDP(void *buff, int len) {
        sockaddr_in client_address;
        socklen_t address_length = sizeof(client_address);
        int bytes;
        if ((bytes = recvfrom(socket_descriptor_, (void *) buff, len, 0, (sockaddr *) &client_address, (socklen_t *) &address_length)) < 0) {
            throw SocketException("Recieve failed (recvfrom())");
        }

        char* data = static_cast<char *>(buff);
        data[bytes] = '\0';
        return bytes;
    }

    void UDPSocket::setBroadcast() {
        int broadcast_permissions = 1;
        setsockopt(socket_descriptor_, SOL_SOCKET, SO_BROADCAST, (void*) &broadcast_permissions, sizeof(broadcast_permissions));
    }

}
