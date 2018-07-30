#pragma once

#include <string>
#include "BXDA/BinaryWriter.h"
#include "BXDJ/XmlWriter.h"

template<typename T = double>
class Vector3 : public BXDA::BinaryWritable, public BXDJ::XmlWritable
{
public:
	T x;
	T y;
	T z;
		
	Vector3();
	Vector3(const Vector3<T> &);
	Vector3(T x, T y, T z);

	template<typename U> Vector3<T> operator+(const Vector3<U> &) const;
	template<typename U> Vector3<T> operator-(const Vector3<U> &) const;
	template<typename U> T operator*(const Vector3<U> &) const; // Dot product
	template<typename S> Vector3<T> operator*(S) const;
	template<typename S> Vector3<T> operator/(S) const;
	T magnitude() const;
	template<typename U> void getRadialCoordinates(Vector3<U>, Vector3<U>, T &, T &) const;

	std::string toString();

protected:
	void write(BXDA::BinaryWriter &) const;
	void write(BXDJ::XmlWriter &) const;

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
template<typename U>
Vector3<T> Vector3<T>::operator+(const Vector3<U> & other) const { return Vector3<T>(x + other.x, y + other.y, z + other.z); }

template<typename T>
template<typename U>
Vector3<T> Vector3<T>::operator-(const Vector3<U> & other) const { return Vector3<T>(x - other.x, y - other.y, z - other.z); }

template<typename T>
template<typename U>
T Vector3<T>::operator*(const Vector3<U>& other) const { return x*other.x + y*other.y + z*other.z; }

template<typename T>
template<typename S>
Vector3<T> Vector3<T>::operator*(S scale) const { return Vector3<T>(x * scale, y * scale, z * scale); }

template<typename T>
template<typename S>
Vector3<T> Vector3<T>::operator/(S scale) const { return Vector3<T>(x / scale, y / scale, z / scale); }

template<typename T>
T Vector3<T>::magnitude() const { return sqrt(x*x + y*y + z*z); }

template<typename T>
template<typename U>
inline void Vector3<T>::getRadialCoordinates(Vector3<U> axis, Vector3<U> origin, T & distance, T & radius) const
{
	distance = (*this - origin) * axis;
	radius = ((*this - origin) - (axis * distance)).magnitude();
}

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

template<typename T>
void Vector3<T>::write(BXDJ::XmlWriter & output) const
{
	output.writeElement("X", std::to_string(x));
	output.writeElement("Y", std::to_string(y));
	output.writeElement("Z", std::to_string(z));
}
