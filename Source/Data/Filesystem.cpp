#include "Filesystem.h"

const std::string Filesystem::ROBOT_DIRECTORY = "C:/Users/t_walkn/Desktop/";
std::string Filesystem::robotName = "unnamed";

void Filesystem::setRobotName(std::string name)
{
	robotName = name;
}

std::string Filesystem::getCurrentRobotDirectory()
{
	return ROBOT_DIRECTORY + '/' + robotName + '/';
}
