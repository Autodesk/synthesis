#pragma once

#include <string>
#include "BXDA/BinaryWriter.h"

template<typename T = double>
class Vector3 : public BXDA::BinaryWritable
{
public:
	T x;
	T y;
	T z;
		
	Vector3();
	Vector3(const Vector3<T> &);
	Vector3(T x, T y, T z);

	Vector3<T> operator+(const Vector3 &) const;
	template<typename S> Vector3<T> operator*(S) const;
	template<typename S> Vector3<T> operator/(S) const;

	std::string toString();

protected:
	void write(BXDA::BinaryWriter &) const;

};

template<typename T = double>
Vector3<T>::Vector3() : Vector3(0, 0, 0)
{}

template<typename T = double>
Vector3<T>::Vector3(const Vector3<T> & vector)
{
	x = vector.x;
	y = vector.y;
	z = vector.z;
}

template<typename T = double>
Vector3<T>::Vector3(T x, T y, T z)
{
	this->x = x;
	this->y = y;
	this->z = z;
}

template<typename T>
Vector3<T> Vector3<T>::operator+(const Vector3<T> & other) const { return Vector3<T>(x + other.x, y + other.y, x + other.z); }

template<typename T>
template<typename S>
Vector3<T> Vector3<T>::operator*(S scale) const { return Vector3<T>(x * scale, y * scale, z * scale); }

template<typename T>
template<typename S>
Vector3<T> Vector3<T>::operator/(S scale) const { return Vector3<T>(x / scale, y / scale, z / scale); }
	
template<typename T>
std::string Vector3<T>::toString()
{
	return "(" + std::to_string(x) + ", " + std::to_string(y) + ", " + std::to_string(z) + ")";
}

template<typename T>
void Vector3<T>::write(BXDA::BinaryWriter & output) const
{
	output.write(x);
	output.write(y);
	output.write(z);
}
