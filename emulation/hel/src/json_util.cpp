#include "json_util.hpp"

hel::JSONParsingException::JSONParsingException(std::string s):details(s){}

const char* hel::JSONParsingException::what()const throw(){
    std::string s = "Synthesis exception: JSON parsing failed due to malformed data in " + details;
    return s.c_str();
}

std::string hel::quote(const std::string& s){
    return "\"" + s + "\"";
}

std::string hel::unquote(std::string s){
    s = hel::trim(s);
    if(s.size() > 0 && s[0] == '\"'){
        s.erase(0,1);
    }
    if(s.size() > 0 && s[s.size() - 1] == '\"'){
        s.erase(s.size() - 1, 1);
    }
    return s;
}

std::string hel::removeExtraneousSpaces(std::string input_str){
    const std::string DOUBLE_SPACE = "  ";
    while(input_str.find(DOUBLE_SPACE) != std::string::npos){
        input_str.replace(input_str.find(DOUBLE_SPACE), DOUBLE_SPACE.size()," ");
    }
    return input_str;
}

std::string hel::excludeFromString(const std::string& input_str,const std::vector<char>& excluded_chars){
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

std::string hel::clipList(std::string input){
    input = trim(input);
    if(input.size() > 0 && input[0] == '['){
        input.erase(0,1);
    }
    if(input.size() > 0 && input[input.size() - 1] == ']'){
        input.erase(input.size() - 1, 1);
    }
    return input;
}

std::vector<std::string> hel::splitObject(std::string input){
    std::vector<std::string> v;
    std::size_t previous_input_size = input.size();
    while(input.size() > 0){
        v.push_back(hel::pullObject(input));
        if(input.size() == previous_input_size){
            throw JSONParsingException("hel::splitObject");
        }
        previous_input_size = input.size();
    }
    return v;
}

std::string hel::pullObject(std::string& input){
    unsigned end = 0;

    int bracket_count = 0;
    int curly_bracket_count = 0;

	char c;
    for(; end < input.size(); end++){
        c = input[end];
        if(c == '['){
            bracket_count++;
        } else if(c == ']'){
            bracket_count--;
        } else if(c == '{'){
            curly_bracket_count++;
        } else if(c == '}'){
            curly_bracket_count--;
        }
        if(bracket_count <= 0 && curly_bracket_count <= 0){
			if(c == ','){ //capture element of list
				break;
			} else if(c == '}' || c == ']'){
				if(bracket_count == 0 && curly_bracket_count == 0){ //if the brackets close at this character, then the object should begin and end with the bracket, so capture the bracket character too
					end++;
				}
				break;
			}
        }
    }

    std::string item = input.substr(0, end);

    input = input.substr(end);

    if(input.size() > 0 && input[0] == ','){ //When removing object from string, remove comma if necessary
        input.erase(0,1);
    }

    return trim(item);
}

std::string hel::pullValue(std::string label, std::string& input){ //returns the first item that matches label
    const std::string OBJECT_START_SYM = ":";
    label += OBJECT_START_SYM;

    std::size_t start = input.find(label);

    if(start == std::string::npos){ //check if label is present
        return "";
    }

    std::string search = input.substr(start + label.size()); //create string without data before label
    std::string value = hel::pullObject(search);

    if(value.size() > 0 && (value[value.size() - 1] == '}' || value[value.size() - 1] == ']')){ //remove closing bracket from value if it falls at the end of the object
        value.erase(value.size() - 1, 1);
    }

    input = input.substr(0, start) + search; //remove object from input
    return trim(value);
}
