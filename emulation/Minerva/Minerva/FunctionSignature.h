#ifndef FUNCTION_SIGNATURE_H
#define FUNCTION_SIGNATURE_H

#include <string>
#include <vector>
#include <exception>
#include <sstream>

namespace minerva{
	
	/**
	 * \brief The class FunctionSignature represents information about a function signature in string form
	 * 
	 * Stores its name, return type, and parameters
	 */
	struct FunctionSignature{
		/**
		 * \brief The exception ParseException represents a generic parse failure
		 */
		struct ParseException: public std::exception{//TODO: this could probably be improved
			const char* what()const throw(){
				return "Parsing failed";
			}
		};
		
		/**
		 * \brief The class ParameterNameInfo represents information about a variable declaration in string form
		 * 
		 * Stores its name and type
		 */
		struct ParameterNameInfo{
			std::string name;
			std::string type;
			
			/**
			 * \brief Constructs a ParameterNameInfo
			 */
			ParameterNameInfo();
			
			/**
			 * \brief Constructs a ParameterNameInfo
			 * 
			 * @param The name of the parameter
			 * @param The type of the parameter
			 */
			 ParameterNameInfo(std::string,std::string);
		};
		
		/**
		 * \brief The class ParameterValueInfo represents information about a variable's value in string form
		 * 
		 * Stores its type and its value in string form
		 */
		struct ParameterValueInfo{
			std::string type;
			std::string value;
			
			/**
			 * \brief Constructs a ParameterValueInfo
			 */
			ParameterValueInfo();
			
			/**
			 * \brief Constructs a ParameterValueInfo
			 * 
			 * @param The type of the parameter in string form
			 * @param The value of the parameter 
			 */
			template <typename T>
			ParameterValueInfo(std::string t,T v){
				type = t;
				
				if (std::is_same<decltype(v), void*>()){//prevent nullptr operator overloading error with stringstream
					value = "nullptr";
					return;
				} else if ((std::is_same<decltype(v), std::nullptr_t>())) {
					return;
				}	
				std::stringstream ss;
				ss<<v;
				value = ss.str();
			}
		};
		
		//These constants are used to parse function signatures for the RoboRIO HAL which follow a consistent naming scheme
		static const std::string FUNC_NAME_PREFIX;
		static const std::string FUNC_NAME_SUFFIX;
		static const std::string FUNC_SIGNATURE_END_INDICATOR;
		static const char FUNC_PARAMETER_DELIMITER = ',';

		std::string name;
		std::string return_type;
		
		std::vector<ParameterNameInfo> parameters;
		
		std::string toString()const;
		
		/**
		 * \brief Parses a standard string representing a function signature
		 *
		 * Assumes the function signature's return type and name are all on the same line,
		 * though its parameters may span multiple lines after that
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

	std::ostream& operator<<(std::ostream&, const minerva::FunctionSignature::ParameterNameInfo);
	bool operator<(const minerva::FunctionSignature::ParameterNameInfo,const minerva::FunctionSignature::ParameterNameInfo);
	std::ostream& operator<<(std::ostream&, const minerva::FunctionSignature::ParameterValueInfo);
	std::ostream& operator<<(std::ostream&, const minerva::FunctionSignature);
	bool operator<(const minerva::FunctionSignature,const minerva::FunctionSignature);

}

#endif