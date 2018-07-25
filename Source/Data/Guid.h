#pragma once

#include <stdlib.h>
#include <stdio.h>
#include <string>

class Guid
{
public:
	Guid();
	Guid(const Guid &);
	Guid(unsigned int seed);
	void regenerate(unsigned int seed);
	std::string toString() const;
	bool isInitialized() const;

private:
	static const int BYTE_COUNT = 16;
	unsigned char bytes[BYTE_COUNT];
	bool init;

};
