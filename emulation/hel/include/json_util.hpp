#ifndef _JSON_UTIL_HPP_
#define _JSON_UTIL_HPP_

#include <vector>
#include <string>
#include <functional>
#include "bounds_checked_array.hpp"

namespace hel{
    constexpr char JSON_PACKET_SUFFIX = '\x1B';

    /**
     * \struct JSONParsingException: std::exception
     * \brief An expcetion representing when parsing failed due to unexpected data format
     */

    struct JSONParsingException: std::exception{
    private:
        std::string details;

    public:
        const char* what()const throw();

        JSONParsingException(std::string);
    };

    template<typename T>
    std::string serializeList(const std::string& label, const T& iterable, std::function<std::string(typename T::value_type)> to_s){
        std::string s = label + ":[";
        for(auto i = iterable.begin(); i != iterable.end(); ++i){
            if(i != iterable.begin()){
                s += ",";
            }
            s += to_s(*i);
        }
        s += "]";
        return s;
    }

    /**
     * \fn std::string removeExtraneousSpaces(std::string input)
     * \brief Removes duplicate spaces from a string
     * \param input the string to clean extra spaces from
     * \return the cleaned string
     */
    std::string removeExtraneousSpaces(std::string);

    /**
     * \fn std::string excludeFromString(std::string input, std::vector<char> characters)
     * \brief Remove the given characters from the input string
     * \param input the string to remove characters from
     * \param characters the characters to exclude
     * \return the input string excluding the specified characters
     */

    std::string excludeFromString(const std::string&, const std::vector<char>&);

    /**
     * \fn std::string trim(std::string input)
     * \brief Trims a string of preceding and following whitespace
     * \param input the string to trim
     * \return the trimmed string
     */

    std::string trim(std::string);

    /**
     * \fn std::vector<std::string> split(std::string input, const char DELIMITER)
     * \brief Splits a string into a vector of string along the given delimiter
     * \param input the string to split
     * \param const char DELIMITER the delimiter to split along
     * \return a vector of strings from the input split along the delimiter, excluding the delimiter
     */

    std::vector<std::string> split(std::string, const char);

    /**
     * \fn std::vector<std::string> splitObject(std::string input)
     * \brief Splits an input string into the found JSON sub-objects
     * \param input the string to parse
     * \return a vector of all the found JSON objects
     */

    std::vector<std::string> splitObject(std::string);

    /**
     * \fn std::string clipList(std::string input)
     * \brief Remove the outer brackets from a JSON list
     * \param input the list to trim
     * \return the input list without outer list brackets
     */

    std::string clipList(std::string);

    /**
     * \fn std::string pullValue(std::string label, std::string& input)
     * \brief Removes a string representing the value part of the first JSON object of the given label from the input
     * \param label the target object
     * \param input the string to parse
     * \return a string containing the value pulled from the input string
     */

    std::string pullValue(std::string,std::string&);

    template<typename T>
    T pullValue(std::string label,std::string& input,const std::function<T(std::string)>& from_s){
        return from_s(pullValue(label, input));
    }

    /**
     * \fn std::string pullObject(std::string& input)
     * \brief Removes the first string representing a JSON object from the input
     * \param input the string to parse
     * \return a string containing the first JSON object in the input
     */

    std::string pullObject(std::string&);

    template<typename T>
    std::vector<T> deserializeList(std::string input, const std::function<T(std::string)>& from_s, bool clip = false){
        input = removeExtraneousSpaces(input);
        if(clip){
            input = clipList(input);
        }
        std::vector<T> v;
        for(std::string i: splitObject(input)){
            v.push_back(from_s(i));
        }
        return v;
    }

    /**
     * \fn std::string quote(std::string input)
     * \brief Add quotes surrounding the input string
     * \param input the string to add quotes to
     * \return the string with outer quotation marks
     */

    std::string quote(const std::string&);

    /**
     * \fn std::string unquote(std::string input)
     * \brief Remove the outer quotation marks of a string
     * \param input the string to remove the outer quotation marks from
     * \return the trimmed string without outer quotation marks
     */

    std::string unquote(std::string);
}

#endif
