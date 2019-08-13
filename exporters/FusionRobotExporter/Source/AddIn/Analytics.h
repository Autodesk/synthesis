#pragma once

#include <cpprest/http_client.h>
#include <cpprest/details/basic_types.h>
#include <core/CoreAll.h>
#include <sstream>
#include <random>
#include <string>

namespace SynthesisAddIn {
	class Analytics
	{
		static bool enabled;
		static std::string clientId;
		static void LoadSettings();
		static void SaveSettings();
		static void Post(utility::string_t queryString);
		static web::uri_builder GetBaseURL();
		static void AppendEvent(web::uri_builder* url, utility::string_t ec, utility::string_t ea, utility::string_t el);
		static void AppendEvent(web::uri_builder* url, utility::string_t ec, utility::string_t ea);

		// https://lowrey.me/guid-generation-in-c-11/
		static unsigned int random_char() {
			std::random_device rd;
			std::mt19937 gen(rd());
			std::uniform_int_distribution<> dis(0, 255);
			return dis(gen);
		}
		static std::string generate_hex(const unsigned int len) {
			std::stringstream ss;
			for (auto i = 0; i < len; i++) {
				const auto rc = random_char();
				std::stringstream hexstream;
				hexstream << std::hex << rc;
				auto hex = hexstream.str();
				ss << (hex.length() < 2 ? '0' + hex : hex);
			}
			return ss.str();
		}
		static std::string generate_guid()
		{
			return generate_hex(4) + "-" + generate_hex(2) + "-" + generate_hex(2) + "-" + generate_hex(2) + "-" + generate_hex(6);
		}
	public:
		static void StartSession(adsk::core::Ptr<adsk::core::Application> app);
		static void EndSession();
		static void LogPage(utility::string_t page);
		static void LogPage(utility::string_t baseUrl, utility::string_t page);
		static void LogEvent(utility::string_t ec, utility::string_t ea);
		static void LogEvent(utility::string_t ec, utility::string_t ea, utility::string_t el);
		static void SetEnabled(bool enabled);
	};
}