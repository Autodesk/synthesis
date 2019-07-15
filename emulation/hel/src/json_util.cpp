#include "json_util.hpp"

#include "error.hpp"

#include <cassert>

namespace hel{
    JSONParsingException::JSONParsingException(std::string det):details(det){}

    const char* JSONParsingException::what()const throw(){
        return makeExceptionMessage("JSON parsing failed due to malformed data in " + details).c_str();
    }

    std::string quote(const std::string& s){
        return "\"" + s + "\"";
    }

    std::string unquote(std::string s){
        s = trim(s);
        if(!s.empty()){
            if(s[0] == '\"'){
                s.erase(0,1);
            }
            if(s[s.size() - 1] == '\"'){
                s.erase(s.size() - 1, 1);
            }
        }
        return s;
    }

    std::string removeExtraneousSpaces(std::string input_str){
        const std::string DOUBLE_SPACE = "  ";
        while(input_str.find(DOUBLE_SPACE) != std::string::npos){
            input_str.replace(input_str.find(DOUBLE_SPACE), DOUBLE_SPACE.size()," ");
        }
        return input_str;
    }

    std::string trim(std::string input_str){
        if(input_str.find_first_of(' ') != std::string::npos){
            input_str.erase(0, input_str.find_first_not_of(' '));
        }
        if(input_str.find_last_not_of(' ') != std::string::npos){
            input_str.erase(input_str.find_last_not_of(' ') + 1);
        }
        return input_str;
    }

    std::vector<std::string> split(std::string input_str, const char DELIMITER){
        std::vector<std::string> split_str;
        std::string segment = "";
        while(input_str.find(DELIMITER) != std::string::npos){
            segment = input_str.substr(0, input_str.find(DELIMITER));

            split_str.push_back(trim(segment));
            input_str = input_str.substr(segment.size() + 1); //remove the segment added to the std::vector along with the delimiter
        }
        split_str.push_back(trim(input_str));  // catch the last segment

        return split_str;
    }

    std::string clipList(std::string input){
        input = trim(input);
        if(!input.empty()){
            if(input[0] == '['){
                input.erase(0,1);
            }
            if(input[input.size() - 1] == ']'){
                input.erase(input.size() - 1, 1);
            }
        }
        return input;
    }

    std::vector<std::string> splitObject(std::string input){
        std::vector<std::string> v;
        std::size_t previous_input_size = input.size();
        while(input.size() > 0){
            v.push_back(pullObject(input));
            if(input.size() == previous_input_size){
                throw JSONParsingException("hel::splitObject()");
            }
            previous_input_size = input.size();
        }
        return v;
    }

    std::string pullObject(std::string& input, unsigned start){
        assert(start < input.size());
        unsigned end = start;
        int bracket_count = 0;
        int curly_bracket_count = 0;

        char c;
        for(; end < input.size(); end++){ //count brackets to find end of JSON object
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
                    if(bracket_count == 0 && curly_bracket_count == 0){ //if brackets close at this character, then the object string should begin and end with brackets, so capture the last bracket too
                        end++;
                    }
                    break;
                }
            }
        }

        std::string item = input.substr(start, end - start);
        input.erase(start, end - start);

        if(input[start] == ','){ //When removing object from string, remove remaining, extraneous comma if necessary
            input.erase(start,1);
        }

        return trim(item);
    }

    std::string pullObject(std::string label, std::string& input){ //returns the first item that matches label
        const std::string OBJECT_START_SYM = ":";
        label += OBJECT_START_SYM;

        std::size_t start = input.find(label);

        if(start == std::string::npos){ //check if label is present
            return "";
        }

        input.erase(start, label.size());
        return pullObject(input,start);
    }

    void indent(std::string& input, int bracket_count){
        for(int i = 0; i < bracket_count; i++){
            input += "\t";
        }
    }

    std::string formatJSON(std::string input){
        int bracket_count = 0;
        std::string out = "";

        for(unsigned i = 0; i < input.size(); i++){
            char c = input[i];
            if(c == '}' || c == ']'){
                bracket_count--;
                out += "\n";
                indent(out, bracket_count);
                out += c;
            } else if(c == '{' || c == '['){
                bracket_count++;
                out += c;
                out += "\n";
                indent(out, bracket_count);
            } else if(c == ','){
                out += c;
                out += "\n";
                indent(out, bracket_count);
                if((i + 1) < input.size() && input[i + 1] == ' '){ //skip space after indenting
                    i++;
                }
            } else {
                out += c;
            }
        }
        return out;
    }
}
