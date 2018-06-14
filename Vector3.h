#pragma once

class Vector3 {
public:
	Vector3();
	Vector3(float, float, float);			//Overloaded Constructor for first item
	Vector3(Vector3*);
	~Vector3();

	bool add(float, float, float);			//Add a new vector to the dynamic list
private:
	Vector3 * next;
	Vector3 * prev;
	float x;								//X coordinate for the vector
	float y;								//Y coordinate 
	float z;								//Z coordiante
};