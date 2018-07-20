#pragma once

#include <string>
#include "BinaryWriter.h"

namespace BXDA
{
	class Vector3 : public BinaryWritable
	{
	public:
		double x;
		double y;
		double z;
		
		Vector3();
		Vector3(const Vector3 &);
		Vector3(double x, double y, double z);

		Vector3 operator+(const Vector3 &) const;
		Vector3 operator*(double) const;
		Vector3 operator/(double) const;

		std::string toString();

	protected:
		void write(BinaryWriter &) const;

	};
}