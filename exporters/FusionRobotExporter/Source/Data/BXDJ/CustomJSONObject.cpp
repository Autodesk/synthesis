#include "CustomJSONObject.h"
#include <rapidjson/stringbuffer.h>
#include <rapidjson/writer.h>

std::string BXDJ::CustomJSONObject::toJSONString() const
{
	nlohmann::json jsonObj;
	jsonObj = getJSONObject();
	return std::string(jsonObj.dump());
}

void BXDJ::CustomJSONObject::fromJSONString(std::string jsonStr)
{
	nlohmann::json jsonObj;
	jsonObj = nlohmann::json::parse(jsonStr);
	loadJSONObject(jsonObj);
}
