#ifndef _ERROR_HPP_
#define _ERROR_HPP_

#include <string>

namespace hel{

    /**
     * \struct Error
     * \brief Base class for all error types
     */

    struct ErrorBase{
        virtual std::string toString()const = 0;
    };

    /**
     * \struct DSError
     * \brief Error message container with data to print to driver station
     */

    struct DSError: public ErrorBase{
        /**
         * \enum Type
         * \brief Represents driver station error type
         */

        enum class Type{WARNING, ERROR};

    private:
        /**
         * \var Type type
         * \brief Whether the error message is a warning or error
         */

        Type type;

        /**
         * \var int32_t error_code
         * \brief The associated error code
         */

        int32_t error_code;

        /**
         * \var std::string details
         * \brief The details of the error
         */

        std::string details;

        /**
         * \var std::string location
         * \brief Currently unknown functionality
         */

        std::string location;

        /**
         * \var std::string call_stack
         * \brief Currently unknown functionality
         */

        std::string call_stack;
    public:

        /**
         * \fn std::string toString()const
         * \brief Formats the error message as a string
         * \return a string containing error message data
         */

        std::string toString()const;

        /**
         * \brief Constructs new DSError given values from NI FPGA's sendError function
         */

        DSError(bool, int32_t, const char*, const char*, const char*);
    };

    std::string to_string(DSError::Type);

    /**
     * \struct UnhandledEnumConstantException: public std::exception
     * \brief An exception for enum constant comparisons which are not handled
     */

    struct UnhandledEnumConstantException: public std::exception{
    private:
        /**
         * \var std::string enum_type
         * \brief The type of enum represented in string form
         */

        std::string enum_type;

    public:
        UnhandledEnumConstantException(std::string);

        const char* what()const throw();
    };

    /**
     * \struct UnhandledCase: public std::exception
     * \brief A generic exception for unhandled cases when all should be handled
     */

    struct UnhandledCase: public std::exception{
        const char* what()const throw();
    };
}

#endif
