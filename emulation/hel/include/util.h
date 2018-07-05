#ifndef _UTIL_H_
#define _UTIL_H_

namespace hel{

	template<typename T>
	unsigned findMostSignificantBit(T value){
		unsigned most_significant_bit = 0;
		
		while(value != 0){
			value /= 2;
			most_significant_bit++;
		}
		
		return most_significant_bit;
	}

	template<typename T>
	bool checkBitHigh(T value,unsigned index){
		index = 1u << index; 
		return value & index;
	}
}

#endif
