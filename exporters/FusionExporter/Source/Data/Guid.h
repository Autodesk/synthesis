#pragma once

#include <stdlib.h>
#include <stdio.h>
#include <string>

/// Globally Unique Identifier: Used to identify Meshes and RigidNodes in the written files.
class Guid
{
public:
	///
	/// Generate a GUID.
	/// \param seed The seed used to generate the random GUID. If equal to INT_MAX, uses the next automatic seed value.
	///
	Guid(unsigned int seed = INT_MAX);
	/// Copy constructor.
	Guid(const Guid &);
	
	static void resetAutomaticSeed(); ///< Resets the next automatic seed value to 0.
	
	void regenerate(unsigned int seed); ///< Regenerates the GUID with the given seed.
	
	std::string toString() const; ///< \return The stringified version of the GUID using hexadecimal characters.
	unsigned int getSeed() const; ///< \return The seed used to generate the GUID.

private:
	static const int BYTE_COUNT = 16; ///< The number of bytes in a GUID.

	static int nextSeed; ///< The next automatic seed value.

	unsigned char bytes[BYTE_COUNT]; ///< The bytes of the GUID.
	unsigned int seed; ///< The seed used to generate the GUID.

};
