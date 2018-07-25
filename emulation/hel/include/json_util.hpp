#ifndef _JSON_UTIL_HPP_
#define _JSON_UTIL_HPP_

#include <vector>
#include <string>
#include <functional>
#include "bounds_checked_array.hpp"

namespace hel{
    struct JSONParsingException: std::exception{
        const char* what()const throw();
    };

    template<typename T, size_t LEN>
    std::string serializeList(std::string label, std::array<T, LEN> arr, std::function<std::string(T)> to_s){
        std::string s = label + ":[";
        for(unsigned i = 0; i < arr.size(); i++){
            s += to_s(arr[i]);
            if((i + 1) < arr.size()){
                s += ",";
            }
        }
        s += "]";
        return s;
    }

    template<typename T, size_t LEN>
    std::string serializeList(std::string label, BoundsCheckedArray<T, LEN> arr, std::function<std::string(T)> to_s){
        serializeList(label, arr, to_s);
    }

    std::string removeExtraneousSpaces(std::string);

    std::string excludeFromString(std::string, std::vector<char>);

    std::string trim(std::string);

    std::vector<std::string> split(std::string, const char);

    std::vector<std::string> splitObject(std::string);

    std::string clipList(std::string);

    std::string pullValue(std::string,std::string&);

    template<typename T>
    T pullValue(std::string label,std::string& input,std::function<T(std::string)> from_s){
        return from_s(pullValue(label, input));
    }

    std::string pullObject(std::string&);

    template<typename T>
    std::vector<T> deserializeList(std::string input, std::function<T(std::string)> from_s, bool clip = false){
        if(clip){
            input = clipList(input);
        }
        std::vector<T> v;
        for(std::string i: splitObject(input)){
            v.push_back(from_s(removeExtraneousSpaces(i)));
        }
        return v;
    }

    std::string quote(std::string);
    std::string unquote(std::string);
}

#endif
