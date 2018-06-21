#ifndef UTIL_H
#define UTIL_H

#include <vector>
#include <string>

namespace minerva{

	/**
	 * \brief Removes specified characters from a string
	 * 
	 * @param The string to modify
	 * @param A vector of characters to remove from the string
	 * @return A modified string with the specified characters removed
	 */
	std::string excludeFromString(std::string, std::vector<char>);

	/**
	 * \brief Replaces groups of spaces in a string with a single space 
	 * 
	 * @param The string to modify
	 * @return A string with extraneous spaces removed
	 */
	std::string removeExtraneousSpaces(std::string str);

	/**
	 * \brief Removes whitespace at the beginning and end of a string
	 * 
	 * @param The string to modify
	 * @return A string without whitespace at its start or end
	 */
	std::string trim(std::string);

	/**
	 * \brief Separates a string into smaller strings based on a given delimiter
	 * 
	 * @param The string to split
	 * @param The delimiter to split based upon
	 * @param A vector of substrings from the original split at locations of the delimiter excluding the delimiters
	 */
	std::vector<std::string> split(std::string, const char);

}

#endif