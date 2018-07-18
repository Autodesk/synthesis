#ifndef _ERROR_H_
#define _ERROR_H_

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
    private:
        /**
         * \enum Type
         * \brief Represents driver station error type
         */

        enum class Type{WARNING, ERROR};
        
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

        std::string toString()const{
            return ""; //TODO
        }

        /**
         * \brief Constructs new DSError given values from NI FPGA's sendError function
         */

        DSError(bool is_error, int32_t ec, const char* det, const char* loc, const char* cs){
            type = is_error ? Type::ERROR : Type::WARNING;
            error_code = ec;
            details = det;
            location = loc;
            call_stack = cs;
        }
    };
}

#endif
