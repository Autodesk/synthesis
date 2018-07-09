#ifndef _UTIL_H_
#define _UTIL_H_

namespace hel{

    /**
     * \fn unsigned findMostSignificantBit(T value)
     * \brief Finds the most significant bit in an integer
     * Finds the index of the high bit of the greatest value; returns zero if zero is passed in
     * \tparam T the type of integer
     * \param value the integer to analyze
     * \return the zero-indexed index of the most significant bit
     */
	
    template<typename T>
	unsigned findMostSignificantBit(T value){
		unsigned most_significant_bit = 0;
		
		while(value != 0){
			value /= 2;
			most_significant_bit++;
		}
		
		return most_significant_bit;
	}

    /**
     * \fn bool checkBitHigh(T value, unsigned index)
     * \brief Check if the bit in place \b index of integer \b value is high
     * \tparam T the type of integer
     * \param value the integer to analyze
     * \param index the zero-indexed index to check
     * \return true if the bit is high
     */

	template<typename T>
	bool checkBitHigh(T value,unsigned index){
		index = 1u << index; 
		return value & index;
	}
}

#endif
