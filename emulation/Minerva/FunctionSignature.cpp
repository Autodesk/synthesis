#include "FunctionSignature.h"

#include <fstream>
#include <iostream>
#include "Util.h"

using namespace std;

minerva::ParameterInfo::ParameterInfo(string n, string t):name(n),type(t){}
minerva::ParameterInfo::ParameterInfo():ParameterInfo("",""){}

ostream& operator<<(ostream& o, const minerva::ParameterInfo SIMPLE_TYPE_INFO){
	o<<"name: \""<<SIMPLE_TYPE_INFO.name<<"\"   type:\""<<SIMPLE_TYPE_INFO.type<<"\"";
	return o;
}

//Function signatures in the RoboRIO HAL follow this naming convention
const string minerva::FunctionSignature::FUNC_START_INDICATOR = " HAL_";
const string minerva::FunctionSignature::FUNC_NAME_END_INDICATOR = "(";
const string minerva::FunctionSignature::FUNC_END_INDICATOR = ");";

minerva::FunctionSignature::FunctionSignature():name(""),return_type(""),parameters(){}

ostream& operator<<(ostream& o, minerva::FunctionSignature FUNCTION_SIGNATURE){
	o<<"[\n";
	o<<"\tname: \""<<FUNCTION_SIGNATURE.name<<"\"\n";
	o<<"\treturn_type: \""<<FUNCTION_SIGNATURE.return_type<<"\"\n";
	o<<"\tparameters: \n";
	for(minerva::ParameterInfo parameter: FUNCTION_SIGNATURE.parameters){
		o<<"\t\t"<<parameter<<"\n";
	}
	o<<"]\n";
	return o;
}

minerva::FunctionSignature minerva::FunctionSignature::parse(const string RAW_FUNCTION_SIGNATURE){
	minerva::FunctionSignature function_signature;
	
	if(RAW_FUNCTION_SIGNATURE.find(minerva::FunctionSignature::FUNC_START_INDICATOR) != string::npos){
		function_signature.return_type = RAW_FUNCTION_SIGNATURE.substr(0,RAW_FUNCTION_SIGNATURE.find(minerva::FunctionSignature::FUNC_START_INDICATOR));//return type is everything before the name of the function
		
		if(RAW_FUNCTION_SIGNATURE.find(minerva::FunctionSignature::FUNC_NAME_END_INDICATOR) != string::npos){
			unsigned name_start_os = function_signature.return_type.size() + 1;//add one to start position since search includes the space between return type and function name
			function_signature.name = RAW_FUNCTION_SIGNATURE.substr(name_start_os,RAW_FUNCTION_SIGNATURE.find(minerva::FunctionSignature::FUNC_NAME_END_INDICATOR) - name_start_os);//name of the function is everything between the return type and the first open parenthesis
			{
				string raw_parameters = RAW_FUNCTION_SIGNATURE.substr(RAW_FUNCTION_SIGNATURE.find(function_signature.name) + function_signature.name.size());//clip off the return type and name of the function
				raw_parameters = raw_parameters.substr(1);//remove initial parenthesis 
				raw_parameters = raw_parameters.substr(0,raw_parameters.size() - minerva::FunctionSignature::FUNC_END_INDICATOR.size());//remove final parenthesis

				if(trim(raw_parameters) != "" && trim(raw_parameters).find_last_of(' ') != string::npos){//skip if function takes no parameters
					vector<string> splitParameters = split(raw_parameters,minerva::FunctionSignature::FUNC_PARAMETER_DELIMITER);
					
					for(string raw_parameter: splitParameters){
						string paramName = minerva::trim(raw_parameter.substr(raw_parameter.find_last_of(' ')));//separated by commas, the parameter name is everything after the last space
						string paramType = minerva::trim(raw_parameter.substr(0,raw_parameter.find_last_of(' ')));//the type of the parameter is everything else
						function_signature.parameters.push_back({paramName,paramType});
					}
				}
			}
		} else {
			throw minerva::ParseException();
		}
	} else {
		throw minerva::ParseException();
	}

	return function_signature;
}

vector<string> captureRawFunctionSignatures(const string INPUT_FILE_NAME){
	ifstream input_source(INPUT_FILE_NAME);
	
	if(!input_source.good()){
		throw ifstream::failure("ifstream failed to open \"" + INPUT_FILE_NAME + "\"");
	}
	
	vector<string> raw_function_signatures;
	string function_signature_buffer = "";
	bool capture_new = false;
	
	while(!input_source.eof()){
		string chars_to_add = "";
		getline(input_source,chars_to_add);		
		chars_to_add = minerva::excludeFromString(chars_to_add,{'\n', '\r', '\t'});
		chars_to_add = minerva::removeExtraneousSpaces(chars_to_add);
		
		if(!capture_new){
			if(function_signature_buffer != ""){//once finished capturing function signature, append it and reset the buffer
				raw_function_signatures.push_back(function_signature_buffer);
				function_signature_buffer = "";
			}
			
			if(chars_to_add.find(minerva::FunctionSignature::FUNC_START_INDICATOR) != string::npos && chars_to_add.find(minerva::FunctionSignature::FUNC_NAME_END_INDICATOR) != string::npos){//use FUNC_NAME_END_INDICATOR to confirm that it's a function rather than an enum or whatever (assuming the whole line containing the start of the function consists of just parts of that function signature)
				capture_new = true;
			}
		}
		
		if(capture_new){
			if(chars_to_add.find(minerva::FunctionSignature::FUNC_END_INDICATOR) != string::npos){//stop line capture when function finishes (assuming another function doesn't start on the same line)
				chars_to_add = chars_to_add.substr(0,chars_to_add.find(minerva::FunctionSignature::FUNC_END_INDICATOR) + minerva::FunctionSignature::FUNC_END_INDICATOR.size());
				capture_new = false;
			} 
			
			function_signature_buffer += chars_to_add;
		}
	}
	
	input_source.close();
	return raw_function_signatures;
}

vector<minerva::FunctionSignature> minerva::parseFunctionSignatures(const string INPUT_FILE_NAME){
	vector<string> raw_function_signatures;
	
	try{
		raw_function_signatures = captureRawFunctionSignatures(INPUT_FILE_NAME);
	} catch(exception& e){
		cout<<e.what()<<"\n";//TODO: decide what to do
	}
	vector<minerva::FunctionSignature> function_signatures;
	
	for(string raw_function_signature: raw_function_signatures){
		minerva::FunctionSignature function_signature;
		try{
			function_signatures.push_back(minerva::FunctionSignature::parse(raw_function_signature));
		} catch(exception& e){
			cout<<e.what()<<"\n";//TODO: decide what to do
		}
	}
	
	return function_signatures;
}

#ifdef FUNCTION_SIGNATURE_TEST

int main(){
	//string test_source = "Accelerometer.h";
	string test_source = "PWM.h";
	
	cout<<"Testing with target "<<test_source<<"\n\n=============================\n\n";
	
	try{
		for(string s: captureRawFunctionSignatures(test_source)){
			cout<<"Captured raw function signature: "<<s<<"\n";
		}
	} catch(exception& e){
		cout<<e.what()<<"\n";
	}
	
	cout<<"\n=============================\n\n";
	
	for(minerva::FunctionSignature function_signature: minerva::parseFunctionSignatures(test_source)){
		cout<<function_signature;
	}
}

#endif