#pragma once

#include <fstream>
#include <string>
#include <stack>
#include <list>

namespace BXDJ
{
	class XmlWritable;

	/// Writes XmlWritables and other data to an XML file
	class XmlWriter
	{
	public:
		///
		/// Constructs an XmlWriter that writes to a given file.
		/// \param file The file to write XML to.
		/// \param lightWeight True to exclude all newlines and whitespace.
		///
		XmlWriter(std::string file, bool lightWeight = true);
		~XmlWriter(); ///< Ends any unfinished elements, writing them to the file, and closes the file output stream.

		///
		/// Begins a new XML element.
		/// \param elementName The name of the element to create.
		///
		void startElement(std::string);

		///
		/// Adds an attribute to the currently open element.
		/// \param name Name of the attribute.
		/// \param value Value of the attribute.
		///
		void writeAttribute(std::string, std::string);

		///
		/// Opens and closes a one-line element.
		/// \param name Name of the element.
		/// \param innerXml Text to place inside the element.
		///
		void writeElement(std::string, std::string);

		///
		/// Ends the currently open element. If no elements are open, does nothing.
		/// If the currently open element is at the bottom of the stack, writes the element and its innerXML to the file.
		///
		void endElement();

		///
		/// Writes an XmlWritable object to the file.
		/// \param xml Object to call the XmlWritable::write function of.
		///
		void write(const XmlWritable &);

	private:
		/// An attribute of an XML element.
		struct Attribute
		{
			std::string name; ///< Name of the attribute.
			std::string value; ///< Value of the attribute.
		};

		/// An element in an XML document.
		struct XmlElement
		{
			std::string name; ///< Name of the element.
			std::list<Attribute> attributes; ///< Any attributes attached to the element.
			std::string innerXml; ///< Contents of the element.
		};

		std::ofstream outputFile; ///< File stream for writing XML.
		bool lightWeight; ///< Whether or not to include whitespace.
		std::stack<XmlElement *> elementStack; ///< Stack of all open elements.

		///
		/// Manages whitespace in the XML document. If lightWeight is set to false, generates indentation with the specified depth.
		/// \param indentation The depth of the indentation to generate.
		/// \return If lightWeight is false, a newline followed by a series of spaces with a length twice the value of indentation.
		///         If lightweight is true, an empty string.
		///
		std::string indent(size_t indentation);

	};

	/// An object that can be fed into an XmlWriter.
	class XmlWritable
	{
		friend XmlWriter;

	private:
		///
		/// Writes the object to an XML file.
		/// \param output XmlWriter that controls the output file.
		///
		virtual void write(XmlWriter & output) const = 0;

	};

	inline void XmlWriter::write(const XmlWritable & xml) { xml.write(*this); }
}
