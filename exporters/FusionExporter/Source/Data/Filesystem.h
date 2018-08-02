#pragma once

#include <string>

class Filesystem
{
public:
	static std::string getCurrentRobotDirectory(std::string robotName);
	static void createDirectory(std::string);

private:
	static const std::string ROBOT_APPDATA_DIRECTORY;

};
