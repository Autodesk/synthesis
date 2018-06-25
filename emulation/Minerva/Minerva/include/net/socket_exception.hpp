#ifndef _SOCKET_EXCEPTION_HPP_
#define _SOCKET_EXCEPTION_HPP_

#include <string>
#include <exception>

namespace minerva::net {
    class SocketException : public std::exception {
    public:
        SocketException(const std::string &message) throw ();
        SocketException() throw();
        virtual const char* what () const throw () { return message.c_str();}
    private:
        std::string message;
    };
}

#endif /* _SOCKET_EXCEPTION_HPP_ */
