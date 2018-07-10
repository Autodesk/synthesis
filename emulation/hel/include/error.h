namespace hel{
    struct Error{
        virtual std::string toString()const = 0;
    };

    struct DSError: public Error{
    private:
        enum class Type{WARNING, ERROR};
        Type type;
        int32_t error_code;
        std::string details;
        std::string location;
        std::string callStack;
    public:
        std::string toString()const{
            return "";
        }

        DSError(bool is_error, int32_t ec, const char* det, const char* loc, const char* cs){
            type = is_error ? Type::ERROR : Type::WARNING;
            error_code = ec;
            details = det;
            location = loc;
            callStack = cs;
        }
    };
}
