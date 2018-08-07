#include "util.hpp"

namespace hel{
	bool stob(std::string a){
		try{
			return (bool)std::stoi(a);
		} catch(...){
			throw;
		}
	}

	std::string as_string(bool a){
		return a ? "1" : "0";
	}

	void copystr(const std::string& str, char* c_arr){
		str.copy(c_arr, sizeof(c_arr));
		std::size_t terminator_index = std::max(std::min((signed int)(str.size()) - 1, (signed int)(sizeof(c_arr)) - 1), 0);
		c_arr[terminator_index] = '\0';
	}
}
