#pragma once

#include <fstream>
#include <string>
#include <stack>
#include <list>

namespace BXDJ
{
	class XmlWritable;

	class XmlWriter
	{
	public:
		XmlWriter(std::string file, bool lightWeight = true);
		~XmlWriter();

		void startElement(std::string);
		void writeAttribute(std::string, std::string);
		void writeElement(std::string, std::string);
		void endElement();
		void write(const XmlWritable &);

	private:
		struct Attribute
		{
			std::string name;
			std::string value;
		};

		struct XmlElement
		{
			std::string name;
			std::list<Attribute> attributes;
			std::string innerXml;
		};

		std::ofstream outputFile;
		bool lightWeight;
		std::stack<XmlElement *> elementStack;

		std::string indent(size_t indentation);

	};

	// Classes that inherit from XmlWritable can be fed into a XmlWriter
	class XmlWritable
	{
		friend XmlWriter;

	private:
		virtual void write(XmlWriter &) const = 0;

	};

	// Inline function for writing XmlWritables. Inlines must be defined in header, and after any classes they use
	inline void XmlWriter::write(const XmlWritable & xml) { xml.write(*this); }
}
