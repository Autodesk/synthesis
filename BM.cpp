#include <iostream>
#include <fstream>

using namespace std;

// Create the macro so we don't repeat the code over and over again.
#define BMBINARY_READ(reader,value) reader.read((char *)&value, sizeof(value))

enum BMBinaryIOMode
{
	None = 0,
	Read,
	Write
};

class BMBinaryIO
{
	// the output file stream to write onto a file
	ofstream writer;
	// the input file stream to read from a file
	ifstream reader;
	// the filepath of the file we're working with
	string filePath;
	// the current active mode.
	BMBinaryIOMode currentMode;

public:
	BMBinaryIO()
	{
		currentMode = BMBinaryIOMode::None;
	}

	// the destructor will be responsible for checking if we forgot to close
	// the file
	~BMBinaryIO()
	{
		if (writer.is_open())
		{
			//BMLogging::error(BMLoggingClass::BinaryIO, "You forgot to call close() after finishing with the file! Closing it...");
			writer.close();
		}

		if (reader.is_open())
		{
			//BMLogging::error(BMLoggingClass::BinaryIO, "You forgot to call close() after finishing with the file! Closing it...");
			reader.close();
		}
	}

	// opens a file with either read or write mode. Returns whether
	// the open operation was successful
	bool open(string fileFullPath, BMBinaryIOMode mode)
	{
		filePath = fileFullPath;

		//BMLogging::info(BMLoggingClass::BinaryIO, "Opening file: " + filePath);

		// Write mode
		if (mode == BMBinaryIOMode::Write)
		{
			currentMode = mode;
			// check if we had a previously opened file to close it
			if (writer.is_open())
				writer.close();

			writer.open(filePath, ios::binary);
			if (!writer.is_open())
			{
				//BMLogging::error(BMLoggingClass::BinaryIO, "Could not open file for write: " + filePath);
				currentMode = BMBinaryIOMode::None;
			}
		}
		// Read mode
		else if (mode == BMBinaryIOMode::Read)
		{
			currentMode = mode;
			// check if we had a previously opened file to close it
			if (reader.is_open())
				reader.close();

			reader.open(filePath, ios::binary);
			if (!reader.is_open())
			{
				//BMLogging::error(BMLoggingClass::BinaryIO, "Could not open file for read: " + filePath);
				currentMode = BMBinaryIOMode::None;
			}
		}

		// if the mode is still the NONE/initial one -> we failed
		return currentMode == BMBinaryIOMode::None ? false : true;
	}

	// closes the file
	void close()
	{
		if (currentMode == BMBinaryIOMode::Write)
		{
			writer.close();
		}
		else if (currentMode == BMBinaryIOMode::Read)
		{
			reader.close();
		}
	}

	bool checkWritabilityStatus()
	{
		if (currentMode != BMBinaryIOMode::Write)
		{
			//BMLogging::error(BMLoggingClass::BinaryIO, "Trying to write with a non Writable mode!");
			return false;
		}
		return true;
	}

	// Generic write method that will write any value to a file (except a string,
	// for strings use writeString instead).
	void write(void *value, size_t size)
	{
		if (!checkWritabilityStatus())
			return;

		// write the value to the file.
		writer.write((const char *)value, size);
	}

	// Writes a string to the file
	void writeString(string str)
	{
		if (!checkWritabilityStatus())
			return;

		// first add a \0 at the end of the string so we can detect
		// the end of string when reading it
		str += '\0';

		// create char pointer from string.
		char* text = (char *)(str.c_str());
		// find the length of the string.
		unsigned long size = str.size();

		// write the whole string including the null.
		writer.write((const char *)text, size);
	}

	// helper to check if we're allowed to read
	bool checkReadabilityStatus()
	{
		if (currentMode != BMBinaryIOMode::Read)
		{
			//BMLogging::error(BMLoggingClass::BinaryIO, "Trying to read with a non Readable mode!");
			return false;
		}

		// check if we hit the end of the file.
		if (reader.eof())
		{
			//BMLogging::error(BMLoggingClass::BinaryIO, "Trying to read but reached the end of file!");
			reader.close();
			currentMode = BMBinaryIOMode::None;
			return false;
		}

		return true;
	}

	// reads a boolean value
	bool readBoolean()
	{
		if (checkReadabilityStatus())
		{
			bool value = false;
			BMBINARY_READ(reader, value);
			return value;
		}

		return false;
	}

	// reads a character value
	char readChar()
	{
		if (checkReadabilityStatus())
		{
			char value = 0;
			BMBINARY_READ(reader, value);
			return value;
		}
		return 0;
	}

	// read an integer value
	int readInt()
	{
		if (checkReadabilityStatus())
		{
			int value = 0;
			BMBINARY_READ(reader, value);
			return value;
		}
		return 0;
	}

	// read a float value
	float readFloat()
	{
		if (checkReadabilityStatus())
		{
			float value = 0;
			BMBINARY_READ(reader, value);
			return value;
		}
		return 0;
	}

	// read a double value
	double readDouble()
	{
		if (checkReadabilityStatus())
		{
			double value = 0;
			BMBINARY_READ(reader, value);
			return value;
		}
		return 0;
	}

	// read a string value
	string readString()
	{
		if (checkReadabilityStatus())
		{
			char c;
			string result = "";
			while ((c = readChar()) != '\0')
			{
				result += c;
			}
			return result;
		}
		return "";
	}
};