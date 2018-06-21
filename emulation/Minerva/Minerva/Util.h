#ifndef UTIL_H
#define UTIL_H

#include <vector>
#include <string>

namespace minerva{

	std::string excludeFromString(std::string, std::vector<char>);

	std::string removeExtraneousSpaces(std::string str);

	std::string trim(std::string);

	std::vector<std::string> split(std::string, const char);

}

#endif