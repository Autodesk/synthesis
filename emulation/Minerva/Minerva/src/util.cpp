#include "util.h"

using namespace std;

string minerva::excludeFromString(string input_str, vector<char> excluded_chars){
	string processed_str = "";
	for(char c: input_str){
		bool exclude = false;
		for(char excluded_char: excluded_chars){
			if(c == excluded_char){
				exclude = true;
				break;
			}
		}
		if(exclude){
			continue;
		}
		if(c == '\n' || c == '\r' || c == '\t'){
			continue;
		}
		processed_str += c;
	}
	
	return processed_str;
}

string minerva::removeExtraneousSpaces(string input_str){
	const string DOUBLE_SPACE = "  ";
	while(input_str.find(DOUBLE_SPACE) != string::npos){
		input_str.replace(input_str.find(DOUBLE_SPACE), DOUBLE_SPACE.size(), " ");
	}
	return input_str;
}

string minerva::trim(string input_str){
	if(input_str.find_first_of(' ') != string::npos){
		input_str.erase(0,input_str.find_first_not_of(' '));	
	}
	if(input_str.find_last_not_of(' ') != string::npos){
		input_str.erase(input_str.find_last_not_of(' ') + 1);
	}
	return input_str;
}

vector<string> minerva::split(string input_str, const char DELIMITER){
	vector<string> split_str;
	while(input_str.find(DELIMITER) != string::npos){
		string segment = input_str.substr(0,input_str.find(DELIMITER));
		
		split_str.push_back(trim(segment));
		input_str = input_str.substr(segment.size() + 1);//remove the segment added to the vector along with the delimiter
	}
	split_str.push_back(trim(input_str));//catch the last segment
	
	return split_str;
}
