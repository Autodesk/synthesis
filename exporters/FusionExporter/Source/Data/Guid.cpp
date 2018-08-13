#include "Guid.h"

const char HEX_CHARS[] = { '0', '1', '2', '3', '4', '5', '6', '7',
						   '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

int Guid::nextSeed = 0;

Guid::Guid(unsigned int seed)
{
	if (seed == INT_MAX)
		seed = nextSeed++;

	regenerate(seed);
}

Guid::Guid(const Guid & guidToCopy)
{
	seed = guidToCopy.seed;

	for (int i = 0; i < BYTE_COUNT; i++)
	{
		bytes[i] = guidToCopy.bytes[i];
	}
}

void Guid::resetAutomaticSeed()
{
	nextSeed = 0;
}

void Guid::regenerate(unsigned int seed)
{
	srand(seed);

	this->seed = seed;

	for (int i = 0; i < BYTE_COUNT; i++)
	{
		bytes[i] = rand() % 0x100;
	}
}

std::string Guid::toString() const
{
	char str[BYTE_COUNT * 2 + 1];
	str[BYTE_COUNT * 2] = 0; // Null terminated c-string

	for (int i = 0; i < BYTE_COUNT; i++)
	{
		str[i * 2]     = HEX_CHARS[bytes[i] / 0x10];
		str[i * 2 + 1] = HEX_CHARS[bytes[i] % 0x10];
	}

	std::string fullString(str);

	return fullString.substr(0, 8) + '-' +
		   fullString.substr(8, 4) + '-' +
		   fullString.substr(12, 4) + '-' +
		   fullString.substr(16, 4) + '-' +
		   fullString.substr(20);
}

unsigned int Guid::getSeed() const
{
	return seed;
}
