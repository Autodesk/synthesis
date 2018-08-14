#ifndef _JSON_UTIL_HPP_
#define _JSON_UTIL_HPP_

#include <vector>
#include <string>
#include <functional>
#include "bounds_checked_array.hpp"

namespace hel{
    /**
     * \brief A unique character to end JSON transmissions with
     * This enables the receiving end to more easily differentiate between packets
     */

    constexpr char JSON_PACKET_SUFFIX = '\x1B';

    /**
     * \brief An exception representing when parsing failed due to unexpected data format
     */

    struct JSONParsingException: std::exception{
    private:
        /**
         * \brief Details about the JSON parsing exception
         */

        std::string details;

    public:
        /**
         * \brief Returns the exception message
         */

        const char* what()const throw();

        /**
         * Constructor for JSONParsingException
         * \param det The details of the parsing exception
         */

        JSONParsingException(std::string);
    };

    /**
     * \brief Serialize an iterable container as a JSON string
     * \param label The JSON label to use
     * \param iterable The data to serialize
     * \param to_s A serialization function for the container's value type
     * \return The JSON serialized data
     */

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
     * \brief Removes duplicate spaces from a string
     * \param input The string to clean extra spaces from
     * \return The cleaned string
     */
    std::string removeExtraneousSpaces(std::string);

    /**
     * \brief Remove the given characters from the input string
     * \param input The string to remove characters from
     * \param characters The characters to exclude
     * \return The input string excluding the specified characters
     */

    std::string excludeFromString(const std::string&, const std::vector<char>&);

    /**
     * \brief Trims a string of preceding and following whitespace
     * \param input The string to trim
     * \return The trimmed string
     */

    std::string trim(std::string);

    /**
     * \brief Splits a string into a vector of string along the given delimiter
     * \param input the string to split
     * \param DELIMITER The delimiter to split along
     * \return A vector of strings from the input split along the delimiter, excluding the delimiter
     */

    std::vector<std::string> split(std::string, const char);

    /**
     * \brief Splits an input string into the found JSON sub-objects
     * \param input The string to parse
     * \return A vector of all the found JSON objects
     */

    std::vector<std::string> splitObject(std::string);

    /**
     * \brief Remove the outer brackets from a JSON list
     * \param input The list to trim
     * \return The input list without outer list brackets
     */

    std::string clipList(std::string);

    /**
     * \brief Removes and returns a string representing a JSON object of the given label from the input
     * \param label The target object
     * \param input The string to parse
     * \return A string containing the value pulled from the input string
     */

    std::string pullObject(std::string,std::string&);

    /**
     * \brief Removes a string representing a JSON object of the given label from the input and deserializes it
     * \param label The target object
     * \param input The string to parse
     * \param from_s The deserialization function to use
     * \return An object containing the parsed data
     */

    template<typename T>
    T pullObject(std::string label,std::string& input,const std::function<T(std::string)>& from_s){
        return from_s(pullObject(label, input));
    }

    /**
     * \brief Removes the first string representing a JSON object from the input
     * \param input The string to parse
     * \return A string containing the first JSON object in the input
     */

    std::string pullObject(std::string&,unsigned start = 0);

    /**
     * \brief Deserialize a JSON list into a vector
     * \param input The JSON string to deserialize
     * \param from_s The deserialization function for the list's value type
     * \param clip Whether to clip the JSON string's brackets or not
     * \return A vector of deserialized objects
     */

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
     * \brief Add quotes surrounding the input string
     * \param input The string to add quotes to
     * \return The string with outer quotation marks
     */

    std::string quote(const std::string&);

    /**
     * \brief Remove the outer quotation marks of a string
     * \param input The string to remove the outer quotation marks from
     * \return The trimmed string without outer quotation marks
     */

    std::string unquote(std::string);
}

#endif
