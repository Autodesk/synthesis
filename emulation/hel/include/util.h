#ifndef _UTIL_H_
#define _UTIL_H_

#include <functional>

namespace hel{

    /**
     * \fn unsigned findMostSignificantBit(T value)
     * \brief Finds the most significant bit in an integer
     * Finds the index of the high bit of the greatest value; returns zero if zero is passed in
     * \type T the type of integer
     * \param value the integer to analyze
     * \return the zero-indexed index of the most significant bit
     */
    
    template<typename T>
    unsigned findMostSignificantBit(T value){
    	unsigned most_significant_bit = 0;
    	
    	while(value != 0){
    		value /= 2;
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

    template<typename T>
    bool checkBitHigh(T value,unsigned index){
    	index = 1u << index; 
    	return value & index;
    }

    template<typename T>
    struct Maybe {

    private:

        T _data;
        bool _is_valid;

    public:
        template<typename R>
        Maybe<R> bind(std::function<Maybe<R>(T)> f) {
            if (!_is_valid) {
                return Maybe<R>();
            }
            return f(_data);
        }

        template<typename R>
        Maybe<R> operator>>=(std::function<Maybe<R>(T)> f) {
            bind(f);
        }

        template<typename R>
        std::function<Maybe<R>(Maybe<T>)> liftM(std::function<R(T)> f) {
            return [f](Maybe<T> arg) {
                return Maybe(f(arg._data));
            };
        }

        template<typename R>
        Maybe<R> fmap(std::function<Maybe<R>(Maybe<T>)> f) {
            if(!_is_valid) {
                return Maybe<R>();
            }
            return f(this);
        }

        T get() {return _data;}
        void set(T data) {_data=data;_is_valid=true;}

        operator bool()const{return _is_valid;}

        Maybe& operator=(const Maybe& m) {_data = m._data; _is_valid = m._is_valid;}

        T operator*() {
            if (!_is_valid) {
                throw "Bad Access";
            }
            return _data;
        }

        Maybe(T data) : _data(data), _is_valid(true) {};
        Maybe() : _is_valid(false) {};
    };
}

#endif
