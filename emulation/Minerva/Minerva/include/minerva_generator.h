#ifndef MINERVA_GENERATOR
#define MINERVA_GENERATOR

#include <array>

#include "function_signature.h"

namespace minerva{

	/**
	 * \brief The class MinervaGenerator houses the functions needed to generate the RoboRIO HAL interface cpp
	 */
	struct MinervaGenerator{
		static const unsigned HAL_HEADER_COUNT = 31;
		static const std::array<std::string,HAL_HEADER_COUNT> HAL_HEADER_NAMES;
		
		static const std::string MINERVA_FILE_NAME;
		static const std::string MINERVA_FILE_PREFIX;
		static const std::string MINERVA_FILE_SUFFIX;
		
		/**
		 * \brief Parses the RoboRIO HAL header files for its function signatures
		 *
		 * @param The relative path to the RoboRIO HAL header files in linux format and following slash
		 * @return A standard vector of FunctionSignatures representing the parsed functions
		 */
		static std::vector<FunctionSignature> parseHALFunctionSignatures(const std::string);
		
		/**
		 * \brief Generates a cpp file to interface with RoboRIO HAL function signatures for emulation 
		 *
		 * @param The relative path to the RoboRIO HAL header files in linux format and following slash
		 */
		static void generateMinerva(const std::string);
	};
	
}

#endif