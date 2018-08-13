#include "XmlWriter.h"

using namespace BXDJ;

XmlWriter::XmlWriter(std::string file, bool lightWeight)
{
	outputFile.open(file);
	this->lightWeight = lightWeight;

	if (outputFile.is_open())
		outputFile << "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
}

XmlWriter::~XmlWriter()
{
	while (!elementStack.empty())
		endElement();

	if (outputFile.is_open())
		outputFile.close();
}

void XmlWriter::startElement(std::string elementName)
{
	elementStack.push(new XmlElement);
	elementStack.top()->name = elementName;
}

void XmlWriter::writeAttribute(std::string name, std::string value)
{
	Attribute attr = {name, value};
	elementStack.top()->attributes.push_back(attr);
}

void XmlWriter::writeElement(std::string name, std::string innerXml)
{
	XmlElement * currentElement = NULL;

	// Determine whether writing to file or parent element
	if (!elementStack.empty())
		currentElement = elementStack.top();
	else
		currentElement = new XmlElement();

	// Write opening, content, and closing on same line
	currentElement->innerXml += indent(elementStack.size()) + "<" + name + ">";
	currentElement->innerXml += innerXml;
	currentElement->innerXml += "</" + name + ">";

	if (elementStack.empty())
	{
		if (outputFile.is_open())
			outputFile << currentElement->innerXml;

		delete currentElement;
	}
}

void XmlWriter::endElement()
{
	// Make sure there is an element to end.
	if (elementStack.empty())
		return;

	XmlElement * endingElement = elementStack.top();
	elementStack.pop();
	XmlElement * currentElement = NULL;

	if (!elementStack.empty())
		currentElement = elementStack.top();
	else
		currentElement = new XmlElement();

	// Print out opening bracket
	currentElement->innerXml += indent(elementStack.size()) + "<" + endingElement->name;

	if (endingElement->attributes.size() > 0)
		for (Attribute attr : endingElement->attributes)
			currentElement->innerXml += " " + attr.name + "=\"" + attr.value + "\"";

	currentElement->innerXml += ">";

	// Print out inner XML
	currentElement->innerXml += endingElement->innerXml;

	// Print out closing bracket
	currentElement->innerXml += indent(elementStack.size()) + "</" + endingElement->name + ">";

	// If the closed item was the last in the stack, print to file
	if (elementStack.empty())
	{
		if (outputFile.is_open())
			outputFile << currentElement->innerXml;

		delete currentElement;
	}

	// Clean up memory
	delete endingElement;
}

std::string XmlWriter::indent(size_t indentation)
{
	if (!lightWeight)
		return '\n' + std::string(indentation * 2, ' ');
	else
		return "";
}
