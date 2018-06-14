#pragma once
#include "Vector3.h" 

//List equivalent for the Vector3 data type
class LVector3 {
public:
	LVector3();
	LVector3(const LVector3*);
	~LVector3();

	LVector3* operator+(const Vector3*) const;		//Add a Vector3

	//add [] operator
	//add >> operator

private:
	Vector3 * Head;

	void add(Vector3*, Vector3*);
};