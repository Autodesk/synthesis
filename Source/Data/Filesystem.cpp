#include "Filesystem.h"

const std::string Filesystem::ROBOT_APPDATA_DIRECTORY = "Synthesis\\Robots";
std::string Filesystem::robotName = "unnamed";

void Filesystem::setRobotName(std::string name)
{
	if (name.length() > 0)
		robotName = name;
	else
		robotName = "untitled";
}

std::string Filesystem::getCurrentRobotDirectory()
{
	char *dir;
	errno_t err = _dupenv_s(&dir, NULL, "APPDATA");

	if (err)
		throw "Failed to get AppData directory!";

	std::string strDir(dir);
	free(dir);

	return strDir + '\\' + ROBOT_APPDATA_DIRECTORY + '\\' + robotName + '\\';
}
