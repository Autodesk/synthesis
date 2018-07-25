#pragma once

#include <stdlib.h>
#include <stdio.h>
#include <string>

class Guid
{
public:
	Guid(unsigned int seed = INT_MAX);
	Guid(const Guid &);
	
	static void resetAutomaticSeed();
	
	void regenerate(unsigned int seed);
	
	std::string toString() const;
	bool isInitialized() const;
	unsigned int getSeed() const;

private:
	static const int BYTE_COUNT = 16;

	static int nextSeed;

	unsigned char bytes[BYTE_COUNT];
	bool init;
	unsigned int seed;

};
