#include "Filesystem.h"
const std::string ROBOT_APPDATA_DIRECTORY = "Autodesk\\Synthesis\\Robots";

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
	boost::filesystem::create_directory(path);
}


bool Filesystem::directoryExists(std::string path)
{
	return boost::filesystem::is_directory(path);
}
