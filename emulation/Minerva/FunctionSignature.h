#ifndef FUNCTION_SIGNATURE_H
#define FUNCTION_SIGNATURE_H

#include <string>
#include <vector>
#include <exception>

namespace minerva{
	
	/**
	 * \brief The exception ParseException represents a generic parse failure
	 */
	struct ParseException: public std::exception{//TODO: this could probably be improved
		const char* what()const throw(){
			return "Parsing failed";
		}
	};
	
	/**
	 * \brief The class ParameterInfo represents information about a variable in string form
	 * 
	 * Stores its name and type
	 */
	struct ParameterInfo{
		std::string name;
		std::string type;
		
		/**
		 * \brief Constructs a FunctionSignature
		 */
		ParameterInfo();
		
		/**
		 * \brief Constructs a FunctionSignature
		 * 
		 * @param The name of the parameter
		 * @param The type of the parameter
		 */
		 ParameterInfo(std::string,std::string);
	};
	
	/**
	 * \brief The class FunctionSignature represents information about a function signature in string form
	 * 
	 * Stores its name, return type, and parameters
	 */
	struct FunctionSignature{
		//These constants are used to parse function signatures for the RoboRIO HAL which follow a consistent naming scheme
		static const std::string FUNC_START_INDICATOR;
		static const std::string FUNC_NAME_END_INDICATOR;
		static const std::string FUNC_END_INDICATOR;
		static const char FUNC_PARAMETER_DELIMITER = ',';

		std::string name;
		std::string return_type;
		
		std::vector<ParameterInfo> parameters;
		
		/**
		 * \brief Parses a standard string representing a function signature
		 *
		 * @param The standard string to parseFunctionSignature
		 * @return A FunctionSignature representing the parsed line
		 * @throw ParseException if it is unable to parse the string due to unexpected formatting
		 */
		static FunctionSignature parse(const std::string);
		
		/**
		 * \brief Constructs a FunctionSignature
		 */
		FunctionSignature();
	};

	/**
	 * \brief Parses a header file for its function signatures
	 *
	 * @param The name of the header file to parse
	 * @return A standard vector of FunctionSignatures representing the parsed functions in the header file
	 */
	std::vector<FunctionSignature> parseFunctionSignatures(const std::string);
	
}

std::ostream& operator<<(std::ostream&, const minerva::ParameterInfo);
std::ostream& operator<<(std::ostream&, const minerva::FunctionSignature);

#endif