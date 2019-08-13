#pragma once

#include <string>
#include <boost/filesystem.hpp>

/// Contains directory-related functions.
namespace Filesystem
{
	///
	/// Gets the directory that robot files will be saved to.
	/// \param robotName The name of the robot being saved.
	///
	std::string getCurrentRobotDirectory(std::string robotName);

	///
	/// Creates a directory.
	/// \param path The path of the directory to create.
	///
	void createDirectory(std::string path);

	///
	/// Checks if a folder exists
	/// \param path The path of the directory to locate
	///

	bool directoryExists(std::string path);
};
