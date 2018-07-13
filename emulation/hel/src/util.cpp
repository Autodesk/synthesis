#include "util.h"

std::string hel::quote(std::string s){
    return "\"" + s + "\"";
}

std::string hel::removeExtraneousSpaces(std::string input_str){
	const std::string DOUBLE_SPACE = "  ";
	while(input_str.find(DOUBLE_SPACE) != std::string::npos){
        input_str.replace(input_str.find(DOUBLE_SPACE), DOUBLE_SPACE.size()," ");
	}
	return input_str;
}

std::string hel::excludeFromString(std::string input_str,std::vector<char> excluded_chars){
	std::string processed_str = "";
	for(char c: input_str){
		bool exclude = false;
		for(char excluded_char : excluded_chars){
			if(c == excluded_char){
				exclude = true;
				break;
			}
		}
		if(exclude){
			continue;
		}
		processed_str += c;
	}

	return processed_str;
}

std::string hel::trim(std::string input_str){
	if(input_str.find_first_of(' ') != std::string::npos){
		input_str.erase(0, input_str.find_first_not_of(' '));
	}
	if(input_str.find_last_not_of(' ') != std::string::npos){
		input_str.erase(input_str.find_last_not_of(' ') + 1);
	}
	return input_str;
}

std::vector<std::string> hel::split(std::string input_str, const char DELIMITER){
    std::vector<std::string> split_str;
    while(input_str.find(DELIMITER) != std::string::npos){
        std::string segment = input_str.substr(0, input_str.find(DELIMITER));

        split_str.push_back(trim(segment));
        input_str = input_str.substr(segment.size() + 1); //remove the segment added to the std::vector along with the delimiter
    }
    split_str.push_back(trim(input_str));  // catch the last segment

    return split_str;
}

std::string hel::removeList(std::string label, std::string& input){
    const std::string LIST_START_SYM = ":";
    label += LIST_START_SYM;
    std::size_t start = input.find(label);
    if(start == std::string::npos){ //check if label is present
        return "";
    }
    if(input.substr(start).find(label) != std::string::npos){ //check for multiple labels i.e. an error
        //TODO error handling -- more than one instance of label
    }
    std::string search = input.substr(start + label.size()); //create string representing
    unsigned end = 0; //marks end of list
    int count = 0;
    for(; end < search.size(); end++){
        if(search[end] == '['){
            count++;
        } else if(search[end] == ']'){
            count--;
        }
        if(count == 0){
            end++; //capture last bracket of list
            break;
        }
    }
    input = input.substr(0,start) + search.substr(end + 1); //get front of input, exclude found list and following comma, and get back
    return search.substr(0, end);
}
