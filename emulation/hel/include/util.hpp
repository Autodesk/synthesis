#ifndef _UTIL_HPP_
#define _UTIL_HPP_

#include <functional>
#include <vector>
#include <cassert>

#define NYI {\
    printf("NYI:" + __FILE__ + ":" __LINE__ + "\n"); \
    exit(1); \
}

namespace hel{

    /**
     * \fn constexpr unsigned findMostSignificantBit(T value)
     * \brief Finds the most significant bit in an integer
     * Finds the index of the high bit of the greatest value; returns zero if zero is passed in
     * \tparam T The type of integer
     * \param value The integer to analyze
     * \return The zero-indexed index of the most significant bit
     */

    template<typename T, typename = std::enable_if<std::is_integral<T>::value>>
    constexpr unsigned findMostSignificantBit(T value){
    	unsigned most_significant_bit = 0;

    	while(value != 0){
    		value >>= 1;
    		most_significant_bit++;
    	}

    	return most_significant_bit;
    }

    /**
     * \fn constexpr bool checkBitHigh(const T& value, const unsigned& index)
     * \brief Check if the bit in place \b index of integer \b value is high
     * \tparam T The type of integer
     * \param value The integer to analyze
     * \param index The zero-indexed index to check
     * \return True if the bit is high
     */

    template<typename T, typename = std::enable_if<std::is_integral<T>::value>>
    constexpr bool checkBitHigh(const T& value,const unsigned& index){
    	return value & (1u << index);
    }

    /**
     * \fn constexpr bool checkBitLow(const T& value, const unsigned& index)
     * \brief Check if the bit in place \b index of integer \b value is low
     * \tparam T The type of integer
     * \param value The integer to analyze
     * \param index The zero-indexed index to check
     * \return True if the bit is high
     */

    template<typename T, typename = std::enable_if<std::is_integral<T>::value>>
    constexpr bool checkBitLow(const T& value,const unsigned& index){
        return !checkBitHigh(value, index);
    }

    /**
     * \fn constexpr TInteger setBit(TInteger integer,bool bit, TIndex index)
     * \brief Set a bit at a given index in a given integer to a given value
     * \param integer The integer to modify
     * \param bit The value of bit to use
     * \param index The zero-indexed index to set the bit at
     * \return The modified integer
     */

    template<typename TInteger, typename TIndex, typename = std::enable_if<std::is_integral<TInteger>::value && std::is_integral<TIndex>::value>>
    constexpr TInteger setBit(TInteger integer,bool bit, TIndex index){
        integer &= ~(1u << index);
        if(bit){
            integer |= 1u << index;
        }

        return integer;
    }

    template<typename T>
    struct Maybe {
    private:
        T _data;
        bool _is_valid;

    public:
        template<typename R>
        constexpr Maybe<R> bind(const std::function<Maybe<R>(T)>& f)const {
            if (!_is_valid) {
                return Maybe<R>();
            }
            return f(_data);
        }

        template<typename R>
        constexpr Maybe<R> operator>>=(const std::function<Maybe<R>(T)>& f)const {
            bind(f);
        }

        template<typename R>
        inline static std::function<Maybe<R>(Maybe<T>)> lift(const std::function<R(T)>& f){
            return (std::function<Maybe<R>(Maybe<T>)>)[f](Maybe<T> arg) {
                return Maybe<R>(f(arg._data));
            };
        }

        template<typename R>
        constexpr Maybe<R> fmap(const std::function<Maybe<R>(Maybe<T>)>& f)const {
            if(!_is_valid) {
                return Maybe<R>();
            }
            Maybe<R> out = f(*this);
            return out;
        }

        constexpr T& get()noexcept{
            assert(_is_valid);
            return _data;
        }
        void set(T data){
            _data = data;
            _is_valid = true;
        }

        constexpr operator bool()const noexcept{
            return _is_valid;
        }

        Maybe& operator=(const Maybe& m)noexcept {
            if(this != &m){
                _data = m._data;
                _is_valid = m._is_valid;
            }
            return *this;
        }

        Maybe(T data)noexcept:_data(data), _is_valid(true){}
        Maybe()noexcept: _is_valid(false) {}
    };

    /**
     * \brief Function to format an iterable container as a string
     * \param iterable The container to convert
     * \param to_s A conversion function from the iterable's value type to a string
     * \param delimiter The delimiter to use between elements
     * \param include_brackets Whether to surround the generated string in brackets or not
     */

    template<typename T>
    std::string as_string(const T& iterable, const std::function<std::string(typename T::value_type)>& to_s, const std::string& delimiter = ",", const bool& include_brackets = true){ //TODO use typename Func instead of std::function
        std::string s = "";
        if(include_brackets){
            s += "[";
        }
        for(auto i = iterable.begin(); i != iterable.end(); ++i){
            if(i != iterable.begin()){
                s += delimiter;
            }
            s += to_s(*i);
        }
        if(include_brackets){
            s += "]";
        }
        return s;
    }

    /**
     * \brief Function to format a pair as a string
     * \param a The pair to convert
     * \param first_to_s A conversion function from the pair's first value type to a string
     * \param second_to_s A conversion function from the pair's second value type to a string
     * \param delimiter The delimiter to use between the two
     * \param include_brackets Whether to surround the generated string in brackets or not
     */

    template<typename First, typename Second>
    std::string as_string(const std::pair<First, Second>& a, const std::function<std::string(First)>& first_to_s, const std::function<std::string(Second)>& second_to_s, const std::string& delimiter = ",", const bool& include_brackets = true){
        std::string s = "";
        if(include_brackets){
            s += "[";
        }
        s += first_to_s(a.first);
        s += delimiter;
        s += second_to_s(a.second);
        if(include_brackets){
            s += "]";
        }
        return s;
    }

    /**
     * \brief Parse a boolean value from a string
     * \param input The string to parse
     * \return The parsed bool
     */

    bool stob(std::string);

    /**
     * \brief Convert a bool to a string
     * \param input The bool to convert
     * \return The resulting string
     */

    std::string as_string(bool);

    /**
     * \brief Hash function for a constant character array
     * \param input The character array to hash
     * \return The resulting hash
     */

    constexpr std::size_t hasher(const char* input){
        return *input ? static_cast<unsigned>(*input) + 33 * hasher(input + 1) : 5381;
    }

    /**
     * \brief Compare the bits in two integers given a comparison mask
     * \param a The first integer to compare
     * \param b The second integer to compare
     * \param comparison_mask The comparison mask to use. The function will compare all the bits where the comparison mask is high
     * \return True if all the specified bits match
     */

    constexpr bool compareBits(uint32_t a, uint32_t b, uint32_t comparison_mask){
        unsigned msb = std::max(findMostSignificantBit(a), findMostSignificantBit(b));
        for(unsigned i = 0 ; i < msb; i++){
            if(checkBitHigh(comparison_mask,i)){
                if(checkBitHigh(a,i) != checkBitHigh(b,i)){
                    return false;
                }
            }
        }
        return true;
    }
}

#endif
