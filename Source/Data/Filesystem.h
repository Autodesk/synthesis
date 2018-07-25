#pragma once

#include <string>

class Filesystem
{
public:
	static void setRobotName(std::string);
	static std::string getCurrentRobotDirectory();

private:
	static const std::string ROBOT_DIRECTORY;
	static std::string robotName;

};
