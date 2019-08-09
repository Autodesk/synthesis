#ifndef _ERROR_HPP_
#define _ERROR_HPP_

#include <string>
#include <iostream>

namespace hel{

    /**
     * \brief Base class for all error types
     */

    struct ErrorBase{
        /**
         * Deconstructor for ErrorBase
         */

        virtual ~ErrorBase(){};

        /**
         * \brief Format the error as a string
         * \return The resulting string
         */

        virtual std::string toString()const = 0;
    };

    /**
     * \brief Error message container with data to print to driver station
     */

    struct DSError: public ErrorBase{
        /**
         * \brief Represents driver station error type
         */

        enum class Type{WARNING, ERROR};

    private:
        /**
         * \brief Whether the error message is a warning or error
         */

        Type type;

        /**
         * \brief The associated error code
         */

        int32_t error_code;

        /**
         * \brief The details of the error
         */

        std::string details;

        /**
         * \brief Currently unknown functionality
         */

        std::string location;

        /**
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
         * Constructor for DSError
         * \param is_error
         * \param error_code
         * \param details
         * \param location
         * \param call_stack
         */

        DSError(bool, int32_t, const char*, const char*, const char*)noexcept;

        /**
         * Constructor for DSError
         * \param source A DSError object to copy
         */

        DSError(const DSError&)noexcept;
    };

    /**
     * \fn std::string asString(DSError::Type type)
     * \brief Formats a DSError::Type as a string
     * \param type The value to convert
     * \return The type formatted as a string
     */

    std::string asString(DSError::Type);

    /**
     * \brief An exception for enum constant comparisons which are not handled
     */

    struct UnhandledEnumConstantException: public std::exception{
    private:
        /**
         * \brief The type of enum represented in string form
         */

        std::string enum_type;

    public:
        /**
         * Constructor for UnhandledEnumConstantException
         * \param enum_type The enum type with the unhandled case as a string
         */

        UnhandledEnumConstantException(std::string)noexcept;

        /**
         * \fn const char* what()const throw
         * \brief Returns the exception message
         */

        const char* what()const throw();
    };

    /**
     * \brief A generic exception for unhandled cases when all should be handled
     */

    struct UnhandledCase: public std::exception{
        /**
         * \fn const char* what()const throw
         * \brief Returns the exception message
         */

        const char* what()const throw();
    };

    /**
     * \brief A generic exception for when user code attempts to access a feature unsupported by Synthesis
     */

    struct UnsupportedFeatureException: std::exception{
    private:
        /**
         * \brief Details about the unsupported feature
         */

        std::string details;

    public:
        /**
         * \fn const char* what()const throw
         * \brief Returns the exception message
         */

        const char* what()const throw();

        /**
         * Constructor for UnsupportedFeatureException
         */

        UnsupportedFeatureException()noexcept;

        /**
         * Constructor for UnsupportedFeatureException
         * \param details Details about the unsupported feature to include in the exception message
         */

        UnsupportedFeatureException(std::string)noexcept;
    };

    /**
     * \brief An exception for input configuration mismatches between user code and the exported robot
     */

    struct InputConfigurationException: std::exception{
    private:
        /**
         * \brief Details about the input misconfiguration
         */

        std::string details;

    public:
        /**
         * \fn const char* what()const throw
         * \brief Returns the exception message
         */

        const char* what()const throw();

        /**
         * Constructor for InputConfigurationException
         * \param details Details about the input misconfiguration to include in the exception message
         */

        InputConfigurationException(std::string)noexcept;
    };
}

#endif
