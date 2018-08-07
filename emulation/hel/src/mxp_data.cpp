#include "mxp_data.hpp"
#include "util.hpp"
#include "json_util.hpp"
#include "error.hpp"

namespace hel{
	std::string as_string(MXPData::Config config){
		switch(config){
		case MXPData::Config::DI:
			return "DI";
		case MXPData::Config::DO:
			return "DO";
		case MXPData::Config::PWM:
			return "PWM";
		case MXPData::Config::SPI:
			return "SPI";
		case MXPData::Config::I2C:
			return "I2C";
		default:
			throw UnhandledEnumConstantException("MXPData::Config");
		}
	}

	MXPData::Config s_to_mxp_config(std::string s){
		switch(hasher(s.c_str())){
		case hasher("DI"):
			return MXPData::Config::DI;
		case hasher("DO"):
			return MXPData::Config::DO;
		case hasher("PWM"):
			return MXPData::Config::PWM;
		case hasher("SPI"):
			return MXPData::Config::SPI;
		case hasher("I2C"):
			return MXPData::Config::I2C;
		default:
			throw UnhandledCase();
		}
	}

	MXPData::MXPData()noexcept:config(MXPData::Config::DI),value(0.0){}

	MXPData::MXPData(const MXPData& source)noexcept{
#define COPY(NAME) NAME = source.NAME
		COPY(config);
		COPY(value);
#undef COPY
	}

	std::string MXPData::toString()const{
		std::string s = "(";
		s += "config:" + as_string(config) + ", ";
		s += "value:" + std::to_string(value) + ")";
		return s;
	}

	std::string MXPData::serialize()const{
		std::string s = "{";
		s += "\"config\":" + quote(as_string(config)) + ", ";
		s += "\"value\":" + std::to_string(value);
		s += "}";
		return s;
	}

	MXPData MXPData::deserialize(std::string s){
		MXPData m;
		m.config = s_to_mxp_config(unquote(pullObject("\"config\"",s)));
		m.value = std::stod(pullObject("\"value\"",s));
		return m;
	}
}
