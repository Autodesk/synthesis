#ifndef _UTIL_HPP_
#define _UTIL_HPP_

#include <functional>
#include <vector>

#define NYI {\
        printf("NYI:" + __FILE__ + ":" __LINE__ + "\n");\
        exit(1);\
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
    bool checkBitLow(T value,unsigned index){
        return !checkBitHigh(value, index);
    }

    template<typename TInteger, typename TIndex>
    TInteger setBit(TInteger integer,bool bit, TIndex index){
        integer &= ~(1u << index);
        if(bit){
            integer |= 1u << index;
        }

        return integer;
    }
    /*
        _   _         _            _             _                _            _        
       /\_\/\_\ _    /\ \         /\ \     _    / /\             /\ \         / /\      
      / / / / //\_\ /  \ \       /  \ \   /\_\ / /  \           /  \ \____   / /  \     
     /\ \/ \ \/ / // /\ \ \     / /\ \ \_/ / // / /\ \         / /\ \_____\ / / /\ \__  
    /  \____\__/ // / /\ \ \   / / /\ \___/ // / /\ \ \       / / /\/___  // / /\ \___\ 
   / /\/________// / /  \ \_\ / / /  \/____// / /  \ \ \     / / /   / / / \ \ \ \/___/ 
  / / /\/_// / // / /   / / // / /    / / // / /___/ /\ \   / / /   / / /   \ \ \       
 / / /    / / // / /   / / // / /    / / // / /_____/ /\ \ / / /   / / /_    \ \ \      
/ / /    / / // / /___/ / // / /    / / // /_________/\ \ \\ \ \__/ / //_/\__/ / /      
\/_/    / / // / /____\/ // / /    / / // / /_       __\ \_\\ \___\/ / \ \/___/ /       
        \/_/ \/_________/ \/_/     \/_/ \_\___\     /____/_/ \/_____/   \_____\/        
                                                                                        
     */
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

    template<typename T>
    std::string to_string(T iterable, std::function<std::string(typename T::value_type)> to_s, std::string delimiter = ",", bool include_brackets = true){
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
    std::string to_string(std::pair<FIRST, SECOND> a, std::function<std::string(FIRST)> first_to_s, std::function<std::string(SECOND)> second_to_s, std::string delimiter = ",", bool include_brackets = true){
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
    std::string to_string(bool);//TODO maybe rename these? namespace helps prevent accidental type cast and wrong call, but still happens

    constexpr std::size_t hasher(const char* input){
        return *input ? static_cast<unsigned>(*input) + 33 * hasher(input + 1) : 5381;
    }

    void copystr(const std::string&, char*);

    bool compareBits(uint32_t, uint32_t, uint32_t);
}

#endif
