#include "CustomJSONObject.h"
#include <rapidjson/stringbuffer.h>
#include <rapidjson/writer.h>

std::string BXDJ::CustomJSONObject::toJSONString() const
{
	rapidjson::Document doc;

	doc.CopyFrom(getJSONObject(doc.GetAllocator()), doc.GetAllocator());
	rapidjson::StringBuffer buffer;
	rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
	doc.Accept(writer);
	return std::string(buffer.GetString());
}

void BXDJ::CustomJSONObject::fromJSONString(std::string jsonStr)
{
	rapidjson::Document doc;
	doc.Parse(jsonStr.c_str());
	rapidjson::Value val;
	val.CopyFrom(doc, doc.GetAllocator());
	loadJSONObject(val);
}
