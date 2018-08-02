#include "Filesystem.h"
#include <windows.h>

const std::string Filesystem::ROBOT_APPDATA_DIRECTORY = "Synthesis\\Robots";

std::string Filesystem::getCurrentRobotDirectory(std::string name)
{
	if (name.length() <= 0)
		name = "unnamed";

	char *dir;
	errno_t err = _dupenv_s(&dir, NULL, "APPDATA");

	if (err)
		throw "Failed to get AppData directory!";

	std::string strDir(dir);
	free(dir);

	return strDir + '\\' + ROBOT_APPDATA_DIRECTORY + '\\' + name + '\\';
}

void Filesystem::createDirectory(std::string path)
{
	std::wstring sPath = std::wstring(path.begin(), path.end());
	CreateDirectory(sPath.c_str(), NULL);
}
