#include "Analytics.h"
#include <nlohmann/json.hpp>

using namespace std;
using namespace web;
using namespace web::http;
using namespace web::http::client;

using namespace adsk::core;
using namespace SynthesisAddIn;

utility::string_t toUrlString(const utility::string_t in)
{
	return web::uri::encode_uri(in);
}

utility::string_t removeSpaces(utility::string_t in)
{
	in.erase(remove_if(in.begin(), in.end(), isspace), in.end());
	return in;
}

std::string Analytics::clientId;
bool Analytics::enabled = true;
bool Analytics::firstLaunchNotification = true;
bool Analytics::guideEnabled = true;

void Analytics::LoadSettings()
{
	std::string jsonId = "";
	try {
		std::ifstream t("SynthesisAddInSettings.json"); // TODO: settings manager class
		std::string jsonStr((std::istreambuf_iterator<char>(t)),
			std::istreambuf_iterator<char>());
		jsonId = jsonStr;
	} catch (const std::exception& e) {}

	nlohmann::json jsonObj;
	jsonObj = nlohmann::json::parse(jsonId, nullptr, false);

	if (jsonObj.contains("AnalyticsID") && jsonObj["AnalyticsID"].is_string() && jsonObj["AnalyticsID"].get<std::string>().length() == 36)
		clientId = jsonObj["AnalyticsID"].get<std::string>();
	else
		clientId = generate_guid();

	if (jsonObj.contains("AnalyticsEnabled") && jsonObj["AnalyticsEnabled"].is_boolean())
		enabled = jsonObj["AnalyticsEnabled"].get<bool>();
	else
		enabled = true;

	if (jsonObj.contains("GuideEnabled") && jsonObj["GuideEnabled"].is_boolean()) // TODO: Move this to settings manager class
		guideEnabled = jsonObj["GuideEnabled"].get<bool>();
	else
		guideEnabled = true;

	if (jsonObj.contains("FirstLaunchNotificationEnabled") && jsonObj["FirstLaunchNotificationEnabled"].is_boolean()) // TODO: Also move this to settings manager class
		firstLaunchNotification = jsonObj["FirstLaunchNotificationEnabled"].get<bool>();
	else
		firstLaunchNotification = true;

	SaveSettings();
}

void Analytics::SaveSettings()
{
	std::string filenameBXDJ = "SynthesisAddInSettings.json"; // TODO: Settings manager class
	nlohmann::json baseJson;
	baseJson["AnalyticsID"] = clientId;
	baseJson["AnalyticsEnabled"] = enabled;
	baseJson["GuideEnabled"] = guideEnabled;
	baseJson["FirstLaunchNotificationEnabled"] = firstLaunchNotification;
	std::ofstream writeStream(filenameBXDJ);
	std::string jsonStr = baseJson.dump(1);
	writeStream << jsonStr << std::endl;
	writeStream.close();
}

void Analytics::Post(utility::string_t queryString) {
	if (!enabled) return;
	http_client client(U("https://www.google-analytics.com/"));
	http_response response = client.request(methods::GET, queryString).get();
}

uri_builder Analytics::GetBaseURL()
{
	if (clientId.length() != 36) LoadSettings();
	auto url = uri_builder(U("collect"));
	url.append_query(U("v"), U("1"));
	url.append_query(U("tid"), U("UA-81892961-5"));
	url.append_query(U("cid"), std::wstring(clientId.begin(), clientId.end())); // TODO: string type conversion
	return url;
}

void Analytics::AppendEvent(uri_builder* url, const utility::string_t ec, const utility::string_t ea, const utility::string_t el)
{
	url->append_query(U("t"), "event");
	url->append_query(U("ec"), ec);
	url->append_query(U("ea"), ea);
	if (!el.empty()) url->append_query(U("el"), el);
}

void Analytics::AppendEvent(uri_builder* url, const utility::string_t ec, const utility::string_t ea)
{
	AppendEvent(url, ec, ea, U(""));
}

void Analytics::StartSession(Ptr<Application> app)
{
	LoadSettings();
	auto url = GetBaseURL();
	url.append_query(U("sc"), U("start"));
	AppendEvent(&url, U("Environment"), U("Opened"));
	Post(url.to_string());
}

void Analytics::EndSession()
{
	auto url = GetBaseURL();
	url.append_query(U("sc"), U("end"));
	AppendEvent(&url, U("Environment"), U("Closed"));
	Post(url.to_string());
	LoadSettings();
}

void Analytics::LogPage(const utility::string_t page)
{
	LogPage(U(""), page);
}

void Analytics::LogPage(utility::string_t baseUrl, utility::string_t page)
{
	auto url = GetBaseURL();
	url.append_query(U("t"), U("pageview"));
	url.append_query(U("dh"), U("inventor"));
	url.append_query(U("dp"), removeSpaces(baseUrl) + U("/") + removeSpaces(page));
	url.append_query(U("dt"), page);
	Post(url.to_string());
}

void Analytics::LogEvent(const utility::string_t ec, const utility::string_t ea)
{
	auto url = GetBaseURL();
	AppendEvent(&url, ec, ea);
	Post(url.to_string());
}

void Analytics::LogEvent(const utility::string_t ec, const utility::string_t ea, const utility::string_t el)
{
	auto url = GetBaseURL();
	AppendEvent(&url, ec, ea, el);
	Post(url.to_string());
}

void Analytics::SetEnabled(bool enabled)
{
	if (enabled == Analytics::enabled) return;
	LoadSettings();
	Analytics::enabled = enabled;
	SaveSettings();
}
