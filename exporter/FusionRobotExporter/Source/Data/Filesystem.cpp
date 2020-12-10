#include "Filesystem.h"
#include <windows.h>

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
	std::wstring sPath = std::wstring(path.begin(), path.end());
	CreateDirectory(sPath.c_str(), NULL);
}


bool Filesystem::directoryExists(std::string path)
{
	std::wstring sPath = std::wstring(path.begin(), path.end());

	DWORD ftyp = GetFileAttributes(sPath.c_str());
	if (ftyp == INVALID_FILE_ATTRIBUTES)
		return false;  //something is wrong with your path!

	if (ftyp & FILE_ATTRIBUTE_DIRECTORY)
		return true;   // this is a directory!

	return false;    // this is not a directory!
}

bool Filesystem::fileExists(std::string path)
{
	std::wstring sPath = std::wstring(path.begin(), path.end());

	DWORD ftyp = GetFileAttributes(sPath.c_str());
	if (ftyp == INVALID_FILE_ATTRIBUTES)
		return false;  //something is wrong with your path!

	if (ftyp & FILE_ATTRIBUTE_DIRECTORY)
		return false;   // this is a directory!

	return true;    // this is not a directory!
}