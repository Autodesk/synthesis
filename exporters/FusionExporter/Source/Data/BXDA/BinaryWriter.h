#pragma once

#include <fstream>

namespace BXDA
{
	class BinaryWritable;

	/// Writes binary data to a file.
	class BinaryWriter
	{
	public:
		///
		/// Construct a BinaryWriter.
		/// \param file File to write to.
		///
		BinaryWriter(std::string file);
		~BinaryWriter(); ///< Closes the output file stream.

		inline void write(const bool & x) { writeInternal(x); } ///< Write a bool to the binary file.
		inline void write(const char & x) { writeInternal(x); } ///< Write a char to the binary file.
		inline void write(const int & x) { writeInternal(x); } ///< Write an int to the binary file.
		inline void write(const unsigned int & x) { writeInternal(x); } ///< Write an unsigned int to the binary file.
		inline void write(const float & x) { writeInternal(x); } ///< Write a float to the binary file.
		inline void write(const double & x) { writeInternal(x); } ///< Write a double to the binary file.
		void write(const BinaryWritable &); ///< Write a BinaryWritable to the binary file.
		void write(const std::string &); ///< Write a string to the binary file.

	private:
		std::ofstream outputFile; ///< The output file stream.

		///
		/// Used for writing the individual bytes of any datatype to the binary file.
		/// \param data Data to write
		///
		template<typename T> void writeInternal(const T &);

	};
	
	template<typename T>
	void BinaryWriter::writeInternal(const T & data)
	{
		if (!outputFile.is_open())
			return;

		// Generate null-terminated c-string, where each character is a byte from the data
		outputFile.write((char*)(&data), sizeof(T));
	}

	/// Class that can define a custom method of being written to a BinaryWriter.
	class BinaryWritable
	{
		friend BinaryWriter;

	private:
		///
		/// Writes the object to a binary file.
		/// \param output BinaryWriter that controls the output file.
		///
		virtual void write(BinaryWriter & output) const = 0;

	};
	
	inline void BinaryWriter::write(const BinaryWritable & bin) { bin.write(*this); }
}
