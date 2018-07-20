#pragma once

#include <fstream>

namespace BXDA
{
	class BinaryWritable;

	class BinaryWriter
	{
	public:
		BinaryWriter(std::string file);
		~BinaryWriter();

		inline void write(const bool & x) { writeInternal(x); }
		inline void write(const char & x) { writeInternal(x); }
		inline void write(const int & x) { writeInternal(x); }
		inline void write(const unsigned int & x) { writeInternal(x); }
		inline void write(const float & x) { writeInternal(x); }
		inline void write(const double & x) { writeInternal(x); }
		void write(const BinaryWritable &);
		void write(const std::string &); // Special function for writing strings

	private:
		std::ofstream outputFile;

		template<typename T> void writeInternal(const T &);

	};

	// Generic function used for outputing any datatype as bytes
	template<typename T>
	void BinaryWriter::writeInternal(const T & data)
	{
		if (!outputFile.is_open())
			return;

		char* bytes = (char*)(&data);
		for (int i = 0; i < sizeof(data); i++)
			outputFile << bytes[i];
	}

	// Classes that inherit from BinaryWritable can be fed into a BinaryWriter
	class BinaryWritable
	{
		friend BinaryWriter;

	private:
		virtual void write(BinaryWriter &) const = 0;

	};
	
	// Inline function for writing BinaryWritables. Inlines must be defined in header, and after any classes they use
	inline void BinaryWriter::write(const BinaryWritable & bin) { bin.write(*this); }
}
