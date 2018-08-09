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
     * \fn unsigned findMostSignificantBit(T value)
     * \brief Finds the most significant bit in an integer
     * Finds the index of the high bit of the greatest value; returns zero if zero is passed in
     * \type T the type of integer
     * \param value the integer to analyze
     * \return the zero-indexed index of the most significant bit
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
     * \fn bool checkBitHigh(T value, unsigned index)
     * \brief Check if the bit in place \b index of integer \b value is high
     * \type T the type of integer
     * \param value the integer to analyze
     * \param index the zero-indexed index to check
     * \return true if the bit is high
     */

    template<typename T, typename = std::enable_if<std::is_integral<T>::value>>
    constexpr bool checkBitHigh(const T& value,const unsigned& index){
    	return value & (1u << index);
    }

    template<typename T, typename = std::enable_if<std::is_integral<T>::value>>
    constexpr bool checkBitLow(T value,unsigned index){
        return !checkBitHigh(value, index);
    }

    template<typename TInteger, typename TIndex, typename = std::enable_if<std::is_integral<TInteger>::value && std::is_integral<TIndex>::value>>
    constexpr TInteger setBit(TInteger integer,bool bit, TIndex index){
        integer &= ~(1u << index);
        if(bit){
            integer |= 1u << index;
        }

        return integer;
    }

	template<typename T>
    struct Maybe { //TODO optimize

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

      constexpr operator bool()const noexcept{return _is_valid;}

        Maybe& operator=(const Maybe& m)noexcept {_data = m._data; _is_valid = m._is_valid;}

        Maybe(T data)noexcept : _data(data), _is_valid(true) {};
        Maybe()noexcept : _is_valid(false) {};
    };

    template<typename T>
    std::string as_string(const T& iterable, const std::function<std::string(typename T::value_type)>& to_s, const std::string& delimiter = ",", const bool& include_brackets = true){
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

    template<typename FIRST, typename SECOND>
    std::string as_string(const std::pair<FIRST, SECOND>& a, const std::function<std::string(FIRST)>& first_to_s, const std::function<std::string(SECOND)>& second_to_s, const std::string& delimiter = ",", const bool& include_brackets = true){
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

	bool stob(std::string);
    std::string as_string(bool);

    constexpr std::size_t hasher(const char* input){
        return *input ? static_cast<unsigned>(*input) + 33 * hasher(input + 1) : 5381;
    }

    void copystr(const std::string&, char*);

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
