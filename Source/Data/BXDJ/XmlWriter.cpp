#include "XmlWriter.h"

using namespace BXDJ;

XmlWriter::XmlWriter(std::string file, bool lightWeight)
{
	outputFile.open(file);
	this->lightWeight = lightWeight;
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
	elementStack.top()->innerXml += "<" + name + ">";
	elementStack.top()->innerXml += innerXml;
	elementStack.top()->innerXml += "</" + name + ">\n";
}

void XmlWriter::endElement()
{
	XmlElement * endingElement = elementStack.top();
	elementStack.pop();
	XmlElement * nextElement = NULL;

	if (!elementStack.empty())
		nextElement = elementStack.top();
	else
		nextElement = new XmlElement();

	// Print out opening bracket
	nextElement->innerXml += "<" + endingElement->name;
	if (endingElement->attributes.size() > 0)
		for (Attribute attr : endingElement->attributes)
			nextElement->innerXml += " " + attr.name + "=\"" + attr.value + "\"";
	nextElement->innerXml += ">\n";

	// Print out inner XML
	nextElement->innerXml += endingElement->innerXml;

	// Print out closing bracket
	nextElement->innerXml += "</" + endingElement->name + ">\n";

	// If the closed item was the last in the stack, print to file
	if (elementStack.empty())
	{
		if (outputFile.is_open())
			outputFile << nextElement->innerXml;

		delete nextElement;
	}

	// Clean up memory
	delete endingElement;
}